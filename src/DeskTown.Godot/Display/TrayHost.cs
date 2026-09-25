using DeskTown.Application.Display;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Native status menu; all commands are forwarded to the application host.</summary>
public partial class TrayHost : Node
{
    private PopupMenu? _menu;
    private StatusIndicator? _indicator;

    public event Action? OpenRequested;
    public event Action? EndFocusRequested;
    public event Action? QuitRequested;
    public event Action<DisplayMode>? ModeRequested;
    public event Action<int>? CompanionScaleRequested;
    public event Action<int>? GhostOpacityRequested;
    public event Action<string>? GhostPositionRequested;
    public event Action? AudioToggleRequested;

    public override void _Ready()
    {
        if (!OperatingSystem.IsWindows()) return;
        _menu = new PopupMenu { Name = "TrayMenu" };
        AddChild(_menu);
        _menu.AddItem("Open DeskTown", 1);
        _menu.AddSeparator();
        _menu.AddItem("Companion", 10);
        _menu.AddItem("Hidden", 11);
        _menu.AddItem("Ghost (Windows QA pending)", 12);
        _menu.SetItemDisabled(_menu.GetItemIndex(12), true);
        _menu.AddSeparator("Companion Scale");
        _menu.AddItem("75%", 20);
        _menu.AddItem("100%", 21);
        _menu.AddItem("125%", 22);
        _menu.AddItem("150%", 23);
        _menu.AddSeparator("Ghost Position");
        _menu.AddItem("Bottom Left", 30);
        _menu.AddItem("Bottom Right", 31);
        _menu.AddItem("Top Left", 32);
        _menu.AddItem("Top Right", 33);
        _menu.AddSeparator("Ghost Opacity");
        _menu.AddItem("40%", 40);
        _menu.AddItem("65%", 41);
        _menu.AddItem("85%", 42);
        _menu.AddSeparator();
        _menu.AddItem("Audio", 50);
        _menu.AddItem("End Focus", 60);
        _menu.AddItem("Quit", 90);
        _menu.IdPressed += id => Dispatch((int)id);

        var image = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
        image.Fill(Color.FromHtml("4b6955"));
        for (var y = 4; y < 12; y++)
            for (var x = 5; x < 11; x++) image.SetPixel(x, y, Color.FromHtml("e8c492"));
        _indicator = new StatusIndicator
        {
            Name = "DeskTownStatus", Icon = ImageTexture.CreateFromImage(image),
            Tooltip = "DeskTown", Visible = true
        };
        AddChild(_indicator);
        _indicator.Menu = _menu.GetPath();
        SetStatus(false, false);
    }

    public void SetStatus(bool focusing, bool workComplete)
    {
        if (_indicator is not null)
            _indicator.Tooltip = focusing ? "DeskTown • Focus"
                : workComplete ? "DeskTown • Work complete" : "DeskTown";
        if (_menu is not null)
            _menu.SetItemDisabled(_menu.GetItemIndex(60), !focusing);
    }

    private void Dispatch(int id)
    {
        switch (id)
        {
            case 1: OpenRequested?.Invoke(); break;
            case 10: ModeRequested?.Invoke(DisplayMode.Companion); break;
            case 11: ModeRequested?.Invoke(DisplayMode.Hidden); break;
            case 12: break; // fail closed until Ghost release gate
            case >= 20 and <= 23: CompanionScaleRequested?.Invoke(new[] { 75, 100, 125, 150 }[id - 20]); break;
            case >= 30 and <= 33:
                GhostPositionRequested?.Invoke(new[] { "BottomLeft", "BottomRight", "TopLeft", "TopRight" }[id - 30]);
                break;
            case >= 40 and <= 42: GhostOpacityRequested?.Invoke(new[] { 40, 65, 85 }[id - 40]); break;
            case 50: AudioToggleRequested?.Invoke(); break;
            case 60: EndFocusRequested?.Invoke(); break;
            case 90: QuitRequested?.Invoke(); break;
        }
    }
}
