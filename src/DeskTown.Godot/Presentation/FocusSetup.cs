using DeskTown.Application.Display;
using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation;

public partial class FocusSetup : Control
{
    public event Action<TimeSpan, IReadOnlyCollection<string>, DisplayMode, int>? StartRequested;
    public event Action? BackRequested;

    public override void _Ready()
    {
        var duration = GetNode<OptionButton>("Panel/Content/Duration");
        foreach (var minutes in new[] { 25, 45, 60 }) duration.AddItem($"{minutes} minutes", minutes);
        var apps = GetNode<OptionButton>("Panel/Content/App");
        apps.AddItem("Any App", 0);
        var mode = GetNode<OptionButton>("Panel/Content/Mode");
        mode.AddItem("PiP Companion", (int)DisplayMode.Companion);
        mode.AddItem("Minimize to taskbar", (int)DisplayMode.Minimized);
        mode.AddItem("Hide completely (Tray only)", (int)DisplayMode.Hidden);
        mode.AddItem("Ghost (Windows QA pending)", (int)DisplayMode.Ghost);
        mode.SetItemDisabled(3, true);
        var scale = GetNode<OptionButton>("Panel/Content/CompanionScale");
        foreach (var percent in new[] { 75, 100, 125, 150 })
            scale.AddItem($"{percent}%", percent);
        mode.ItemSelected += _ =>
            scale.Disabled = (DisplayMode)mode.GetSelectedId() != DisplayMode.Companion;
        GetNode<Button>("Panel/Content/Start").Pressed += () =>
        {
            var chosen = apps.Selected == 0 ? Array.Empty<string>()
                : new[] { apps.GetItemText(apps.Selected) };
            StartRequested?.Invoke(TimeSpan.FromMinutes(duration.GetSelectedId()), chosen,
                (DisplayMode)mode.GetSelectedId(), scale.GetSelectedId());
        };
        GetNode<Button>("Panel/Content/Back").Pressed += () => BackRequested?.Invoke();
    }

    public void Bind(TownProjection town, IReadOnlyList<string> processNames,
        DisplayMode savedMode, int companionScalePercent)
    {
        GetNode<Label>("Panel/Content/Progress").Text =
            $"Restore Workshop  {Math.Floor(town.Progress.CountedDuration.TotalMinutes):0} / 25 Focus";
        var apps = GetNode<OptionButton>("Panel/Content/App");
        apps.Clear();
        apps.AddItem("Any App", 0);
        foreach (var name in processNames) apps.AddItem(name);
        apps.Select(0);
        var mode = GetNode<OptionButton>("Panel/Content/Mode");
        var modeIndex = mode.GetItemIndex((int)savedMode);
        mode.Select(modeIndex >= 0 && savedMode != DisplayMode.Ghost
            ? modeIndex : mode.GetItemIndex((int)DisplayMode.Hidden));
        var scale = GetNode<OptionButton>("Panel/Content/CompanionScale");
        var selected = scale.GetItemIndex(companionScalePercent);
        if (selected >= 0) scale.Select(selected);
        scale.Disabled = (DisplayMode)mode.GetSelectedId() != DisplayMode.Companion;
    }
}
