using System;
using System.Threading.Tasks;
using Godot;

namespace Petal;

/// <summary>
/// A popover listing things to choose from in a <see cref="RowList"/>, or a "you have none" text when the list is
/// empty (GREENHOUSE.md §3). Picking one closes it. <see cref="SeedPopover"/> and <see cref="FertilizerPopover"/>
/// fill in the rows.
/// </summary>
public abstract partial class ChoicesPopover : Popover
{
	protected RowList Rows { get; private set; }
	private Control _empty;

	public override void _Ready()
	{
		base._Ready();
		Rows = GetNode<RowList>("%Rows");
		_empty = GetNode<Control>("%Empty");
	}

	protected Task ShowRows(Control anchor, float width)
	{
		_empty.Visible = Rows.IsEmpty;
		return ShowBy(anchor, width);
	}

	protected void Pick<T>(Action<T> picked, T choice)
	{
		Hide();
		picked(choice);
	}
}
