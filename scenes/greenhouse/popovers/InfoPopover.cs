using Godot;

namespace Petal;

/// <summary>
/// What is growing in a held pot (GAMEPLAY.md §4.1), shown diagonally off the pot's corner that faces the middle of
/// the screen.
/// </summary>
public partial class InfoPopover : Popover
{
	/// <summary>How far into the pot the popover's corner sits, sideways and up or down.</summary>
	private const float InsetX = 15;
	private const float InsetY = 40;

	private Label _text;

	public override void _Ready()
	{
		base._Ready();
		_text = GetNode<Label>("%Text");
	}

	/// <param name="text">Already translated; may span several lines.</param>
	public void ShowText(Control anchor, string text)
	{
		_text.Text = text;
		ShowBy(anchor, Panel.GetCombinedMinimumSize().X);
	}

	protected override Vector2 PositionBy(Rect2 anchor, Vector2 size)
	{
		var middle = GetViewportRect().Size / 2;
		bool right = anchor.GetCenter().X < middle.X;
		bool down = anchor.GetCenter().Y < middle.Y;
		return new Vector2(
			right ? anchor.End.X - InsetX : anchor.Position.X + InsetX - size.X,
			down ? anchor.End.Y - InsetY : anchor.Position.Y + InsetY - size.Y);
	}
}
