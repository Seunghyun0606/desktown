using DeskTown.Application.Display;
using DeskTown.Domain.Simulation;
using DeskTown.Domain.Projects;
using DeskTown.Presentation.Display;
using global::Godot;

namespace DeskTown.Presentation;

public partial class FocusSetup : Control
{
    public event Action<TimeSpan, IReadOnlyCollection<string>, DisplayMode, int>? StartRequested;
    public event Action? BackRequested;

    public override void _Ready()
    {
        CatalogUiArt.Panel(this);
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
        foreach (var (index, icon) in new[] { (0, 2), (2, 3), (3, 4) })
            if (CatalogUiArt.Texture("UI-ICO-001", icon) is { } texture)
                mode.SetItemIcon(index, texture);
        GetNode<Button>("Panel/Content/Start").Icon = CatalogUiArt.Texture("UI-ICO-001", 0);
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
        var restored = town.Workshop == WorkshopState.Complete;
        GetNode<Label>("Panel/Content/Title").Text = restored ? "Focus with Mina" : "RESTORE WORKSHOP";
        GetNode<Label>("Panel/Content/Npc").Text = restored
            ? "The Workshop is restored. Stay a while."
            : "Mina wants to restore the abandoned workshop.";
        GetNode<Label>("Panel/Content/Progress").Text =
            restored ? "Workshop restored"
                : $"Restore Workshop  {Math.Floor(town.Progress.CountedDuration.TotalMinutes):0} / 25 Focus";
        GetNode<Label>("Panel/Content/Equal").Text = restored
            ? "Every display choice counts the same Focus."
            : "Every display choice builds the same Workshop.";
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
