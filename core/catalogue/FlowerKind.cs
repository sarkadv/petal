namespace Petal;

/// <summary>
/// Every flower in the catalogue (GAMEPLAY.md §4.4); its details live in <see cref="FlowerData"/>.
/// Data files store these as numbers, so never renumber an existing value.
/// </summary>
public enum FlowerKind
{
	Daisy = 0,
	Tulip = 1,
	Carnation = 2,
	Fern = 3,
	Sunflower = 4,
	Rose = 5,
	Lily = 6,
	Lavender = 7,
	Peony = 8,
	Orchid = 9,
	BlueRose = 10,
}
