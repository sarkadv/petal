using System;
using Godot;

namespace Petal;

/// <summary>
/// One item on the seed stall (GAMEPLAY.md §4.3): a flower's seeds, with swatches to pick the colour
/// when it comes in several, or a fertilizer.
/// </summary>
public partial class StallRow : PanelContainer
{
	[Export] private Texture2D _dotIcon;

	private GameState _game;
	private Func<int> _ownedCount;
	private Func<bool> _canBuy;
	private Func<bool> _buyOne;

	private TextureRect _icon;
	private Label _name;
	private Label _price;
	private Label _info;
	private Label _owned;
	private Button _buy;

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
		_name = GetNode<Label>("%Name");
		_price = GetNode<Label>("%Price");
		_info = GetNode<Label>("%Info");
		_owned = GetNode<Label>("%Owned");
		_buy = GetNode<Button>("%BuyButton");
		_buy.Pressed += () => _buyOne();
	}

	public override void _ExitTree()
	{
		if (_game != null)
			_game.Changed -= Refresh;
	}

	public void SetupSeeds(GameState game, FlowerData flower)
	{
		Listen(game);
		_icon.Texture = _dotIcon;
		_price.Text = flower.SeedPrice.ToString();
		_info.Text = string.Format(Tr("Grow time: {0} d"), flower.GrowDays);
		if (flower.Colours.Count > 1)
			AddSwatches(flower);
		SellSeed(new Seed(flower.Kind, flower.Colours[0]));
	}

	public void SetupFertilizer(GameState game, FertilizerData fertilizer)
	{
		Listen(game);
		_icon.Texture = fertilizer.Icon;
		_name.Text = Tr(fertilizer.Name);
		_price.Text = fertilizer.Price.ToString();
		_info.Text = Tr(fertilizer.Description);
		Sell(
			() => game.FertilizerCount(fertilizer.Kind),
			() => game.CanBuy(fertilizer.Kind),
			() => game.BuyFertilizer(fertilizer.Kind));
	}

	private void Listen(GameState game)
	{
		_game = game;
		_game.Changed += Refresh;
	}

	private void AddSwatches(FlowerData flower)
	{
		var swatches = GetNode<Container>("%Swatches");
		var group = new ButtonGroup();
		foreach (var colour in flower.Colours)
		{
			var swatch = new Button
			{
				ToggleMode = true,
				ButtonGroup = group,
				ButtonPressed = colour == flower.Colours[0],
				ThemeTypeVariation = "SwatchButton",
				Icon = _dotIcon,
				IconAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(64, 64),
			};
			swatch.SetIconTint(colour.Tint());
			swatch.Pressed += () => SellSeed(new Seed(flower.Kind, colour));
			swatches.AddChild(swatch);
		}
	}

	private void SellSeed(Seed seed)
	{
		_name.Text = seed.DisplayName;
		_icon.SelfModulate = seed.Colour.Tint();
		Sell(() => _game.SeedCount(seed), () => _game.CanBuy(seed), () => _game.BuySeed(seed));
	}

	private void Sell(Func<int> ownedCount, Func<bool> canBuy, Func<bool> buyOne)
	{
		_ownedCount = ownedCount;
		_canBuy = canBuy;
		_buyOne = buyOne;
		Refresh();
	}

	private void Refresh()
	{
		_owned.Text = string.Format(Tr("Owned: {0}"), _ownedCount());
		_buy.Disabled = !_canBuy();
	}
}
