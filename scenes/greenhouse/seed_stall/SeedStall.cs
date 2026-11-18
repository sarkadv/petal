using Godot;

namespace Petal;

/// <summary>
/// The garden supplies stall in the greenhouse (GAMEPLAY.md §4.3), shown over the greenhouse. Its Seeds tab sells
/// every unlocked flower's seeds in any colour and its Fertilizers tab both fertilizers, at fixed prices.
/// </summary>
public partial class SeedStall : Overlay
{
	[Export] private PackedScene _rowScene;

	private GameState _game;
	private Label _money;
	private Control _seedRows;
	private Control _fertilizerRows;
	private ScrollContainer _scroll;

	public override void _Ready()
	{
		_game = GameState.Current;
		_money = GetNode<Label>("%Money");
		_seedRows = GetNode<Control>("%SeedRows");
		_fertilizerRows = GetNode<Control>("%FertilizerRows");
		_scroll = GetNode<ScrollContainer>("%Scroll");
		GetNode<Button>("%SeedsTab").Pressed += () => ShowTab(seeds: true);
		GetNode<Button>("%FertilizersTab").Pressed += () => ShowTab(seeds: false);
		GetNode<Button>("%CloseButton").Pressed += Hide;

		// Unlocks only change in the upgrade shop (§7), so the rows can be built once.
		foreach (var flower in _game.UnlockedFlowers)
			AddRow(_seedRows).SetupSeeds(_game, flower);
		foreach (var fertilizer in GameData.Instance.Fertilizers)
			AddRow(_fertilizerRows).SetupFertilizer(_game, fertilizer);

		ShowTab(seeds: true);
		_game.Changed += RefreshMoney;
		RefreshMoney();
	}

	public override void _ExitTree()
	{
		_game.Changed -= RefreshMoney;
	}

	private StallRow AddRow(Node parent)
	{
		var row = _rowScene.Instantiate<StallRow>();
		parent.AddChild(row);
		return row;
	}

	private void ShowTab(bool seeds)
	{
		_seedRows.Visible = seeds;
		_fertilizerRows.Visible = !seeds;
		_scroll.ScrollVertical = 0;
	}

	private void RefreshMoney()
	{
		_money.Text = _game.Money.ToString();
	}
}
