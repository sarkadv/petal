namespace Petal;

/// <summary>A kind of harvested stem in today's inventory (GAMEPLAY.md §4.5). Quality is in stars (§4.4).</summary>
public readonly record struct Stem(FlowerKind Kind, FlowerColour Colour, int Quality)
{
	public FlowerData Flower => GameData.Instance.Flower(Kind);
}
