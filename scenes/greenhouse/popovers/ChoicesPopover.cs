using System;
using System.Threading.Tasks;
using Godot;

namespace Petal;

/// <summary>
/// A popover listing things to choose from, a row each with a thin line between rows, or a "you have none" text when
/// the list is empty (GREENHOUSE.md §3). Picking one closes it. <see cref="SeedPopover"/> and
/// <see cref="FertilizerPopover"/> fill in the rows.
/// </summary>
public abstract partial class ChoicesPopover : Popover
{
	[Export] private PackedScene _rowScene;

	protected Container Rows { get; private set; }
	private Control _empty;

	public override void _Ready()
	{
		base._Ready();
		Rows = GetNode<Container>("%Rows");
		_empty = GetNode<Control>("%Empty");
	}

	protected void ClearRows()
	{
		foreach (var row in Rows.GetChildren())
		{
			Rows.RemoveChild(row);
			row.QueueFree();
		}
	}

	protected Control AddRow()
	{
		var row = _rowScene.Instantiate<Control>();
		row.GetNode<Control>("%Line").Visible = Rows.GetChildCount() > 0;
		Rows.AddChild(row);
		return row;
	}

	protected Task ShowRows(Control anchor, float width)
	{
		_empty.Visible = Rows.GetChildCount() == 0;
		return ShowBy(anchor, width);
	}

	protected void Pick<T>(Action<T> picked, T choice)
	{
		Hide();
		picked(choice);
	}
}
