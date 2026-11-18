using System.Collections.Generic;
using Godot;

namespace Petal;

/// <summary>
/// One greenhouse plot on screen, a terracotta pot seen from above (GAMEPLAY.md §4.1): the plants at their
/// stage, dark wet soil once watered (dry soil means it still needs water), and soil tinted by its fertilizer.
/// Holding it shows what's growing (see Greenhouse).
/// </summary>
public partial class PlotView : Button
{
	[Export] private Texture2D _seedling;
	[Export] private Texture2D _bud;
	[Export] private Texture2D _leaves;
	[Export] private Texture2D _withered;
	[Export] private Texture2D _dead;

	private readonly List<(TextureRect Plant, TextureRect Bloom)> _plants = new();
	private TextureRect _wetSoil;
	private TextureRect _fertilizedSoil;
	private Tween _pop;

	public Plot Plot { get; set; }

	public override void _Ready()
	{
		foreach (var plant in GetNode("%Plants").GetChildren())
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

	/// <summary>A small bounce to show an action worked.</summary>
	public void Pop()
	{
		PivotOffset = Size / 2;
		_pop?.Kill();
		_pop = CreateTween();
		_pop.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.08f);
		_pop.TweenProperty(this, "scale", Vector2.One, 0.12f);
	}
}
