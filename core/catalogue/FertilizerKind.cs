namespace Petal;

/// <summary>
/// Every fertilizer (GAMEPLAY.md §4.2); its details live in <see cref="FertilizerData"/>.
/// Data files store these as numbers, so never renumber an existing value.
/// </summary>
public enum FertilizerKind
{
	Growth = 0,
	Bloom = 1,
}
