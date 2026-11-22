using Godot;

namespace Petal;

/// <summary>
/// Rows with a thin line between them (GREENHOUSE.md §2, §3), as in the choices popups and the inventory. Each row
/// scene has a <c>%Line</c>, hidden on the first row.
/// </summary>
public partial class RowList : VBoxContainer
{
	[Export] private PackedScene _rowScene;

	public bool IsEmpty => GetChildCount() == 0;

	public void Clear()
	{
		foreach (var row in GetChildren())
		{
			RemoveChild(row);
			row.QueueFree();
		}
	}

	public T AddRow<T>() where T : Control
	{
		var row = _rowScene.Instantiate<T>();
		row.GetNode<Control>("%Line").Visible = !IsEmpty;
		AddChild(row);
		return row;
	}
}
