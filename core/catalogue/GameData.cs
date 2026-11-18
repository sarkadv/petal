using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// Balancing numbers and the flower and fertilizer catalogues, kept in <c>data/game_data.tres</c> so they can be
/// tuned without touching code (GAMEPLAY.md §9).
/// </summary>
[GlobalClass]
public partial class GameData : Resource
{
	private const string FilePath = "res://data/game_data.tres";
	private static GameData _instance;

	public static GameData Instance => _instance ??= GD.Load<GameData>(FilePath);

	[Export] public int StartingMoney { get; set; }
	[Export] public int StartingPlots { get; set; }

	/// <summary>A harvest gives a random number of stems in this range, inclusive, whatever the flower.</summary>
	[Export] public int MinHarvestStems { get; set; } = 3;
	[Export] public int MaxHarvestStems { get; set; } = 6;

	/// <summary>Seed counts keyed like <c>"Tulip:Red"</c> (see <see cref="Seed.Parse"/>).</summary>
	[Export] public Godot.Collections.Dictionary<string, int> StarterSeeds { get; set; } = new();

	/// <summary>Chance, from 0 to 1, that one Bloom fertilizer gives an extra star at harvest.</summary>
	[Export] public float BloomStarChance { get; set; } = 0.4f;
	[Export] public Godot.Collections.Array<FlowerData> Flowers { get; set; } = new();
	[Export] public Godot.Collections.Array<FertilizerData> Fertilizers { get; set; } = new();

	private Dictionary<FlowerKind, FlowerData> _flowersByKind;
	private Dictionary<FertilizerKind, FertilizerData> _fertilizersByKind;

	public FlowerData Flower(FlowerKind kind) => (_flowersByKind ??= Flowers.ToDictionary(f => f.Kind))[kind];

	public FertilizerData Fertilizer(FertilizerKind kind) =>
		(_fertilizersByKind ??= Fertilizers.ToDictionary(f => f.Kind))[kind];
}
