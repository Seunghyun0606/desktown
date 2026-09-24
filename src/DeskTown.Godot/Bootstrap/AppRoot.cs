using DeskTown.Application.Configuration;
using DeskTown.Application.Ports;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

public partial class AppRoot : Node
{
    private readonly PrototypeOptions _configuredOptions = PrototypeOptions.Default;
    private readonly IStructuredLogger _logger = new GodotStructuredLogger();

    public override void _Ready()
    {
        var resolution = PrototypeOptionsResolver.Resolve(_configuredOptions);
        var options = resolution.Options;

        if (resolution.UsedFallback)
        {
            GetNode<Label>("MainShell/Center/Message").Text =
                "DeskTown\nFoundation ready\nDefault configuration restored";

            _logger.Information(
                "prototype_options_fallback",
                new Dictionary<string, object?>
                {
                    ["validation_error_count"] = resolution.ValidationErrors.Count
                });
        }

        Engine.MaxFps = options.VisibleMaxFramesPerSecond;

        _logger.Information(
            "application_started",
            new Dictionary<string, object?>
            {
                ["engine_version"] = Engine.GetVersionInfo()["string"].AsString(),
                ["visible_fps_cap"] = options.VisibleMaxFramesPerSecond,
                ["logical_tick_seconds"] = options.LogicalTickInterval.TotalSeconds,
                ["checkpoint_seconds"] = options.CheckpointInterval.TotalSeconds
            });
    }
}
