using Godot;

namespace Petal;

/// <summary>A seed in stock: a flower in one colour, bought and kept as its own item (GAMEPLAY.md §4.3).</summary>
public readonly record struct Seed(FlowerKind Kind, FlowerColour Colour)
{
	public FlowerData Flower => GameData.Instance.Flower(Kind);

	/// <summary>"Tulip (red)", or just "Daisy" for flowers that only come in one colour.</summary>
	public string DisplayName
	{
		get
		{
			string name = TranslationServer.Translate(Flower.Name).ToString();
			return Flower.Colours.Count > 1 ? $"{name} ({Colour.DisplayName()})" : name;
		}
	}

	/// <summary>Reads the <c>"Tulip:Red"</c> form used in data files.</summary>
	public static Seed Parse(string key)
	{
		var parts = key.Split(':');
		return new Seed(
			System.Enum.Parse<FlowerKind>(parts[0], ignoreCase: true),
			System.Enum.Parse<FlowerColour>(parts[1], ignoreCase: true));
	}
}
