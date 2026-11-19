using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// Something shown over a screen: a window, a question or a popup. Back closes the topmost one. By default it covers
/// the whole screen, so taps on the screen behind it close it; <see cref="Popover"/> lets taps through and closes
/// itself instead.
/// A dim behind its content is a <c>DimPanel</c> child that ignores the mouse.
/// </summary>
public partial class Overlay : Control
{
	public override void _GuiInput(InputEvent input)
	{
		if (input is InputEventMouseButton { Pressed: true })
			Hide();
	}

	/// <summary>Hides the last visible overlay among <paramref name="parent"/>'s children, the one drawn on top.</summary>
	/// <returns>False when none was open.</returns>
	public static bool CloseTopmost(Node parent)
	{
		var top = parent.GetChildren().OfType<Overlay>().LastOrDefault(o => o.Visible);
		top?.Hide();
		return top != null;
	}
}
