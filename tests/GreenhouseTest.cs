using System.Linq;
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
	private Control SeedPopover => Get<Control>("%SeedPopover");
	private Node SeedRows => _screen.GetNode("%SeedPopover/%Rows");
	private SeedPacket Packet(int row, int index) => SeedRows.GetChild(row).GetNode("%Packets").GetChild<SeedPacket>(index);
	private Control FertilizerPopover => Get<Control>("%FertilizerPopover");
	private Node FertilizerRows => _screen.GetNode("%FertilizerPopover/%Rows");
	private Control InfoPopover => Get<Control>("%InfoPopover");
	private static Label CountOf(Node owner) => owner.GetNode("%Badge").GetNode<Label>("%Count");
	private Label BadgeCount(string tool) => CountOf(Get<Node>(tool));
	private Control BadgeIcon(string tool) => Get<Node>(tool).GetNode("%Badge").GetNode<Control>("%Icon");

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
		AssertThat(SeedPopover.Visible).IsFalse();
	}

	[TestCase]
	public async Task HoldingSeedsShowsAPacketRowPerFlowerAndPicksOne()
	{
		await _runner.Hold(Get<Button>("%SeedsTool"));

		AssertThat(SeedPopover.Visible).IsTrue();
		AssertThat(SeedRows.GetChildren().Take(3).Select(r => r.GetNode<Label>("%Heading").Text))
			.ContainsExactly("Daisy", "Tulip", "Fern");
		var tulips = SeedRows.GetChild(1).GetNode("%Packets").GetChildren().Cast<SeedPacket>();
		AssertThat(tulips.Select(p => p.GetNode<Label>("%Colour").Text)).ContainsExactly("red", "yellow");
		AssertThat(CountOf(Packet(0, 0)).Text).IsEqual("×6");
		AssertThat(Packet(0, 0).ButtonPressed).IsTrue();

		await _runner.Tap(Packet(1, 0));

		AssertThat(SeedPopover.Visible).IsFalse();
		AssertThat(BadgeCount("%SeedsTool").Text).IsEqual("×2");
	}

	[TestCase]
	public async Task OnlyRowsThatDoNotFitGetAScrollbar()
	{
		foreach (var colour in GameData.Instance.Flower(FlowerKind.Dahlia).Colours)
			_game.BuySeed(new Seed(FlowerKind.Dahlia, colour));
		await _runner.Hold(Get<Button>("%SeedsTool"));

		foreach (var row in SeedRows.GetChildren())
		{
			bool dahlia = row.GetNode<Label>("%Heading").Text == "Dahlia";
			AssertThat(row.GetNode<ScrollContainer>("%Scroll").GetHScrollBar().Visible).IsEqual(dahlia);
		}
		AssertThat(_screen.GetNode<ScrollContainer>("%SeedPopover/%Scroll").GetVScrollBar().Visible).IsTrue();
	}

	[TestCase]
	public async Task TheChosenSeedIsScrolledIntoView()
	{
		foreach (var kind in new[] { FlowerKind.Rose, FlowerKind.Lavender, FlowerKind.Peony })
			_game.BuySeed(new Seed(kind, GameData.Instance.Flower(kind).Colours[0]));
		await _runner.Hold(Get<Button>("%SeedsTool"));
		var scroll = _screen.GetNode<ScrollContainer>("%SeedPopover/%Scroll");
		scroll.ScrollVertical = 10000;
		await _runner.AwaitIdleFrame();
		await _runner.Tap(Packet(SeedRows.GetChildCount() - 1, 0));

		await _runner.Hold(Get<Button>("%SeedsTool"));

		var chosen = Packet(SeedRows.GetChildCount() - 1, 0);
		AssertThat(chosen.ButtonPressed).IsTrue();
		AssertThat(scroll.GetGlobalRect().Encloses(chosen.GetGlobalRect())).IsTrue();
	}

	[TestCase]
	public async Task DraggingThePacketsDoesNotPickOne()
	{
		foreach (var colour in GameData.Instance.Flower(FlowerKind.Dahlia).Colours)
			_game.BuySeed(new Seed(FlowerKind.Dahlia, colour));
		await _runner.Hold(Get<Button>("%SeedsTool"));
		var list = _screen.GetNode<ScrollContainer>("%SeedPopover/%Scroll");
		var dahlias = SeedRows.GetChildren().First(r => r.GetNode<Label>("%Heading").Text == "Dahlia");
		var row = dahlias.GetNode<ScrollContainer>("%Scroll");
		var packet = row.GetNode("%Packets").GetChild<SeedPacket>(1);

		// Both drags stay within how far the packets can scroll, so they move with the finger and it lets go on the
		// packet it pressed, as on a phone.
		await _runner.Drag(Packet(1, 0), new Vector2(0, -200));

		AssertThat(list.ScrollVertical).IsGreater(0);
		AssertThat(list.GetGlobalRect().HasPoint(packet.GetGlobalRect().GetCenter())).IsTrue();
		AssertThat(Packet(1, 0).ButtonPressed).IsFalse();

		await _runner.Drag(packet, new Vector2(-80, 0));

		AssertThat(row.ScrollHorizontal).IsGreater(0);
		AssertThat(packet.ButtonPressed).IsFalse();
		AssertThat(SeedPopover.Visible).IsTrue();
		AssertThat(Packet(0, 0).ButtonPressed).IsTrue();
	}

	[TestCase]
	public async Task RunningOutKeepsTheChosenSeedAtZero()
	{
		await _runner.Hold(Get<Button>("%SeedsTool"));
		await _runner.Tap(Packet(1, 0));
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

		AssertThat(FertilizerPopover.Visible).IsTrue();
		AssertThat(FertilizerRows.GetChildCount()).IsEqual(0);
		AssertThat(_screen.GetNode<Label>("%FertilizerPopover/%Empty").Visible).IsTrue();
	}

	[TestCase]
	public async Task HoldingFertilizeShowsARowPerFertilizerAndPicksOne()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.BuyFertilizer(FertilizerKind.Bloom);
		_game.BuyFertilizer(FertilizerKind.Bloom);

		await _runner.Hold(Get<Button>("%FertilizerTool"));

		var rows = FertilizerRows.GetChildren();
		AssertThat(rows.Select(r => r.GetNode<Button>("%Button").Text)).ContainsExactly("Growth fertilizer", "Bloom fertilizer");
		AssertThat(rows.Select(r => CountOf(r).Text)).ContainsExactly("×1", "×2");
		AssertThat(rows.Select(r => r.GetNode<Control>("%Line").Visible)).ContainsExactly(false, true);
		AssertThat(rows[0].GetNode<Button>("%Button").ButtonPressed).IsTrue();

		await _runner.Tap(rows[1].GetNode<Button>("%Button"));

		AssertThat(FertilizerPopover.Visible).IsFalse();
		AssertThat(BadgeCount("%FertilizerTool").Text).IsEqual("×2");
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

		AssertThat(FertilizerPopover.Visible).IsFalse();
	}

	[TestCase]
	public async Task TappingAnotherToolWhileChoosingSeedsPicksItUp()
	{
		await _runner.Hold(Get<Button>("%SeedsTool"));
		await _runner.Tap(Get<Button>("%WaterTool"));

		AssertThat(SeedPopover.Visible).IsFalse();
		AssertThat(Get<Button>("%WaterTool").ButtonPressed).IsTrue();
	}

	[TestCase]
	public async Task HoldingFertilizeWhileChoosingSeedsOpensItsChoices()
	{
		await _runner.Hold(Get<Button>("%SeedsTool"));
		await _runner.Hold(Get<Button>("%FertilizerTool"));

		AssertThat(SeedPopover.Visible).IsFalse();
		AssertThat(FertilizerPopover.Visible).IsTrue();
		AssertThat(Get<Button>("%FertilizerTool").ButtonPressed).IsTrue();
	}

	[TestCase]
	public async Task HoldingSeedsWhileChoosingFertilizerOpensItsChoices()
	{
		await _runner.Hold(Get<Button>("%FertilizerTool"));
		await _runner.Hold(Get<Button>("%SeedsTool"));

		AssertThat(FertilizerPopover.Visible).IsFalse();
		AssertThat(SeedPopover.Visible).IsTrue();
		AssertThat(Get<Button>("%SeedsTool").ButtonPressed).IsTrue();
	}

	[TestCase]
	public async Task TappingAPotWhileChoosingFertilizerOnlyClosesThePopover()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		await _runner.Hold(Get<Button>("%FertilizerTool"));
		await _runner.Tap(Pot(2));

		AssertThat(FertilizerPopover.Visible).IsFalse();
		AssertThat(_game.Plots[2].Fertilizer).IsNull();
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
