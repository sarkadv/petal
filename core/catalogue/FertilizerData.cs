using Godot;

namespace Petal;

/// <summary>One fertilizer (GAMEPLAY.md §4.2), sold at the garden supplies (§4.3). All fertilizers live in <see cref="GameData"/>.</summary>
[GlobalClass]
public partial class FertilizerData : Resource
{
	[Export] public FertilizerKind Kind { get; set; }

	/// <summary>English name, also the translation key.</summary>
	[Export] public string Name { get; set; } = "";

	/// <summary>English description of what it does, also the translation key.</summary>
	[Export] public string Description { get; set; } = "";

	[Export] public Texture2D Icon { get; set; }
	[Export] public int Price { get; set; }

	/// <summary>Tint over a pot's soil while this fertilizer is in it (GREENHOUSE.md §4).</summary>
	[Export] public Color SoilTint { get; set; } = Colors.White;
}
