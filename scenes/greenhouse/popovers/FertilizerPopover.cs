using System;
using System.Collections.Generic;
using Godot;

namespace Petal;

/// <summary>
/// The fertilizers to choose from when Fertilize is held (GAMEPLAY.md §4.2, GREENHOUSE.md §3), shown below the tool:
/// a row for each fertilizer in stock with its icon, name and count. Tapping a row closes it and raises
/// <see cref="FertilizerPicked"/>.
/// </summary>
public partial class FertilizerPopover : ChoicesPopover
{
	private const float Width = 440;

	public event Action<FertilizerData> FertilizerPicked;

	public async void ShowFertilizers(
		Control anchor, IEnumerable<(FertilizerData Fertilizer, int Count)> fertilizers, FertilizerData chosen)
	{
		Rows.Clear();
		foreach (var (fertilizer, count) in fertilizers)
		{
			var row = Rows.AddRow<Control>();
			var button = row.GetNode<Button>("%Button");
			button.Text = fertilizer.Name;
			button.Icon = fertilizer.Icon;
			button.ButtonPressed = fertilizer == chosen;
			button.Pressed += () => Pick(fertilizer);
			row.GetNode<CountBadge>("%Badge").SetCount(count);
		}
		await ShowRows(anchor, Width);
	}

	private void Pick(FertilizerData fertilizer)
	{
		Hide();
		FertilizerPicked?.Invoke(fertilizer);
	}
}
