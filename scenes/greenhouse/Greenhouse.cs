using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// The morning greenhouse (GAMEPLAY.md §4), seen from above as in docs/reference/greenhouse_concept.svg:
/// pots worked with the tools on the shelf (plant, water, fertilize, harvest, clear: §4.2), the seed stall
/// (§4.3), the inventory button (§4.5), and the Open Shop door to move on to the day's customers.
/// Tapping a tool selects it; holding Seeds or Fertilize opens a small popover to choose which one it uses, and
/// holding a pot shows what's growing in it.
/// The layout is a fixed 720×1280 stage, centred, with grass filling any extra screen.
/// </summary>
public partial class Greenhouse : Control
{
	private enum Tool { Seeds, Water, Fertilizer, Harvest, Clear }

	/// <summary>How long a press must last to count as a hold rather than a tap.</summary>
	public const double HoldSeconds = 0.4;

	[Export] private PackedScene _plotScene;
	[Export] private Texture2D _dotIcon;

	private GameState _game;
	private Tool _tool;
	/// <summary>Null until the player has any.</summary>
	private Seed? _seed;
	/// <summary>Null until the player has any.</summary>
	private FertilizerData _fertilizer;
	private PlotView _toClear;
	/// <summary>Bumped on every press and release, so a hold timer from an earlier press is ignored.</summary>
	private int _holdId;
	/// <summary>Set when a pot was held, so releasing it doesn't also use the tool.</summary>
	private bool _plotWasHeld;

	private readonly Dictionary<Tool, ToolButton> _toolButtons = new();
	private readonly List<PlotView> _plotViews = new();
	private Label _dayLabel;
	private Label _moneyLabel;
	private CountBadge _inventoryBadge;
	private Control _seedPacket;
	private SeedPopover _seedPopover;
	private FertilizerPopover _fertilizerPopover;
	private InfoPopover _infoPopover;
	private Inventory _inventory;
	private Control _confirmClear;

	public override void _Ready()
	{
		_game = GameState.Current;

		_dayLabel = GetNode<Label>("%DayLabel");
		_moneyLabel = GetNode<Label>("%MoneyLabel");
		_inventoryBadge = GetNode<CountBadge>("%InventoryBadge");
		_seedPacket = GetNode<Control>("%SeedPacket");
		_seedPopover = GetNode<SeedPopover>("%SeedPopover");
		_fertilizerPopover = GetNode<FertilizerPopover>("%FertilizerPopover");
		_infoPopover = GetNode<InfoPopover>("%InfoPopover");
		_inventory = GetNode<Inventory>("%Inventory");
		_confirmClear = GetNode<Control>("%ConfirmClear");

		var plots = GetNode<Container>("%Plots");
		foreach (var plot in _game.Plots)
		{
			var view = _plotScene.Instantiate<PlotView>();
			view.Plot = plot;
			plots.AddChild(view);
			view.Pressed += () => OnPlotPressed(view);
			WatchHold(view, () => ShowPlotInfo(view));
			// Runs after this release's Pressed, so letting go of a held pot doesn't also use the tool on it.
			view.ButtonUp += () => Callable.From(() => _plotWasHeld = false).CallDeferred();
			_plotViews.Add(view);
		}

		AddTool(Tool.Seeds, "%SeedsTool", holdable: true);
		AddTool(Tool.Water, "%WaterTool");
		AddTool(Tool.Fertilizer, "%FertilizerTool", holdable: true);
		AddTool(Tool.Harvest, "%HarvestTool");
		AddTool(Tool.Clear, "%ClearTool");
		Control[] tools = _toolButtons.Values.ToArray<Control>();
		_seedPopover.TapThrough = tools;
		_fertilizerPopover.TapThrough = tools;
		_infoPopover.TapThrough = tools;

		GetNode<Button>("%BackButton").Pressed += GoBack;
		GetNode<Button>("%InventoryButton").Pressed += () => _inventory.Open(_game.Inventory);
		GetNode<Button>("%StallButton").Pressed += GetNode<Control>("%SeedStall").Show;
		GetNode<Button>("%OpenShopButton").Pressed += OnOpenShopPressed;
		GetNode<Button>("%KeepButton").Pressed += _confirmClear.Hide;
		GetNode<Button>("%RemoveButton").Pressed += OnRemovePressed;

		_game.Changed += Refresh;
		SelectTool(Tool.Water);
		Refresh();
	}

	public override void _ExitTree()
	{
		_game.Changed -= Refresh;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMGoBackRequest)
			GoBack();
	}

	private void AddTool(Tool tool, string path, bool holdable = false)
	{
		var button = GetNode<ToolButton>(path);
		button.Pressed += () => SelectTool(tool);
		if (holdable)
			WatchHold(button, () => ShowToolChoices(tool, button));
		_toolButtons[tool] = button;
	}

	/// <summary>Calls <paramref name="onHeld"/> once the button has been held down for HoldSeconds.</summary>
	private void WatchHold(BaseButton button, System.Action onHeld)
	{
		button.ButtonDown += () =>
		{
			int id = ++_holdId;
			GetTree().CreateTimer(HoldSeconds).Timeout += () =>
			{
				if (IsInstanceValid(this) && id == _holdId)
					onHeld();
			};
		};
		button.ButtonUp += () => _holdId++;
	}

	private void ShowToolChoices(Tool tool, Button button)
	{
		SelectTool(tool);
		if (tool == Tool.Seeds)
			_seedPopover.ShowSeeds(button, _game.Seeds, _seed, PickSeed);
		else
			_fertilizerPopover.ShowFertilizers(button, _game.Fertilizers, _fertilizer, PickFertilizer);
	}

	private void PickSeed(Seed seed)
	{
		_seed = seed;
		UpdateToolBadges();
	}

	private void PickFertilizer(FertilizerData fertilizer)
	{
		_fertilizer = fertilizer;
		UpdateToolBadges();
	}

	private void ShowPlotInfo(PlotView view)
	{
		_plotWasHeld = true;
		var plot = view.Plot;
		if (plot.Seed is null) return;
		string state = plot.Stage switch
		{
			PlotStage.Ready => Tr("Ready to harvest"),
			PlotStage.Withered => Tr("Withered"),
			PlotStage.Dead => Tr("Dead"),
			_ => string.Format(Tr("Ready in {0} d"), plot.DaysLeft),
		};
		_infoPopover.ShowText(view, $"{plot.Seed.Value.DisplayName}\n{state}");
	}

	private void SelectTool(Tool tool)
	{
		_tool = tool;
		_toolButtons[tool].ButtonPressed = true;
	}

	private void OnPlotPressed(PlotView view)
	{
		if (_plotWasHeld) return;
		var plot = view.Plot;
		bool done = _tool switch
		{
			Tool.Seeds => _seed is { } seed && _game.Plant(plot, seed),
			Tool.Water => _game.Water(plot),
			Tool.Fertilizer => _fertilizer != null && _game.Fertilize(plot, _fertilizer.Kind),
			Tool.Harvest => _game.Harvest(plot),
			Tool.Clear => Clear(view),
			_ => false,
		};
		if (done)
			view.Pop();
	}

	private bool Clear(PlotView view)
	{
		if (!view.Plot.IsAlive) return _game.Clear(view.Plot);
		_toClear = view;
		_confirmClear.Show();
		return false;
	}

	private void OnRemovePressed()
	{
		_confirmClear.Hide();
		if (_game.Clear(_toClear.Plot))
			_toClear.Pop();
	}

	private void OnOpenShopPressed()
	{
		// Until the shop (§5) exists, opening it just ends the day and starts the next morning.
		_game.EndDay();
	}

	private void GoBack()
	{
		if (!Overlay.CloseTopmost(this))
			GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}

	private void Refresh()
	{
		_dayLabel.Text = string.Format(Tr("Day {0}"), _game.Day);
		_moneyLabel.Text = _game.Money.ToString();
		_inventoryBadge.SetCount(_game.InventoryCount);
		EnsureChoices();
		UpdateToolBadges();
		foreach (var view in _plotViews)
			view.Refresh();
	}

	/// <summary>
	/// Until something is chosen, the tool takes the first one in stock. A choice that runs out stays chosen at ×0;
	/// only the player switches it.
	/// </summary>
	private void EnsureChoices()
	{
		_seed ??= _game.Seeds.Select(s => (Seed?)s.Seed).FirstOrDefault();
		_fertilizer ??= _game.Fertilizers.Select(f => f.Fertilizer).FirstOrDefault();
	}

	/// <summary>
	/// The Seeds and Fertilize tools always show how many of their chosen item are left (×0 when there's nothing).
	/// While there's at least one, they also show its colour or icon, and the seed tray shows the chosen packets.
	/// </summary>
	private void UpdateToolBadges()
	{
		int seeds = _seed is { } seed ? _game.SeedCount(seed) : 0;
		var seedTint = _seed?.Colour.Tint() ?? Colors.White;
		_seedPacket.Visible = seeds > 0;
		_seedPacket.SelfModulate = seedTint;
		_toolButtons[Tool.Seeds].ShowBadge(seeds, _dotIcon, seedTint);

		int fertilizers = _fertilizer != null ? _game.FertilizerCount(_fertilizer.Kind) : 0;
		_toolButtons[Tool.Fertilizer].ShowBadge(fertilizers, _fertilizer?.Icon, Colors.White);
	}
}
