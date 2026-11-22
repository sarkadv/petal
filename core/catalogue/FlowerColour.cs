using Godot;

namespace Petal;

/// <summary>
/// The colours flowers come in (GAMEPLAY.md §4.4).
/// Data files store these as numbers, so never renumber an existing value.
/// </summary>
public enum FlowerColour
{
	Red = 0,
	Pink = 1,
	White = 2,
	Yellow = 3,
	Orange = 4,
	Purple = 5,
	Blue = 6,
	Green = 7,
}

/// <summary>How each <see cref="FlowerColour"/> looks and is named.</summary>
public static class FlowerColours
{
	/// <summary>Colour used to tint white placeholder art and swatches.</summary>
	public static Color Tint(this FlowerColour colour) => colour switch
	{
		FlowerColour.Red => new Color("d9534f"),
		FlowerColour.Pink => new Color("f08fb0"),
		FlowerColour.White => new Color("f7f3ea"),
		FlowerColour.Yellow => new Color("f4d03f"),
		FlowerColour.Orange => new Color("f39c4a"),
		FlowerColour.Purple => new Color("9b6fc4"),
		FlowerColour.Blue => new Color("5b8fd6"),
		_ => new Color("7fb069"),
	};

	public static string DisplayName(this FlowerColour colour) =>
		TranslationServer.Translate(colour.ToString()).ToString();
}
