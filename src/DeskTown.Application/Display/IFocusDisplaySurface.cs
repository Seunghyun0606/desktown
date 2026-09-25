namespace DeskTown.Application.Display;

/// <summary>A visual surface only. Focus and simulation never depend on it.</summary>
public interface IFocusDisplaySurface
{
    void Show();
    void Hide();
}
