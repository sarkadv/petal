using System.Collections.Generic;
using System.Linq;
using GdUnit4;
using static GdUnit4.Assertions;

namespace Petal.Tests;

/// <summary>The greenhouse rules (GAMEPLAY.md §4) on a fresh game, without any screen.</summary>
[TestSuite]
[RequireGodotRuntime]
public class PlotRulesTest
{
	private static readonly Seed Daisy = new(FlowerKind.Daisy, FlowerColour.White);
	private static readonly Seed RedTulip = new(FlowerKind.Tulip, FlowerColour.Red);

	private GameState _game;
	private IReadOnlyList<Plot> Plots => _game.Plots;

	[BeforeTest]
	public void Setup() => _game = GameState.NewGame();

	[TestCase]
	public void NewGameMatchesTheStartingValues()
	{
		AssertThat(_game.Money).IsEqual(100);
		AssertThat(Plots.Count).IsEqual(6);
		AssertThat(_game.SeedCount(Daisy)).IsEqual(6);
		AssertThat(_game.SeedCount(new Seed(FlowerKind.Tulip, FlowerColour.Yellow))).IsEqual(2);
	}

	[TestCase]
	public void UnwateredSeedlingsDieOvernight()
	{
		_game.Plant(Plots[0], Daisy);
		_game.EndDay();

		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Dead);
	}

	[TestCase]
	public void WateredPlantsGrowAndRipen()
	{
		_game.Plant(Plots[0], RedTulip);
		_game.Water(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Growing);

		_game.Water(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);
	}

	[TestCase]
	public void ReadyPlantsWitherAfterTwoDays()
	{
		RipenDaisy(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);

		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Withered);
		AssertThat(_game.Harvest(Plots[0])).IsFalse();
	}

	[TestCase]
	public void AnyPotCanBeWateredOnceADay()
	{
		AssertThat(_game.Water(Plots[0])).IsTrue();
		AssertThat(_game.Water(Plots[0])).IsFalse();
	}

	[TestCase]
	public void SeedsPlantedIntoWetSoilCountAsWatered()
	{
		_game.Water(Plots[0]);
		_game.Plant(Plots[0], Daisy);
		_game.EndDay();

		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);
	}

	[TestCase]
	public void HarvestingKeepsTheSoilWetUntilNight()
	{
		RipenDaisy(Plots[0]);
		_game.Water(Plots[0]);
		_game.Harvest(Plots[0]);
		AssertThat(Plots[0].Watered).IsTrue();

		_game.EndDay();
		AssertThat(Plots[0].Watered).IsFalse();
	}

	[TestCase]
	public void HarvestsGiveThreeToSixStemsThatWiltAtClosing()
	{
		var seen = new HashSet<int>();
		for (int i = 0; i < 200; i++)
		{
			var plot = new Plot();
			plot.Plant(Daisy);
			plot.Water();
			plot.AdvanceNight();
			seen.Add(plot.Harvest().Count);
		}
		AssertThat(seen).ContainsExactlyInAnyOrder(3, 4, 5, 6);

		RipenDaisy(Plots[0]);
		_game.Harvest(Plots[0]);
		AssertThat(_game.InventoryCount).IsBetween(3, 6);
		_game.EndDay();
		AssertThat(_game.InventoryCount).IsEqual(0);
	}

	[TestCase]
	public void TheInventoryIsListedByFlowerThenColourThenLowestQuality()
	{
		var yellowTulip = new Seed(FlowerKind.Tulip, FlowerColour.Yellow);
		_game.Plant(Plots[0], yellowTulip);
		_game.Water(Plots[0]);
		_game.EndDay();
		_game.Plant(Plots[1], RedTulip);
		_game.Water(Plots[0]);
		_game.Water(Plots[1]);
		_game.EndDay();
		RipenDaisy(Plots[2]);
		// The yellow tulip ripened yesterday (★); the red tulip and the daisy ripen today (★★).
		foreach (var plot in Plots)
			_game.Harvest(plot);

		AssertThat(_game.Inventory.Select(s => s.Stem)).ContainsExactly(
			new Stem(FlowerKind.Daisy, FlowerColour.White, 2),
			new Stem(FlowerKind.Tulip, FlowerColour.Red, 2),
			new Stem(FlowerKind.Tulip, FlowerColour.Yellow, 1));
	}

	[TestCase]
	public void QualityIsBestOnTheRipeningDay()
	{
		RipenDaisy(Plots[0]);
		AssertThat(Plots[0].BaseQuality).IsEqual(2);

		_game.EndDay();
		AssertThat(Plots[0].BaseQuality).IsEqual(1);
	}

	[TestCase]
	public void FertilizerDoesNothingUntilNight()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.BuyFertilizer(FertilizerKind.Bloom);
		_game.Plant(Plots[0], RedTulip);
		_game.Fertilize(Plots[0], FertilizerKind.Growth);
		_game.Plant(Plots[1], Daisy);
		_game.Fertilize(Plots[1], FertilizerKind.Bloom);

		AssertThat(Plots[0].DaysLeft).IsEqual(2);
		AssertThat(Plots[1].BloomStarChance).IsEqual(0f);
	}

	[TestCase]
	public void FertilizerStaysInTheSoilOnlyUntilNight()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.Fertilize(Plots[0], FertilizerKind.Growth);
		_game.Plant(Plots[0], RedTulip);
		_game.Water(Plots[0]);
		_game.Fertilize(Plots[1], FertilizerKind.Growth);
		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);
		AssertThat(Plots[1].Fertilizer).IsNull();

		_game.Plant(Plots[1], RedTulip);
		_game.Water(Plots[1]);
		_game.EndDay();
		AssertThat(Plots[1].DaysLeft).IsEqual(1);
	}

	[TestCase]
	public void FertilizerOnlyHelpsThePlantInThePotAtNight()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.Plant(Plots[0], RedTulip);
		_game.Fertilize(Plots[0], FertilizerKind.Growth);
		_game.Clear(Plots[0]);
		AssertThat(Plots[0].Fertilizer).IsEqual(FertilizerKind.Growth);

		_game.Plant(Plots[0], RedTulip);
		_game.Water(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);
	}

	[TestCase]
	public void GrowthOnAPlantDueTomorrowStillRipensIt()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.Plant(Plots[0], Daisy);
		_game.Fertilize(Plots[0], FertilizerKind.Growth);
		_game.Water(Plots[0]);
		_game.EndDay();

		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);
	}

	[TestCase]
	public void APotTakesOneFertilizerAtATime()
	{
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.BuyFertilizer(FertilizerKind.Bloom);
		_game.Fertilize(Plots[0], FertilizerKind.Growth);

		AssertThat(_game.Fertilize(Plots[0], FertilizerKind.Bloom)).IsFalse();
		AssertThat(_game.FertilizerCount(FertilizerKind.Bloom)).IsEqual(1);
	}

	[TestCase]
	public void GrowthTakesADayOffEachDayItIsUsed()
	{
		var peony = new Seed(FlowerKind.Peony, FlowerColour.Pink);
		_game.BuyFertilizer(FertilizerKind.Growth);
		_game.BuyFertilizer(FertilizerKind.Growth);
		Plots[0].Plant(peony);
		_game.Fertilize(Plots[0], FertilizerKind.Growth);
		_game.Water(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].DaysLeft).IsEqual(2);

		AssertThat(_game.Fertilize(Plots[0], FertilizerKind.Growth)).IsTrue();
		_game.Water(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].Stage).IsEqual(PlotStage.Ready);
	}

	[TestCase]
	public void EachBloomRaisesTheChanceOfAnExtraStarButNeverMakesItCertain()
	{
		float perBloom = GameData.Instance.BloomStarChance;
		for (int i = 0; i < 5; i++)
			_game.BuyFertilizer(FertilizerKind.Bloom);
		_game.Plant(Plots[0], Daisy);
		_game.Fertilize(Plots[0], FertilizerKind.Bloom);
		_game.Water(Plots[0]);
		_game.EndDay();
		AssertThat(Plots[0].BloomStarChance).IsEqualApprox(perBloom, 0.0001f);

		float chance = Plots[0].BloomStarChance;
		for (int day = 0; day < 4; day++)
		{
			AssertThat(_game.Fertilize(Plots[0], FertilizerKind.Bloom)).IsTrue();
			_game.EndDay();
			if (Plots[0].Stage == PlotStage.Withered)
				break;
			AssertThat(Plots[0].BloomStarChance).IsGreater(chance);
			chance = Plots[0].BloomStarChance;
		}
		AssertThat(chance).IsLess(1f);
	}

	[TestCase]
	public void BloomIsLostWhenAPlantDies()
	{
		_game.BuyFertilizer(FertilizerKind.Bloom);
		_game.Plant(Plots[0], Daisy);
		_game.Fertilize(Plots[0], FertilizerKind.Bloom);
		_game.EndDay();
		_game.Clear(Plots[0]);
		_game.Plant(Plots[0], Daisy);

		AssertThat(Plots[0].BloomStarChance).IsEqual(0f);
	}

	[TestCase]
	public void DeadPlantsMustBeClearedBeforeReplanting()
	{
		_game.Plant(Plots[0], Daisy);
		_game.EndDay();

		AssertThat(_game.Plant(Plots[0], Daisy)).IsFalse();
		AssertThat(_game.Clear(Plots[0])).IsTrue();
		AssertThat(_game.Plant(Plots[0], Daisy)).IsTrue();
	}

	[TestCase]
	public void BuyingCostsMoneyAndNeedsEnough()
	{
		AssertThat(_game.BuySeed(RedTulip)).IsTrue();
		AssertThat(_game.Money).IsEqual(96);
		AssertThat(_game.SeedCount(RedTulip)).IsEqual(3);

		while (_game.Money >= 5)
			_game.BuyFertilizer(FertilizerKind.Growth);
		AssertThat(_game.BuyFertilizer(FertilizerKind.Growth)).IsFalse();
	}

	/// <summary>Plants a daisy (one-day flower), waters it and lets it ripen overnight.</summary>
	private void RipenDaisy(Plot plot)
	{
		_game.Plant(plot, Daisy);
		_game.Water(plot);
		foreach (var other in Plots)
			if (other != plot && other.IsGrowing)
				_game.Water(other);
		_game.EndDay();
	}
}
