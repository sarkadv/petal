using Godot;

namespace Petal;

/// <summary>
/// One item in the <see cref="Inventory"/> (GREENHOUSE.md §2), a flower in one colour and quality: the flower in its
/// colour, a star per quality level (GAMEPLAY.md §4.4), the colour's name and how many there are.
/// </summary>
public partial class InventoryItem : Control
{
	public Stem Stem { get; private set; }

	/// <summary>Call once it's in the tree.</summary>
	public void SetItem(Stem stem, int count)
	{
		Stem = stem;
		GetNode<TextureRect>("%Flower").SelfModulate = stem.Colour.Tint();
		var stars = GetNode<TextureRect>("%Stars");
		var star = stars.Texture.GetSize();
		stars.CustomMinimumSize = new Vector2(star.X * stem.Quality, star.Y);
		GetNode<Label>("%Colour").Text = stem.Colour.DisplayName();
		GetNode<CountBadge>("%Badge").SetCount(count);
	}
}
