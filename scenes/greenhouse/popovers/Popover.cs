using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Petal;

/// <summary>
/// Base for the greenhouse's small popups that appear by something held down (GAMEPLAY.md §4.1, §4.2): a panel kept
/// on screen that closes when you tap anywhere else. It lets the mouse through and watches taps itself (rather than
/// <see cref="Overlay"/>'s tap-to-close), so a tap on <see cref="TapThrough"/> also reaches what's under it; any
/// other tap outside the panel only closes it. <see cref="ChoicesPopover"/> and <see cref="InfoPopover"/> fill it.
/// </summary>
public abstract partial class Popover : Overlay
{
	private const float Gap = 6;
	private const float ScreenMargin = 16;

	/// <summary>Controls that still get the tap that closes the popover, e.g. the tools, so one tap switches tool.</summary>
	public Control[] TapThrough { get; set; } = Array.Empty<Control>();

	protected PanelContainer Panel { get; private set; }

	public override void _Ready()
	{
		Panel = GetNode<PanelContainer>("%Panel");
		MouseFilter = MouseFilterEnum.Ignore;
	}

	public override void _Input(InputEvent input)
	{
		if (!Visible || input is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } press) return;
		if (Panel.GetGlobalRect().HasPoint(press.Position)) return;
		Hide();
		if (!TapThrough.Any(c => c.IsVisibleInTree() && c.GetGlobalRect().HasPoint(press.Position)))
			GetViewport().SetInputAsHandled();
	}

	/// <summary>Where the panel goes by the anchor, before it's kept on screen: centred below it by default.</summary>
	protected virtual Vector2 PositionBy(Rect2 anchor, Vector2 size) =>
		new(anchor.GetCenter().X - size.X / 2, anchor.End.Y + Gap);

	/// <summary>Shows the panel at <paramref name="width"/>, as tall as its content, by the anchor and on screen.</summary>
	protected async Task ShowBy(Control anchor, float width)
	{
		Panel.Size = new Vector2(width, 0);

		// Place it once the content has been laid out at that width, so it lands in the same spot every time;
		// until then it's invisible. A control never shrinks by itself, so trim any height left over from before.
		Panel.Modulate = Colors.Transparent;
		Show();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if (!IsInstanceValid(this) || !Visible) return;
		Panel.Size = new Vector2(width, Panel.GetCombinedMinimumSize().Y);

		var size = Panel.Size;
		var screen = GetViewportRect().Size;
		var position = PositionBy(anchor.GetGlobalRect(), size);
		Panel.GlobalPosition = new Vector2(
			Mathf.Clamp(position.X, ScreenMargin, screen.X - size.X - ScreenMargin),
			Mathf.Clamp(position.Y, ScreenMargin, screen.Y - size.Y - ScreenMargin));
		Panel.Modulate = Colors.White;
	}
}
