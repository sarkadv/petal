using Godot;

namespace Petal;

/// <summary>
/// A row of items under a flower's name that scrolls sideways (GREENHOUSE.md §2, §3), in the seed popover and the
/// inventory. Goes in a <see cref="RowList"/>.
/// </summary>
public partial class ItemRow : VBoxContainer
{
	private Label _heading;

	/// <summary>The flower's English name, which the label translates.</summary>
	public string Heading { get => _heading.Text; set => _heading.Text = value; }

	public ScrollContainer Scroll { get; private set; }
	public Container Items { get; private set; }

	public override void _Ready()
	{
		_heading = GetNode<Label>("%Heading");
		Scroll = GetNode<ScrollContainer>("%Scroll");
		Items = GetNode<Container>("%Items");
	}
}
