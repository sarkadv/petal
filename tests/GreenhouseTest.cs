using System.Threading.Tasks;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

namespace Petal.Tests;

/// <summary>The greenhouse screen as described in docs/GREENHOUSE.md, driven with taps and holds.</summary>
[TestSuite]
[RequireGodotRuntime]
public class GreenhouseTest
{
	private static readonly Seed Daisy = new(FlowerKind.Daisy, FlowerColour.White);
	private static readonly Seed RedTulip = new(FlowerKind.Tulip, FlowerColour.Red);

	private ISceneRunner _runner;
	private Node _screen;
	private GameState _game;

	[BeforeTest]
	public async Task Setup()
	{
		TranslationServer.SetLocale("en");
		GameState.StartNewGame();
		_game = GameState.Current;
		_runner = ISceneRunner.Load(Scenes.Greenhouse, true);
		_screen = _runner.Scene();
		// The pots are added in _Ready and only placed by the grid on the next frame.
		await _runner.AwaitIdleFrame();
	}

	[AfterTest]
	public void TearDown() => _runner.Dispose();

	private T Get<T>(string uniqueName) where T : Node => _screen.GetNode<T>(uniqueName);
	private Control Pot(int index) => _screen.GetNode("%Plots").GetChild<Control>(index);
	private Control ChoicePopover => Get<Control>("%ChoicePopover");
	private Node Choices => _screen.GetNode("%ChoicePopover/%Content");
	private Control InfoPopover => Get<Control>("%InfoPopover");
	private Label BadgeCount(string tool) => Get<Node>(tool).GetNode<Label>("%BadgeCount");
	private Control BadgeIcon(string tool) => Get<Node>(tool).GetNode<Control>("%BadgeIcon");

	[TestCase]
	public void OpensOnDayOneWithWaterSelected()
	{
		AssertThat(Get<Label>("%DayLabel").Text).IsEqual("Day 1");
		AssertThat(Get<Label>("%MoneyLabel").Text).IsEqual("100");
		AssertThat(Get<Button>("%WaterTool").ButtonPressed).IsTrue();
		AssertThat(Get<Button>("%SeedsTool").ButtonPressed).IsFalse();
		AssertThat(BadgeCount("%SeedsTool").Text).IsEqual("×6");
		AssertThat(BadgeCount("%FertilizerTool").Text).IsEqual("×0");
	}

	[TestCase]
	public async Task ExactlyOneToolIsAlwaysSelected()
	{
		await _runner.Tap(Get<Button>("%HarvestTool"));
		AssertThat(Get<Button>("%HarvestTool").ButtonPressed).IsTrue();
		AssertThat(Get<Button>("%WaterTool").ButtonPressed).IsFalse();

		await _runner.Tap(Get<Button>("%HarvestTool"));
		AssertThat(Get<Button>("%HarvestTool").ButtonPressed).IsTrue();
	}

	[TestCase]
	public async Task TappingAPotWithWaterWetsTheSoil()
	{
		await _runner.Tap(Pot(0));

		AssertThat(_game.Plots[0].Watered).IsTrue();
		AssertThat(Pot(0).GetNode<Control>("%WetSoil").Visible).IsTrue();
	}

	[TestCase]
	public async Task TappingAPotWithSeedsPlantsTheChosenSeed()
	{
		await _runner.Tap(Get<Button>("%SeedsTool"));
		await _runner.Tap(Pot(0));

		AssertThat(_game.Plots[0].Stage).IsEqual(PlotStage.Seedling);
		AssertThat(_game.Plots[0].Seed).IsEqual(Daisy);
		AssertThat(BadgeCount("%SeedsTool").Text).IsEqual("×5");
		AssertThat(ChoicePopover.Visible).IsFalse();
	}

	[TestCase]
	public async Task HoldingSeedsShowsTheChoicesAndPicksOne()
	{
		await _runner.Hold(Get<Button>("%SeedsTool"));

		AssertThat(ChoicePopover.Visible).IsTrue();
		AssertThat(Choices.GetChildCount()).IsEqual(4);
		var daisy = Choices.GetChild<Button>(0);
		AssertThat(daisy.Text).IsEqual("Daisy ×6");
		AssertThat(daisy.ButtonPressed).IsTrue();

		await _runner.Tap(Choices.GetChild<Button>(1));

		AssertThat(ChoicePopover.Visible).IsFalse();
		AssertThat(BadgeCount("%SeedsTool").Text).IsEqual("×2");
	}

	[TestCase]
	public async Task RunningOutKeepsTheChosenSeedAtZero()
	{
		await _runner.Hold(Get<Button>("%SeedsTool"));
		await _runner.Tap(Choices.GetChild<Button>(1));
		await _runner.Tap(Pot(0));
		await _runner.Tap(Pot(1));
		AssertThat(BadgeCount("%SeedsTool").Text).IsEqual("×0");
		AssertThat(Get<Control>("%SeedPacket").Visible).IsFalse();
		AssertThat(BadgeIcon("%SeedsTool").Visible).IsFalse();

		await _runner.Tap(Pot(2));
		AssertThat(_game.Plots[2].Stage).IsEqual(PlotStage.Empty);
		AssertThat(BadgeCount("%SeedsTool").Text).IsEqual("×0");
	}

	[TestCase]
	public async Task HoldingFertilizeWithNoneSaysSo()
	{
		await _runner.Hold(Get<Button>("%FertilizerTool"));

		AssertThat(ChoicePopover.Visible).IsTrue();
		AssertThat(Choices.GetChild<Label>(0).Text).IsEqual("You have no fertilizer");
	}

	[TestCase]
	public async Task HoldingAPotShowsWhatIsGrowingTowardsTheMiddle()
	{
		_game.Plant(_game.Plots[0], RedTulip);

		await _runner.Hold(Pot(0));

		AssertThat(InfoPopover.Visible).IsTrue();
		AssertThat(_screen.GetNode<Label>("%InfoPopover/%Text").Text).IsEqual("Tulip (red)\nReady in 2 d");
		var panel = _screen.GetNode<Control>("%InfoPopover/%Panel").GetGlobalRect();
		var pot = Pot(0).GetGlobalRect();
		AssertThat(panel.Position.X).IsBetween(pot.GetCenter().X, pot.End.X);
		AssertThat(panel.Position.Y).IsBetween(pot.GetCenter().Y, pot.End.Y);
	}

	[TestCase]
	public async Task LettingGoOfAHeldPotDoesNotUseTheTool()
	{
		_game.Plant(_game.Plots[0], RedTulip);

		await _runner.Hold(Pot(0));

		AssertThat(_game.Plots[0].Watered).IsFalse();
	}

	[TestCase]
	public async Task HoldingAnEmptyPotShowsNothing()
	{
		await _runner.Hold(Pot(0));

		AssertThat(InfoPopover.Visible).IsFalse();
		AssertThat(_game.Plots[0].Watered).IsFalse();
	}

	[TestCase]
	public async Task TappingElsewhereClosesThePopover()
	{
		await _runner.Hold(Get<Button>("%FertilizerTool"));
		await _runner.Tap(Get<Control>("%DayLabel"));

		AssertThat(ChoicePopover.Visible).IsFalse();
	}

	[TestCase]
	public async Task TheWholeStandOpensGardenSupplies()
	{
		var stand = Get<Control>("%StallButton").GetGlobalRect();
		var stall = Get<Control>("%SeedStall");
		foreach (float y in new[] { stand.Position.Y + 10, stand.GetCenter().Y, stand.End.Y - 10 })
		{
			await _runner.TapAt(new Vector2(stand.GetCenter().X, y));
			AssertThat(stall.Visible).OverrideFailureMessage($"stand tapped at y={y}").IsTrue();
			stall.Hide();
		}
	}

	[TestCase]
	public async Task GardenSuppliesSellsFertilizerOnItsOwnTab()
	{
		await _runner.Tap(Get<Control>("%StallButton"));
		var stall = Get<Control>("%SeedStall");
		AssertThat(stall.GetNode<Control>("%SeedRows").Visible).IsTrue();
		AssertThat(stall.GetNode<Control>("%FertilizerRows").Visible).IsFalse();

		await _runner.Tap(stall.GetNode<Button>("%FertilizersTab"));
		AssertThat(stall.GetNode<Control>("%SeedRows").Visible).IsFalse();

		var bloomRow = stall.GetNode("%FertilizerRows").GetChild(1);
		await _runner.Tap(bloomRow.GetNode<Button>("%BuyButton"));

		AssertThat(_game.FertilizerCount(FertilizerKind.Bloom)).IsEqual(1);
		AssertThat(_game.Money).IsEqual(95);
		AssertThat(BadgeCount("%FertilizerTool").Text).IsEqual("×1");
	}

	[TestCase]
	public async Task TappingOutsideGardenSuppliesClosesIt()
	{
		await _runner.Tap(Get<Control>("%StallButton"));
		var stall = Get<Control>("%SeedStall");

		await _runner.Tap(stall.GetNode<Control>("%Money"));
		AssertThat(stall.Visible).IsTrue();

		await _runner.TapAt(new Vector2(360, 20));
		AssertThat(stall.Visible).IsFalse();
	}

	[TestCase]
	public async Task TappingTheOpenTabKeepsItOpen()
	{
		await _runner.Tap(Get<Control>("%StallButton"));
		var stall = Get<Control>("%SeedStall");
		var seedsTab = stall.GetNode<Button>("%SeedsTab");

		await _runner.Tap(seedsTab);

		AssertThat(seedsTab.ButtonPressed).IsTrue();
		AssertThat(stall.GetNode<Button>("%FertilizersTab").ButtonPressed).IsFalse();
		AssertThat(stall.GetNode<Control>("%SeedRows").Visible).IsTrue();
	}

	[TestCase]
	public async Task BackClosesTheTopmostWindowFirst()
	{
		await _runner.Tap(Get<Control>("%StallButton"));
		var stall = Get<Control>("%SeedStall");

		Get<Control>("%ConfirmClear").Show();

		_screen.Notification((int)Node.NotificationWMGoBackRequest);
		AssertThat(Get<Control>("%ConfirmClear").Visible).IsFalse();
		AssertThat(stall.Visible).IsTrue();

		_screen.Notification((int)Node.NotificationWMGoBackRequest);
		AssertThat(stall.Visible).IsFalse();
		AssertThat(_screen.IsInsideTree()).IsTrue();
	}

	[TestCase]
	public async Task ClearingALivingPlantAsksFirst()
	{
		_game.Plant(_game.Plots[0], Daisy);
		await _runner.Tap(Get<Button>("%ClearTool"));

		await _runner.Tap(Pot(0));
		AssertThat(Get<Control>("%ConfirmClear").Visible).IsTrue();
		await _runner.Tap(Get<Button>("%KeepButton"));
		AssertThat(_game.Plots[0].Stage).IsEqual(PlotStage.Seedling);

		await _runner.Tap(Pot(0));
		await _runner.Tap(Get<Button>("%RemoveButton"));
		AssertThat(_game.Plots[0].Stage).IsEqual(PlotStage.Empty);
	}

	[TestCase]
	public async Task TappingOutsideTheQuestionKeepsThePlant()
	{
		_game.Plant(_game.Plots[0], Daisy);
		await _runner.Tap(Get<Button>("%ClearTool"));
		await _runner.Tap(Pot(0));

		await _runner.TapAt(new Vector2(360, 20));

		AssertThat(Get<Control>("%ConfirmClear").Visible).IsFalse();
		AssertThat(_game.Plots[0].Stage).IsEqual(PlotStage.Seedling);
	}

	[TestCase]
	public async Task FertilizingTintsTheSoil()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		await _runner.Tap(Get<Button>("%FertilizerTool"));
		AssertThat(BadgeIcon("%FertilizerTool").Visible).IsTrue();

		await _runner.Tap(Pot(0));

		AssertThat(_game.Plots[0].Fertilizer).IsEqual(FertilizerKind.Growth);
		AssertThat(Pot(0).GetNode<Control>("%FertilizedSoil").Visible).IsTrue();
		AssertThat(BadgeIcon("%FertilizerTool").Visible).IsFalse();
		AssertThat(BadgeCount("%FertilizerTool").Text).IsEqual("×0");
	}

	[TestCase]
	public async Task OpenShopStartsTheNextDay()
	{
		await _runner.Tap(Get<Button>("%OpenShopButton"));

		AssertThat(_game.Day).IsEqual(2);
		AssertThat(Get<Label>("%DayLabel").Text).IsEqual("Day 2");
	}
}
