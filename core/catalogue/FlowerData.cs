using Godot;

namespace Petal;

/// <summary>One flower in the catalogue (GAMEPLAY.md §4.4). All flowers live in <see cref="GameData"/>.</summary>
[GlobalClass]
public partial class FlowerData : Resource
{
	[Export] public FlowerKind Kind { get; set; }

	/// <summary>English name, also the translation key.</summary>
	[Export] public string Name { get; set; } = "";

	[Export] public Godot.Collections.Array<FlowerColour> Colours { get; set; } = new();
	[Export] public int GrowDays { get; set; } = 1;
	[Export] public int SeedPrice { get; set; }
	[Export] public int BaseValue { get; set; }
	[Export] public string[] Tags { get; set; } = System.Array.Empty<string>();

	/// <summary>Unlocked in a new game; the others come from seed catalogs and upgrades (§7).</summary>
	[Export] public bool UnlockedAtStart { get; set; }
}
