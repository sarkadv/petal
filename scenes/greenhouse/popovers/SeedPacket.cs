using Godot;

namespace Petal;

/// <summary>
/// One seed in the <see cref="SeedPopover"/> (GREENHOUSE.md §3): a packet showing the flower in its colour, the
/// colour's name and how many are left.
/// </summary>
public partial class SeedPacket : Button
{
	public Seed Seed { get; private set; }

	/// <summary>Call once it's in the tree.</summary>
	public void SetSeed(Seed seed, int count)
	{
		Seed = seed;
		GetNode<TextureRect>("%Flower").SelfModulate = seed.Colour.Tint();
		GetNode<Label>("%Colour").Text = seed.Colour.DisplayName();
		GetNode<CountBadge>("%Badge").SetCount(count);
	}
}
