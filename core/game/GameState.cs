using System;
using System.Collections.Generic;
using System.Linq;

namespace Petal;

/// <summary>
/// The game being played: day, money, greenhouse plots, seed and fertilizer stock, and today's harvested
/// stems (GAMEPLAY.md §4, §9). Plain C# so it can be saved later (§2). Actions return false when they
/// aren't possible; screens listen to <see cref="Changed"/> to redraw.
/// </summary>
public class GameState
{
	private static GameState _current;

	/// <summary>Starts a new game if none is running, so any scene can be launched directly.</summary>
	public static GameState Current => _current ??= NewGame();

	/// <summary>Replaces the current game with a new one.</summary>
	public static void StartNewGame() => _current = NewGame();

	public event Action Changed;

	public int Day { get; private set; } = 1;
	public int Money { get; private set; }
	public IReadOnlyList<Plot> Plots => _plots;
	public int StemCount => _stems.Values.Sum();

	private readonly List<Plot> _plots = new();
	private readonly Dictionary<Seed, int> _seeds = new();
	private readonly Dictionary<FertilizerKind, int> _fertilizers = new();
	private readonly Dictionary<Stem, int> _stems = new();
	private readonly HashSet<FlowerKind> _unlockedFlowers = new();

	public static GameState NewGame()
	{
		var data = GameData.Instance;
		var game = new GameState { Money = data.StartingMoney };
		for (int i = 0; i < data.StartingPlots; i++)
			game._plots.Add(new Plot());
		foreach (var (key, count) in data.StarterSeeds)
			game._seeds[Seed.Parse(key)] = count;
		foreach (var flower in data.Flowers.Where(f => f.UnlockedAtStart))
			game._unlockedFlowers.Add(flower.Kind);
		return game;
	}

	/// <summary>Unlocked flowers in catalogue order.</summary>
	public IEnumerable<FlowerData> UnlockedFlowers =>
		GameData.Instance.Flowers.Where(f => _unlockedFlowers.Contains(f.Kind));

	/// <summary>Seeds in stock, in catalogue order.</summary>
	public IEnumerable<(Seed Seed, int Count)> Seeds =>
		_seeds.OrderBy(s => GameData.Instance.Flowers.IndexOf(s.Key.Flower))
			.ThenBy(s => s.Key.Colour)
			.Select(s => (s.Key, s.Value));

	/// <summary>Fertilizers in stock, in catalogue order.</summary>
	public IEnumerable<(FertilizerData Fertilizer, int Count)> Fertilizers =>
		GameData.Instance.Fertilizers
			.Select(f => (f, FertilizerCount(f.Kind)))
			.Where(f => f.Item2 > 0);

	public int SeedCount(Seed seed) => _seeds.GetValueOrDefault(seed);
	public int FertilizerCount(FertilizerKind fertilizer) => _fertilizers.GetValueOrDefault(fertilizer);

	public bool CanBuy(Seed seed) => Money >= seed.Flower.SeedPrice;
	public bool CanBuy(FertilizerKind fertilizer) => Money >= GameData.Instance.Fertilizer(fertilizer).Price;

	public bool BuySeed(Seed seed) => CanBuy(seed) && Buy(_seeds, seed, seed.Flower.SeedPrice);

	public bool BuyFertilizer(FertilizerKind fertilizer) =>
		CanBuy(fertilizer) && Buy(_fertilizers, fertilizer, GameData.Instance.Fertilizer(fertilizer).Price);

	public bool Plant(Plot plot, Seed seed)
	{
		if (!plot.CanPlant || SeedCount(seed) == 0) return false;
		Add(_seeds, seed, -1);
		plot.Plant(seed);
		return NotifyChanged();
	}

	public bool Water(Plot plot)
	{
		if (!plot.CanWater) return false;
		plot.Water();
		return NotifyChanged();
	}

	public bool Fertilize(Plot plot, FertilizerKind fertilizer)
	{
		if (!plot.CanFertilize || FertilizerCount(fertilizer) == 0) return false;
		Add(_fertilizers, fertilizer, -1);
		plot.Fertilize(fertilizer);
		return NotifyChanged();
	}

	public bool Harvest(Plot plot)
	{
		if (!plot.CanHarvest) return false;
		var (stem, count) = plot.Harvest();
		Add(_stems, stem, count);
		return NotifyChanged();
	}

	public bool Clear(Plot plot)
	{
		if (!plot.CanClear) return false;
		plot.Clear();
		return NotifyChanged();
	}

	/// <summary>Today's stems wilt (§4.5), the plots go through the night (§4.1) and the next morning starts.</summary>
	public void EndDay()
	{
		_stems.Clear();
		foreach (var plot in _plots)
			plot.AdvanceNight();
		Day++;
		NotifyChanged();
	}

	private bool Buy<T>(Dictionary<T, int> stock, T item, int price)
	{
		Money -= price;
		Add(stock, item, 1);
		return NotifyChanged();
	}

	private bool NotifyChanged()
	{
		Changed?.Invoke();
		return true;
	}

	private static void Add<T>(Dictionary<T, int> stock, T item, int amount)
	{
		int count = stock.GetValueOrDefault(item) + amount;
		if (count > 0)
			stock[item] = count;
		else
			stock.Remove(item);
	}
}
