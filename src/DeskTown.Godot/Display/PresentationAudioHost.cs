using System.Text.Json;
using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Bounded, visible-surface audio. No simulation or persistence authority.</summary>
public partial class PresentationAudioHost : Node
{
    private enum Surface { Town, Companion, Ghost, Reward, Discovery, Ui }
    private readonly Dictionary<string, AudioStreamOggVorbis> _sounds = new();
    private readonly List<(AudioStreamPlayer Player, Surface Surface)> _voices = new();
    private readonly Dictionary<string, AudioStreamPlayer> _loops = new();
    private readonly Dictionary<string, double> _lastCue = new();
    private Node? _root;
    private bool _enabled;
    private bool _paused;
    private double _seconds;
    private double _birdWait = 8;
    private int _birdIndex;

    public void Configure(Node root)
    {
        _root = root;
        LoadSounds();
        var town = root.GetNode<TownScene>("MainShell/Town");
        ConnectMina(town.GetNode<MinaPlaceholderView>("World/MinaView"), Surface.Town);
        foreach (var name in new[] { "NoahView", "RumiView" })
            town.GetNode<TownNpcPlaceholderView>("World/" + name).WalkingFrameChanged += frame =>
            {
                if (frame is 1 or 4) PlayCue("SND-STP-001", Surface.Town, .7f, .55);
            };
        ConnectMina(root.GetNode<MinaPlaceholderView>("CompanionWindow/Stage/MinaView"), Surface.Companion);
        ConnectMina(root.GetNode<MinaPlaceholderView>("GhostWindow/GhostStage/MinaView"), Surface.Ghost);
        var reward = root.GetNode<RewardReveal>("MainShell/RewardReveal");
        reward.VisibilityChanged += () =>
        {
            if (reward.IsVisibleInTree()) PlayCue("SND-HMR-001", Surface.Reward, .55f, 0);
        };
        reward.WorkshopChanged += () => PlayCue("SND-CMP-001", Surface.Reward, .9f, 0);
        var discovery = root.GetNode<DiscoveryEvent>("MainShell/DiscoveryEvent");
        discovery.VisibilityChanged += () =>
        {
            if (!discovery.IsVisibleInTree()) return;
            StopSurface(Surface.Reward);
            PlayCue("SND-DSC-001", Surface.Discovery, .9f, 0);
        };
        ConnectButtons(root.GetNode("MainShell"));
        ConnectButtons(root.GetNode("CompanionWindow"));
    }

    private void ConnectMina(MinaPlaceholderView mina, Surface surface) => mina.FrameChanged += (clip, frame) =>
    {
        if (clip == MinaAnimationClip.Work && frame == 2)
            PlayCue("SND-HMR-001", surface, .75f, 1.8);
        else if (surface == Surface.Town && clip == MinaAnimationClip.Walk && frame is 1 or 4)
            PlayCue("SND-STP-001", surface, .7f, .55);
    };

    private void ConnectButtons(Node node)
    {
        if (node is BaseButton button) button.Pressed += Click;
        foreach (var child in node.GetChildren()) ConnectButtons(child);
    }

    public void Click() => PlayCue("SND-UI-001", Surface.Ui, .75f, .08);

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled) StopAll();
    }

    public void SetPaused(bool paused)
    {
        _paused = paused;
        if (paused) StopAll();
    }

    public override void _Process(double delta)
    {
        _seconds += delta;
        foreach (var surface in Enum.GetValues<Surface>())
            if (!Allows(surface)) StopSurface(surface);
        var townVisible = Allows(Surface.Town);
        SyncLoop("SND-AMB-001", townVisible, 1);
        SyncLoop("SND-FIR-001", townVisible, .35f);
        if (!townVisible) { _birdWait = 8; return; }
        _birdWait -= delta;
        if (_birdWait <= 0)
        {
            PlayCue("SND-BRD-001", Surface.Town, .65f, 0);
            _birdWait = new[] { 29, 37, 31 }[_birdIndex++ % 3];
        }
    }

    private bool TownWindowVisible => _root is not null && _root.GetWindow().Visible
        && _root.GetWindow().Mode != Window.ModeEnum.Minimized;

    private bool OverlayVisible => _root is not null && new[]
        { "FirstLaunch", "FocusSetup", "RecoveryPrompt", "RewardReveal", "DiscoveryEvent" }
        .Any(name => _root.GetNode<Control>("MainShell/" + name).IsVisibleInTree());

    private bool Allows(Surface surface)
    {
        if (!_enabled || _paused || _root is null || DisplayServer.GetName() == "headless") return false;
        return surface switch
        {
            Surface.Town => TownWindowVisible && !OverlayVisible &&
                _root.GetNode<Control>("MainShell/Town").IsVisibleInTree(),
            Surface.Companion => VisibleWindow("CompanionWindow"),
            Surface.Ghost => VisibleWindow("GhostWindow"),
            Surface.Reward => TownWindowVisible && _root.GetNode<Control>("MainShell/RewardReveal").IsVisibleInTree(),
            Surface.Discovery => TownWindowVisible && _root.GetNode<Control>("MainShell/DiscoveryEvent").IsVisibleInTree(),
            Surface.Ui => TownWindowVisible || VisibleWindow("CompanionWindow"),
            _ => false
        };
    }

    private bool VisibleWindow(string path) => _root!.GetNode<Window>(path) is { Visible: true } window
        && window.Mode != Window.ModeEnum.Minimized;

    private void PlayCue(string id, Surface surface, float gain, double minimumInterval)
    {
        if (!Allows(surface) || !_sounds.TryGetValue(id, out var sound)) return;
        if (_lastCue.TryGetValue(id, out var previous) && _seconds - previous < minimumInterval) return;
        // Reuse four voices. No queued catch-up and no new player per footstep.
        var index = _voices.FindIndex(v => !v.Player.Playing);
        if (index < 0)
        {
            if (_voices.Count >= 4) return;
            _voices.Add((NewPlayer("Cue" + _voices.Count), surface));
            index = _voices.Count - 1;
        }
        var player = _voices[index].Player;
        _voices[index] = (player, surface);
        player.Stream = sound;
        player.VolumeDb = Mathf.LinearToDb(gain);
        player.Play();
        _lastCue[id] = _seconds;
    }

    private AudioStreamPlayer NewPlayer(string name)
    {
        var player = new AudioStreamPlayer { Name = name, MaxPolyphony = 1 };
        AddChild(player);
        return player;
    }

    private void SyncLoop(string id, bool enabled, float gain)
    {
        if (!_sounds.TryGetValue(id, out var sound)) return;
        if (!_loops.TryGetValue(id, out var player))
        {
            player = NewPlayer(id);
            player.Stream = sound;
            player.VolumeDb = Mathf.LinearToDb(gain);
            _loops.Add(id, player);
        }
        if (!enabled) player.Stop();
        else if (!player.Playing) player.Play();
    }

    private void StopSurface(Surface surface)
    {
        foreach (var voice in _voices)
            if (voice.Surface == surface) voice.Player.Stop();
        if (surface == Surface.Town)
            foreach (var loop in _loops.Values) loop.Stop();
    }

    private void StopAll()
    {
        foreach (var voice in _voices) voice.Player.Stop();
        foreach (var loop in _loops.Values) loop.Stop();
        _birdWait = 8;
    }

    public override void _ExitTree()
    {
        StopAll();
        foreach (var player in _voices.Select(v => v.Player).Concat(_loops.Values))
            player.Stream = null;
        foreach (var sound in _sounds.Values) sound.Dispose();
        _sounds.Clear();
        _voices.Clear();
        _loops.Clear();
    }

    public async Task PrepareQuitAsync()
    {
        SetEnabled(false);
        SetProcess(false);
        foreach (var player in _voices.Select(v => v.Player).Concat(_loops.Values))
            player.Stream = null;
        // Let the audio mixer release stopped playbacks before engine teardown.
        await ToSignal(GetTree().CreateTimer(.2), SceneTreeTimer.SignalName.Timeout);
    }

    private void LoadSounds()
    {
        try
        {
            using var file = global::Godot.FileAccess.Open("res://assets/catalogs/asset_catalog.json", global::Godot.FileAccess.ModeFlags.Read);
            if (file is null) return;
            using var json = JsonDocument.Parse(file.GetAsText());
            foreach (var row in json.RootElement.GetProperty("assets").EnumerateArray())
            {
                if (row.GetProperty("kind").GetString() != "audio" ||
                    row.GetProperty("status").GetString() is not ("production" or "placeholder")) continue;
                var resource = row.GetProperty("resource").GetString();
                if (resource is null || !resource.StartsWith("res://assets/art/", StringComparison.Ordinal)) continue;
                var stream = ResourceLoader.Load<AudioStreamOggVorbis>(resource);
                if (stream is null) continue;
                var id = row.GetProperty("id").GetString()!;
                stream.Loop = id is "SND-AMB-001" or "SND-FIR-001";
                stream.LoopOffset = 0;
                _sounds.Add(id, stream);
            }
        }
        catch (Exception)
        {
            _sounds.Clear(); // Optional missing audio is silent; no gameplay failure.
        }
    }
}
