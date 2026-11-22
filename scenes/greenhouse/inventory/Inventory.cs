using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// Today's inventory of harvested flowers (GAMEPLAY.md §4.5, GREENHOUSE.md §2), shown over the greenhouse: a row per
/// flower with an item for each colour and quality.
/// </summary>
public partial class Inventory : Overlay
{
	[Export] private PackedScene _itemScene;

	private RowList _rows;
	private ScrollContainer _scroll;
	private Control _empty;

	public override void _Ready()
	{
		_rows = GetNode<RowList>("%Rows");
		_scroll = GetNode<ScrollContainer>("%Scroll");
		_empty = GetNode<Control>("%Empty");
		GetNode<Button>("%CloseButton").Pressed += Hide;
	}

	public void Open(IEnumerable<(Stem Stem, int Count)> inventory)
	{
		_rows.Clear();
		foreach (var flower in inventory.GroupBy(s => s.Stem.Kind))
		{
			var row = _rows.AddRow<ItemRow>();
			row.Heading = flower.First().Stem.Flower.Name;
			foreach (var (stem, count) in flower)
			{
				var item = _itemScene.Instantiate<InventoryItem>();
				row.Items.AddChild(item);
				item.SetItem(stem, count);
			}
		}

		_empty.Visible = _rows.IsEmpty;
		_scroll.Visible = !_rows.IsEmpty;
		_scroll.ScrollVertical = 0;
		Show();
	}
}
