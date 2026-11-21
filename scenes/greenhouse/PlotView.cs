using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// One greenhouse plot on screen, a terracotta pot seen from above (GAMEPLAY.md §4.1): the plants at their
/// stage, dark wet soil once watered (dry soil means it still needs water), and soil tinted by its fertilizer.
/// Holding it shows what's growing (see Greenhouse). After a tool is used on it, the change shows when the tool
/// reaches the soil (see PotEffects).
/// </summary>
public partial class PlotView : Button
{
	[Export] private Texture2D _seedling;
	[Export] private Texture2D _bud;
	[Export] private Texture2D _leaves;
	[Export] private Texture2D _withered;
	[Export] private Texture2D _dead;

	private readonly List<(TextureRect Plant, TextureRect Bloom)> _plants = new();
	private Control _plantsNode;
	private TextureRect _wetSoil;
	private TextureRect _fertilizedSoil;

	public Plot Plot { get; set; }

	/// <summary>Where the flower heads are drawn, also while hidden.</summary>
	public IEnumerable<Control> Blooms => _plants.Select(p => (Control)p.Bloom);

	public override void _Ready()
	{
		_plantsNode = GetNode<Control>("%Plants");
		foreach (var plant in _plantsNode.GetChildren())
			_plants.Add(((TextureRect)plant, plant.GetNode<TextureRect>("Bloom")));
		_wetSoil = GetNode<TextureRect>("%WetSoil");
		_fertilizedSoil = GetNode<TextureRect>("%FertilizedSoil");
	}

	public void Refresh()
	{
		var plot = Plot;
		var plantTexture = plot.Stage switch
		{
			PlotStage.Seedling => _seedling,
			PlotStage.Growing => _bud,
			PlotStage.Ready => _leaves,
			PlotStage.Withered => _withered,
			PlotStage.Dead => _dead,
			_ => null,
		};
		bool ready = plot.Stage == PlotStage.Ready;
		var tint = ready ? plot.Seed!.Value.Colour.Tint() : Colors.White;
		foreach (var (plant, bloom) in _plants)
		{
			plant.Texture = plantTexture;
			bloom.Visible = ready;
			bloom.SelfModulate = tint;
		}

		_wetSoil.Visible = plot.Watered;
		_fertilizedSoil.Visible = plot.Fertilizer.HasValue;
		if (plot.Fertilizer is { } fertilizer)
			_fertilizedSoil.SelfModulate = GameData.Instance.Fertilizer(fertilizer).SoilTint;
	}

	public void ShowWatered(float delay) => FadeIn(_wetSoil, delay);

	public void ShowFertilized(float delay) => FadeIn(_fertilizedSoil, delay);

	/// <summary>The new sprouts grow up out of the soil one after another.</summary>
	public void ShowPlanted(float delay)
	{
		for (int i = 0; i < _plants.Count; i++)
		{
			var plant = _plants[i].Plant;
			plant.PivotOffset = plant.Size / 2;
			plant.Scale = Vector2.Zero;
			CreateTween().TweenProperty(plant, "scale", Vector2.One, 0.3f)
				.SetDelay(delay + i * 0.06f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		}
	}

	/// <summary>A copy of the plants as they are now, to animate after they've gone from the pot.</summary>
	public Control CopyPlants(bool blooms = true)
	{
		var copy = (Control)_plantsNode.Duplicate();
		copy.UniqueNameInOwner = false;
		if (!blooms)
			foreach (var plant in copy.GetChildren())
				plant.GetNode<CanvasItem>("Bloom").Hide();
		return copy;
	}

	private void FadeIn(CanvasItem soil, float delay)
	{
		soil.Modulate = Colors.Transparent;
		CreateTween().TweenProperty(soil, "modulate:a", 1f, 0.25f).SetDelay(delay);
	}
}
