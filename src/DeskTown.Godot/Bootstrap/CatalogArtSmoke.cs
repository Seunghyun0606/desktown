using DeskTown.Domain.Simulation;
using DeskTown.Presentation.Display;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

/// <summary>CI-only scene lookup with synthetic catalog assignments.</summary>
internal static class CatalogArtSmoke
{
    public static void Run(SceneTree tree)
    {
        try
        {
            var idle = VisualAssetCatalog.Frame("CHR-MIN-001", 3);
            Require(idle is not null && idle.Value.Source.Position.X == 144 &&
                idle.Value.Source.Size == new Vector2(48, 48), "Mina idle frame missing");
            Require(VisualAssetCatalog.Frame("CHR-MIN-001", 4) is null &&
                VisualAssetCatalog.Frame("CHR-MIN-002") is null, "Invalid clip did not fall back");

            var town = ResourceLoader.Load<PackedScene>("res://scenes/town/TownScene.tscn")
                ?? throw new InvalidDataException("Town scene missing");
            var townView = town
                .Instantiate<DeskTown.Presentation.TownScene>();
            tree.Root.AddChild(townView);
            townView.Bind(TownSimulation.Create().Snapshot.Town);
            var house = townView.GetNode<ColorRect>("World/House");
            Require(house.GetNode<Sprite2D>("CatalogArt").Visible &&
                house.Color.A == 0 && !house.GetNode<Label>("Name").Visible,
                "Assigned Town house art did not replace fallback");
            var library = townView.GetNode<ColorRect>("World/LockedArea");
            Require(library.Color.A == 1 && library.GetNode<Label>("Name").Visible,
                "Unassigned Town art did not keep fallback");
            var workshop = townView.GetNode<ColorRect>("World/Workshop");
            var label = workshop.GetNode<Label>("State");
            foreach (var frame in new[] { 0, 1, 2 })
            {
                CatalogRectArt.Bind(workshop, "BLD-WRK-001", frame, label);
                Require(workshop.GetNode<Sprite2D>("CatalogArt").RegionRect.Position.X == frame * 160 &&
                    !label.Visible, "Workshop state region is incorrect");
            }
            CatalogRectArt.Bind(workshop, "BLD-WRK-001", 3, label);
            Require(workshop.Color.A == 1 && label.Visible &&
                !workshop.GetNode<Sprite2D>("CatalogArt").Visible,
                "Invalid frame did not restore fallback");

            var companion = ResourceLoader.Load<PackedScene>("res://scenes/companion/CompanionStage.tscn")
                ?? throw new InvalidDataException("Companion scene missing");
            var companionView = companion
                .Instantiate<CompanionStage>();
            tree.Root.AddChild(companionView);
            var window = companionView.GetNode<ColorRect>("SkyWindow");
            Require(window.Color.A == 0 && window.GetNode<Sprite2D>("CatalogArt").Visible,
                "Assigned Companion art did not replace fallback");
            Require(companionView.GetNode<ColorRect>("Stool").Color.A == 1,
                "Unassigned Companion art did not keep fallback");
            GD.Print("DESKTOWN_CI_ART_SMOKE_OK");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PrintErr($"DESKTOWN_CI_ART_SMOKE_FAILED:{error.GetType().Name}: {error.Message}");
            tree.Quit(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
