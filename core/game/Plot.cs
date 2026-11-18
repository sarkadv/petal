using System;

namespace Petal;

public enum PlotStage { Empty, Seedling, Growing, Ready, Withered, Dead }

/// <summary>
/// One greenhouse plot and the plant in it, moving through the stages in GAMEPLAY.md §4.1 one night at a time.
/// <see cref="GameState"/> checks the Can… members before calling an action.
/// </summary>
public class Plot
{
	private const int DaysUntilWithered = 2;

	public PlotStage Stage { get; private set; }

	/// <summary>What was planted. Kept on a dead or withered plant until it's cleared.</summary>
	public Seed? Seed { get; private set; }

	public bool Watered { get; private set; }

	/// <summary>Fertilizer in the soil. It works on whatever plant is in the pot that night, then it's gone.</summary>
	public FertilizerKind? Fertilizer { get; private set; }

	/// <summary>Nights until the plant is ready.</summary>
	public int DaysLeft { get; private set; }

	private int _daysReady;

	/// <summary>
	/// How many times the plant has had Bloom fertilizer. Each one raises the chance of an extra star. A dead or
	/// withered plant can't be harvested, so it loses them when cleared.
	/// </summary>
	private int _blooms;

	public bool IsGrowing => Stage is PlotStage.Seedling or PlotStage.Growing;
	public bool IsAlive => IsGrowing || Stage == PlotStage.Ready;
	public bool CanPlant => Stage == PlotStage.Empty;
	/// <summary>Any pot can be watered once a day, though only seedlings and growing plants need it (§4.1).</summary>
	public bool CanWater => !Watered;
	public bool CanHarvest => Stage == PlotStage.Ready;
	public bool CanClear => Stage != PlotStage.Empty;

	/// <summary>
	/// Any pot can be fertilized once a day (§4.2). Overnight, Growth takes a day off and Bloom raises the chance of an
	/// extra star for the plant in the pot then.
	/// </summary>
	public bool CanFertilize => Fertilizer == null;

	/// <summary>Stars the stems get if harvested now, before Bloom (§4.4): ★, +1 on the day it ripens.</summary>
	public int BaseQuality => 1 + (_daysReady == 0 ? 1 : 0);

	/// <summary>
	/// Chance of one more star at harvest (§4.2). Each Bloom is its own try at the star, so the chance rises with
	/// every use but never reaches certainty.
	/// </summary>
	public float BloomStarChance => 1f - MathF.Pow(1f - GameData.Instance.BloomStarChance, _blooms);

	public void Plant(Seed seed)
	{
		Stage = PlotStage.Seedling;
		Seed = seed;
		DaysLeft = seed.Flower.GrowDays;
	}

	public void Water() => Watered = true;

	public void Fertilize(FertilizerKind fertilizer) => Fertilizer = fertilizer;

	private void Feed(FertilizerKind fertilizer)
	{
		if (fertilizer == FertilizerKind.Bloom)
			_blooms++;
		else if (IsGrowing)
			DaysLeft = Math.Max(1, DaysLeft - 1); // leaves the last day for tonight's growth to ripen it
	}

	public (Stem Stem, int Count) Harvest()
	{
		var seed = Seed!.Value;
		var data = GameData.Instance;
		int stems = Random.Shared.Next(data.MinHarvestStems, data.MaxHarvestStems + 1);
		int bloomStar = Random.Shared.NextSingle() < BloomStarChance ? 1 : 0;
		var harvest = (new Stem(seed.Kind, seed.Colour, BaseQuality + bloomStar), stems);
		Clear();
		return harvest;
	}

	/// <summary>Takes the plant out. Water and fertilizer stay in the soil for whatever is planted there today.</summary>
	public void Clear()
	{
		Stage = PlotStage.Empty;
		Seed = null;
		_blooms = 0;
		DaysLeft = 0;
		_daysReady = 0;
	}

	public void AdvanceNight()
	{
		if (Fertilizer is { } fertilizer && IsAlive)
			Feed(fertilizer);

		if (IsGrowing)
		{
			if (!Watered)
				Stage = PlotStage.Dead;
			else if (--DaysLeft == 0)
				Stage = PlotStage.Ready;
			else
				Stage = PlotStage.Growing;
		}
		else if (Stage == PlotStage.Ready && ++_daysReady >= DaysUntilWithered)
			Stage = PlotStage.Withered;

		Watered = false;
		Fertilizer = null;
	}
}
