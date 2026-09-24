using DeskTown.Application.Configuration;
using DeskTown.Application.Ports;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

public partial class AppRoot : Node
{
    private readonly PrototypeOptions _options = PrototypeOptions.Default;
    private readonly IStructuredLogger _logger = new GodotStructuredLogger();

    public override void _Ready()
    {
        var validation = _options.Validate();
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Errors));
        }

        Engine.MaxFps = _options.VisibleMaxFramesPerSecond;

        _logger.Information(
            "application_started",
            new Dictionary<string, object?>
            {
                ["engine_version"] = Engine.GetVersionInfo()["string"].AsString(),
                ["visible_fps_cap"] = _options.VisibleMaxFramesPerSecond,
                ["logical_tick_seconds"] = _options.LogicalTickInterval.TotalSeconds,
                ["checkpoint_seconds"] = _options.CheckpointInterval.TotalSeconds
            });
    }
}
