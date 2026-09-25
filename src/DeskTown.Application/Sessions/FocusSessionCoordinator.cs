using DeskTown.Application.Activity;
using DeskTown.Application.Ports;
using DeskTown.Domain.Focus;

namespace DeskTown.Application.Sessions;

/// <summary>
/// Coordinates one complete Focus command flow without knowing about rendering
/// or display modes. Activity observations are descriptive only; the manager's
/// monotonic clock remains the sole source of counted progression.
/// </summary>
public sealed class FocusSessionCoordinator
{
    private readonly FocusSessionManager _manager;
    private readonly IActivityTracker _activityTracker;
    private readonly SessionLedger _ledger;
    private readonly IEnergyPolicy _energyPolicy;
    private readonly ActivityAccumulator _activity = new();

    public FocusSessionCoordinator(
        FocusSessionManager manager,
        IActivityTracker activityTracker,
        SessionLedger ledger,
        IEnergyPolicy energyPolicy)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        _energyPolicy = energyPolicy ?? throw new ArgumentNullException(nameof(energyPolicy));

        if (_activityTracker.SampleInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(activityTracker),
                "The activity tracker sample interval must be positive.");
        }
    }

    public event Action<FocusSessionCheckpoint>? CheckpointAvailable;

    public event Action<FocusSessionFinalized>? SessionFinalized;

    public FocusSession? CurrentSession => _manager.CurrentSession;

    public FocusSession? ActiveSession => _manager.ActiveSession;

    public FocusEnergy TotalEnergy => _energyPolicy.CalculateTotal(_ledger);

    public FocusActivitySummary CurrentActivity => _activity.CreateSnapshot();

    public FocusSession StartFocus(
        FocusSessionId sessionId,
        TimeSpan targetDuration,
        IEnumerable<string>? intendedProcessNames = null)
    {
        var session = FocusSession.Create(sessionId, targetDuration, intendedProcessNames);
        _manager.Start(session);
        _activity.Reset();
        PublishCheckpoint(session);
        return session;
    }

    public FocusSession RestoreSuspended(FocusSessionCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        if (checkpoint.Status is not (FocusSessionStatus.Running or FocusSessionStatus.Suspended)
            || checkpoint.StartedAtUtc is null || checkpoint.EndedAtUtc is not null
            || checkpoint.TotalRecordedEnergy != TotalEnergy)
            throw new ArgumentException("Invalid recoverable Focus checkpoint.", nameof(checkpoint));

        var session = FocusSession.RestoreSuspended(checkpoint.SessionId,
            checkpoint.StartedAtUtc.Value, checkpoint.TargetDuration,
            checkpoint.CountedDuration, checkpoint.IdleDuration,
            checkpoint.IntendedProcessNames);
        _manager.RestoreSuspended(session);
        _activity.Restore(checkpoint.Activity);
        PublishCheckpoint(session);
        return session;
    }

    public FocusSessionTickResult Tick()
    {
        var session = RequireActiveSession();
        if (session.Status == FocusSessionStatus.Suspended)
        {
            return _manager.Tick(TimeSpan.Zero);
        }

        var sample = SampleSafely();
        var result = _manager.Tick(sample.IdleDuration > TimeSpan.Zero);
        _activity.Record(sample, result.AppliedElapsed, result.AppliedIdle);

        if (result.Completed)
        {
            FinalizeSession(session);
        }
        else
        {
            PublishCheckpoint(session);
        }

        return result;
    }

    public FocusSessionStatus Suspend()
    {
        var session = RequireActiveSession();
        if (session.Status != FocusSessionStatus.Running)
        {
            throw new FocusSessionManagerException("Only a running focus session can be suspended.");
        }

        var sample = SampleSafely();
        var before = session.CountedDuration;
        var idleBefore = session.IdleDuration;
        var status = _manager.Suspend(sample.IdleDuration > TimeSpan.Zero);
        _activity.Record(
            sample,
            session.CountedDuration - before,
            session.IdleDuration - idleBefore);

        if (status == FocusSessionStatus.Completed)
        {
            FinalizeSession(session);
        }
        else
        {
            PublishCheckpoint(session);
        }

        return status;
    }

    public FocusSession Resume()
    {
        var session = _manager.Resume();
        PublishCheckpoint(session);
        return session;
    }

    public FocusSessionFinalized Stop()
    {
        var session = RequireActiveSession();
        ActivitySample? sample = null;
        var before = session.CountedDuration;
        var idleBefore = session.IdleDuration;

        if (session.Status == FocusSessionStatus.Running)
        {
            sample = SampleSafely();
        }

        var stopped = _manager.Stop(sample?.IdleDuration > TimeSpan.Zero);
        if (sample is not null)
        {
            _activity.Record(
                sample,
                stopped.CountedDuration - before,
                stopped.IdleDuration - idleBefore);
        }

        return FinalizeSession(stopped);
    }

    private FocusSession RequireActiveSession()
    {
        return _manager.ActiveSession
            ?? throw new FocusSessionManagerException("There is no active focus session.");
    }

    private ActivitySample SampleSafely()
    {
        try
        {
            return _activityTracker.Sample() ?? ActivitySample.Unknown;
        }
        catch (Exception exception) when (IsRecoverableTrackerFailure(exception))
        {
            return ActivitySample.Unknown;
        }
    }

    private FocusSessionFinalized FinalizeSession(FocusSession session)
    {
        var newlyRecorded = _ledger.TryRecord(session);
        var totalEnergy = _energyPolicy.CalculateTotal(_ledger);
        var checkpoint = CreateCheckpoint(session, totalEnergy);
        var finalized = new FocusSessionFinalized(checkpoint, newlyRecorded, totalEnergy);

        CheckpointAvailable?.Invoke(checkpoint);
        SessionFinalized?.Invoke(finalized);
        return finalized;
    }

    private void PublishCheckpoint(FocusSession session)
    {
        CheckpointAvailable?.Invoke(CreateCheckpoint(session, TotalEnergy));
    }

    private FocusSessionCheckpoint CreateCheckpoint(
        FocusSession session,
        FocusEnergy totalRecordedEnergy)
    {
        return new FocusSessionCheckpoint(
            session.Id,
            session.Status,
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.TargetDuration,
            session.CountedDuration,
            session.IdleDuration,
            session.IntendedProcessNames,
            _activity.CreateSnapshot(),
            totalRecordedEnergy);
    }

    private static bool IsRecoverableTrackerFailure(Exception exception)
    {
        return exception is ArgumentException
            or InvalidOperationException
            or System.ComponentModel.Win32Exception
            or DllNotFoundException
            or EntryPointNotFoundException
            or PlatformNotSupportedException
            or UnauthorizedAccessException;
    }

    private sealed class ActivityAccumulator
    {
        private readonly Dictionary<string, TimeSpan> _processDurations =
            new(StringComparer.OrdinalIgnoreCase);
        private TimeSpan _activeDuration;
        private TimeSpan _idleDuration;
        private TimeSpan _unknownDuration;

        public void Reset()
        {
            _processDurations.Clear();
            _activeDuration = TimeSpan.Zero;
            _idleDuration = TimeSpan.Zero;
            _unknownDuration = TimeSpan.Zero;
        }

        public void Restore(FocusActivitySummary summary)
        {
            ArgumentNullException.ThrowIfNull(summary);
            if (summary.ActiveDuration < TimeSpan.Zero || summary.IdleDuration < TimeSpan.Zero
                || summary.UnknownDuration < TimeSpan.Zero || summary.Processes is null
                || summary.Processes.Any(p => p.Duration < TimeSpan.Zero))
                throw new ArgumentException("Invalid activity summary.", nameof(summary));

            Reset();
            _activeDuration = summary.ActiveDuration;
            _idleDuration = summary.IdleDuration;
            _unknownDuration = summary.UnknownDuration;
            foreach (var process in summary.Processes)
            {
                _processDurations.TryGetValue(process.ProcessName, out var duration);
                _processDurations[process.ProcessName] = duration + process.Duration;
            }
        }

        public void Record(
            ActivitySample sample,
            TimeSpan appliedDuration,
            TimeSpan appliedIdle)
        {
            if (appliedDuration <= TimeSpan.Zero)
            {
                return;
            }

            if (sample.IdleDuration > TimeSpan.Zero)
            {
                _idleDuration += appliedIdle;
                _unknownDuration += appliedDuration - appliedIdle;
            }
            else if (sample.ActiveDuration > TimeSpan.Zero)
            {
                var observedActive = sample.ActiveDuration <= appliedDuration
                    ? sample.ActiveDuration
                    : appliedDuration;
                _activeDuration += observedActive;
                _unknownDuration += appliedDuration - observedActive;
            }
            else
            {
                _unknownDuration += appliedDuration;
            }

            var sampleDuration = sample.IdleDuration > TimeSpan.Zero
                ? sample.IdleDuration
                : sample.ActiveDuration;
            var observedDuration = sampleDuration <= appliedDuration
                ? sampleDuration
                : appliedDuration;
            AddProcessDuration(sample.ProcessName, observedDuration);
            AddProcessDuration(
                ActivitySample.UnknownProcessName,
                appliedDuration - observedDuration);
        }

        private void AddProcessDuration(string processName, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                return;
            }

            _processDurations.TryGetValue(processName, out var current);
            _processDurations[processName] = current + duration;
        }

        public FocusActivitySummary CreateSnapshot()
        {
            var processes = _processDurations
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(pair => new ProcessActivityDuration(pair.Key, pair.Value))
                .ToArray();

            return new FocusActivitySummary(
                _activeDuration,
                _idleDuration,
                _unknownDuration,
                Array.AsReadOnly(processes));
        }
    }
}
