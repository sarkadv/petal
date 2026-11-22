using System;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// Short animations over the greenhouse pots (GREENHOUSE.md §4): the tool doing its gesture over the pot it's used on
/// and what it drops into the soil, plants that leave a pot shrinking away, and harvested flowers flying to the
/// inventory basket. Only for show: the game has already changed when one starts, and none of them block taps.
/// Positions are in the stage's coordinates; this layer covers the stage, above the pots and the shelf.
/// </summary>
public partial class PotEffects : Control
{
	/// <summary>How long a tool takes to fade in over the pot.</summary>
	private const float AppearSeconds = 0.1f;
	/// <summary>How long a tool takes to tip over the pot (or the trowel to dig in), once it has appeared.</summary>
	private const float TipSeconds = 0.15f;
	/// <summary>When the tool reaches the soil and the pot shows the change, counted from the tap.</summary>
	private const float HitSeconds = 0.4f;
	/// <summary>How long a tool takes to right itself or lift out while fading away; also how long anything fades out.</summary>
	private const float LeaveSeconds = 0.15f;
	/// <summary>How long anything flying (a drop, a clod, a flower) takes to fade in before it moves.</summary>
	private const float FlyFadeInSeconds = 0.05f;

	/// <summary>How big a tool is drawn over a pot, compared with its art (and its size on the shelf).</summary>
	private const float CameoScale = 0.85f;
	/// <summary>How many times a shaken tool (the seed packet) wiggles while it pours.</summary>
	private const int ShakeWiggles = 3;
	/// <summary>How far each wiggle turns the tool either side of its tipped angle, in radians (about 11°).</summary>
	private const float ShakeAngle = 0.2f;

	/// <summary>The watering can hovers up-left of the pot and tips clockwise; its spout is right of its top.</summary>
	private static readonly Gesture CanTip = new(new Vector2(-56, -64), 0.45f, new Vector2(52, -26));
	/// <summary>The fertilizer sack hovers up-right of the pot and tips anticlockwise; its mouth is at its top.</summary>
	private static readonly Gesture SackTip = new(new Vector2(58, -62), -0.7f, new Vector2(0, -30));
	/// <summary>The seed packet hovers up-right of the pot, tips anticlockwise and shakes; it opens at its top.</summary>
	private static readonly Gesture PacketTip = new(new Vector2(52, -72), -0.6f, new Vector2(0, -46), Shake: true);

	/// <summary>How many drops, seeds or granules a tool pours into a pot.</summary>
	private const int PouredBits = 4;
	/// <summary>The time between one poured bit landing in the soil and the next.</summary>
	private const float PourGapSeconds = 0.05f;
	/// <summary>How long a drop, seed or granule takes to fall from the tool into the soil.</summary>
	private const float FallSeconds = 0.18f;
	/// <summary>How far from a pot's centre things dropped into its soil land.</summary>
	private const float SoilRadius = 42f;

	/// <summary>Where the trowel's centre is when dug in, from the pot's centre.</summary>
	private static readonly Vector2 TrowelDug = new(-28, -28);
	/// <summary>How far the trowel is lifted out of the soil before digging in and after.</summary>
	private static readonly Vector2 TrowelLift = new(-24, -24);
	/// <summary>The trowel's turn as it appears, in radians. The blade points up and right in the art.</summary>
	private const float TrowelRaisedAngle = 1.1f;
	/// <summary>The trowel's turn while dug in, with the blade pointing down into the pot.</summary>
	private const float TrowelDugAngle = 1.4f;
	/// <summary>The trowel's turn as it lifts out and flicks the soil.</summary>
	private const float TrowelFlickAngle = 0.9f;
	/// <summary>How many clods of soil the trowel flicks out of a pot.</summary>
	private const int FlickedClods = 5;
	/// <summary>How far past the soil's edge the clods land, at least and at most.</summary>
	private const float ClodNearest = 20f;
	private const float ClodFarthest = 40f;
	/// <summary>How far upwards the clods' path is pulled (see Fly).</summary>
	private const float ClodLift = 40f;
	/// <summary>The time between one clod setting off and the next.</summary>
	private const float ClodGapSeconds = 0.02f;
	private const float ClodFlightSeconds = 0.3f;

	/// <summary>The size in pixels of a harvested flower as it lifts out of the pot.</summary>
	private const float FlowerSize = 48f;
	/// <summary>How small a harvested flower has become, compared with <see cref="FlowerSize"/>, by the basket.</summary>
	private const float FlowerLandingScale = 0.5f;
	/// <summary>How far, at most, a flower sets off from its bloom in each direction, so they don't stack up.</summary>
	private const float BloomJitter = 8f;
	/// <summary>How far upwards the flowers' path to the basket is pulled (see Fly).</summary>
	private const float FlowerLift = 160f;
	/// <summary>The time between one flower setting off and the next.</summary>
	private const float FlowerGapSeconds = 0.08f;
	private const float FlowerFlightSeconds = 0.6f;
	/// <summary>How big the basket bounces as each flower lands in it.</summary>
	private const float BasketBounce = 1.15f;

	/// <summary>How small plants leaving a pot become, compared with their size, before they're gone.</summary>
	private const float ShrinkScale = 0.3f;
	private const float ShrinkSeconds = 0.25f;

	/// <summary>A harvested flower reached the basket (see <see cref="Harvest"/>).</summary>
	public event Action StemLanded;

	[Export] private PackedScene _packet;
	[Export] private Texture2D _can;
	[Export] private Texture2D _sack;
	[Export] private Texture2D _trowel;
	[Export] private Texture2D _drop;
	[Export] private Texture2D _speck;
	[Export] private Texture2D _bloom;
	/// <summary>Tint of the seeds shaken from the packet.</summary>
	[Export] private Color _seedColour;
	/// <summary>Tint of the clods the trowel flicks out.</summary>
	[Export] private Color _soilColour;

	/// <summary>
	/// How a tool tips over a pot: where it hovers (<paramref name="Hover"/>, from the pot's centre), how far it tips
	/// (<paramref name="Angle"/>, in radians, clockwise), where its opening is (<paramref name="Spout"/>, in art pixels
	/// from the art's centre) and whether it shakes while it pours.
	/// </summary>
	private readonly record struct Gesture(Vector2 Hover, float Angle, Vector2 Spout, bool Shake = false);

	/// <summary>A seed packet with the seed's flower on it shakes seeds into the pot, and sprouts come up.</summary>
	public void Plant(PlotView pot, Color flower)
	{
		var centre = Centre(pot);
		var packet = _packet.Instantiate<TextureRect>();
		packet.GetNode<CanvasItem>("%Flower").SelfModulate = flower;
		var opening = Tip(Cameo(packet), centre, PacketTip);
		Pour(_speck, _seedColour, opening, centre);
		pot.ShowPlanted(HitSeconds);
		pot.Bounce(delay: HitSeconds);
	}

	/// <summary>The watering can tips over the pot from its top left and pours, and the soil darkens.</summary>
	public void Water(PlotView pot)
	{
		var centre = Centre(pot);
		var spout = Tip(Cameo(_can), centre, CanTip);
		Pour(_drop, Colors.White, spout, centre);
		pot.ShowWatered(HitSeconds);
		pot.Bounce(delay: HitSeconds);
	}

	/// <summary>The sack tips over the pot from its top right and pours granules in the fertilizer's colour.</summary>
	public void Fertilize(PlotView pot, Color soilTint)
	{
		var centre = Centre(pot);
		var mouth = Tip(Cameo(_sack), centre, SackTip);
		// The soil tint is see-through so it only stains the soil; the granules are the same colour, but solid.
		var granules = new Color(soilTint, 1f);
		Pour(_speck, granules, mouth, centre);
		pot.ShowFertilized(HitSeconds);
		pot.Bounce(delay: HitSeconds);
	}

	/// <summary>The trowel digs into the pot and flicks out soil, and the old plants shrink away.</summary>
	public void Clear(PlotView pot, Control plants)
	{
		var centre = Centre(pot);
		var tool = Cameo(_trowel);
		var dug = centre + TrowelDug - tool.Size / 2;
		var raised = dug + TrowelLift;
		tool.Position = raised;
		tool.Rotation = TrowelRaisedAngle;
		var tween = Appear(tool);
		tween.TweenProperty(tool, TweenPaths.Position, dug, TipSeconds).SetEase(Tween.EaseType.In);
		tween.Parallel().TweenProperty(tool, TweenPaths.Rotation, TrowelDugAngle, TipSeconds);
		float dugIn = HitSeconds - AppearSeconds - TipSeconds;
		tween.TweenInterval(dugIn);
		tween.TweenProperty(tool, TweenPaths.Position, raised, LeaveSeconds).SetEase(Tween.EaseType.Out);
		tween.Parallel().TweenProperty(tool, TweenPaths.Rotation, TrowelFlickAngle, LeaveSeconds);
		FadeOut(tween.Parallel(), tool);

		for (int i = 0; i < FlickedClods; i++)
		{
			var clod = Sprite(_speck, _speck.GetSize(), _soilColour);
			var outward = Vector2.FromAngle(Random(0, MathF.Tau))
				* Random(SoilRadius + ClodNearest, SoilRadius + ClodFarthest);
			var flight = Fly(clod, centre, centre + outward, ClodLift, HitSeconds + i * ClodGapSeconds,
				ClodFlightSeconds, Tween.EaseType.Out);
			FadeOut(flight, clod);
		}
		ShrinkAway(pot, plants, HitSeconds);
		pot.Bounce(delay: HitSeconds);
	}

	/// <summary>
	/// The flowers lift out of the pot and fly to the basket one by one, a flower per stem, raising
	/// <see cref="StemLanded"/> as each arrives and bouncing the basket. The leaves shrink away.
	/// </summary>
	public void Harvest(PlotView pot, Control leaves, Color flower, int stems, Control basket)
	{
		var starts = pot.Blooms.Select(Centre).ToArray();
		var to = Centre(basket);
		for (int i = 0; i < stems; i++)
		{
			var jitter = new Vector2(Random(-BloomJitter, BloomJitter), Random(-BloomJitter, BloomJitter));
			var from = starts[i % starts.Length] + jitter;
			var head = Sprite(_bloom, Vector2.One * FlowerSize, flower);
			var tween = Fly(head, from, to, FlowerLift, i * FlowerGapSeconds, FlowerFlightSeconds,
				Tween.EaseType.InOut);
			tween.Parallel().TweenProperty(head, TweenPaths.Scale, Vector2.One * FlowerLandingScale, FlowerFlightSeconds);
			tween.TweenCallback(Callable.From(() =>
			{
				StemLanded?.Invoke();
				basket.Bounce(BasketBounce);
			}));
			tween.TweenCallback(new Callable(head, Node.MethodName.QueueFree));
		}
		ShrinkAway(pot, leaves, 0f);
		pot.Bounce();
	}

	/// <summary>
	/// The tool fades in over the pot at <paramref name="pot"/>, tips over as the <paramref name="gesture"/> says
	/// (shaking if it says so), rights itself and fades out. Returns where its spout is while tipped.
	/// </summary>
	private static Vector2 Tip(TextureRect tool, Vector2 pot, Gesture gesture)
	{
		var centre = pot + gesture.Hover;
		SetCentre(tool, centre);
		var tween = Appear(tool);
		tween.TweenProperty(tool, TweenPaths.Rotation, gesture.Angle, TipSeconds)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		// Stays tipped until the last bit poured from it has landed (see Pour).
		float pouring = HitSeconds + FallSeconds - AppearSeconds - TipSeconds;
		if (gesture.Shake)
			for (int i = 0; i < ShakeWiggles; i++)
				tween.TweenProperty(tool, TweenPaths.Rotation, gesture.Angle + (i % 2 == 0 ? ShakeAngle : -ShakeAngle),
					pouring / ShakeWiggles);
		else
			tween.TweenInterval(pouring);
		tween.TweenProperty(tool, TweenPaths.Rotation, 0f, LeaveSeconds);
		FadeOut(tween.Parallel(), tool);
		// The tool turns around its centre, so the spout does too; the art is drawn smaller over the pot.
		return centre + (gesture.Spout * CameoScale).Rotated(gesture.Angle);
	}

	/// <summary>A few drops, seeds or granules fall from <paramref name="from"/> into the soil, landing around the hit.</summary>
	private void Pour(Texture2D texture, Color tint, Vector2 from, Vector2 pot)
	{
		for (int i = 0; i < PouredBits; i++)
		{
			var bit = Sprite(texture, texture.GetSize(), tint);
			var landing = pot + Vector2.FromAngle(Random(0, MathF.Tau)) * Random(0, SoilRadius);
			// Counted back from where it lands: the first bit on the hit, the others a gap apart after it.
			float landsAt = HitSeconds + i * PourGapSeconds;
			float start = landsAt - FallSeconds - FlyFadeInSeconds;
			FadeOut(Fly(bit, from, landing, 0f, start, FallSeconds, Tween.EaseType.In), bit);
		}
	}

	private void ShrinkAway(PlotView pot, Control plants, float delay)
	{
		AddChild(plants);
		plants.SetAnchorsPreset(LayoutPreset.TopLeft);
		plants.Size = pot.Size;
		SetCentre(plants, Centre(pot));
		plants.PivotOffset = plants.Size / 2;
		var tween = plants.CreateTween();
		tween.TweenInterval(delay);
		tween.TweenProperty(plants, TweenPaths.Scale, Vector2.One * ShrinkScale, ShrinkSeconds)
			.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
		FadeOut(tween.Parallel(), plants, ShrinkSeconds);
	}

	/// <summary>
	/// After <paramref name="delay"/>, the sprite shows at <paramref name="from"/> and moves to <paramref name="to"/>
	/// along a curve pulled <paramref name="lift"/> upwards (its highest point is half that above the straight line).
	/// </summary>
	private static Tween Fly(Control sprite, Vector2 from, Vector2 to, float lift, float delay, float seconds,
		Tween.EaseType ease)
	{
		var bend = (from + to) / 2 + Vector2.Up * lift;
		SetCentre(sprite, from);
		var tween = sprite.CreateTween();
		tween.TweenInterval(delay);
		tween.TweenProperty(sprite, TweenPaths.ModulateAlpha, 1f, FlyFadeInSeconds);
		// A quadratic Bézier curve: as t goes 0 → 1 it sets off from `from` towards `bend`, then curves into `to`.
		Vector2 Curve(float t) => from.Lerp(bend, t).Lerp(bend.Lerp(to, t), t);
		tween.TweenMethod(Callable.From<float>(t => SetCentre(sprite, Curve(t))), 0f, 1f, seconds)
			.SetTrans(Tween.TransitionType.Quad).SetEase(ease);
		return tween;
	}

	private static Tween Appear(Control sprite)
	{
		var tween = sprite.CreateTween();
		tween.TweenProperty(sprite, TweenPaths.ModulateAlpha, 1f, AppearSeconds);
		return tween;
	}

	/// <summary>Fades the sprite out where the tween leaves it and removes it.</summary>
	private static void FadeOut(Tween tween, Control sprite, float seconds = LeaveSeconds)
	{
		tween.TweenProperty(sprite, TweenPaths.ModulateAlpha, 0f, seconds);
		tween.TweenCallback(new Callable(sprite, Node.MethodName.QueueFree));
	}

	/// <summary>A tool over a pot, a little smaller than on the shelf.</summary>
	private TextureRect Cameo(Texture2D art) => Cameo(Rect(art, Colors.White));

	private TextureRect Cameo(TextureRect tool) => Add(tool, tool.Texture.GetSize() * CameoScale);

	private TextureRect Sprite(Texture2D texture, Vector2 size, Color tint) => Add(Rect(texture, tint), size);

	/// <summary>Puts the sprite on this layer, hidden until its animation shows it.</summary>
	private TextureRect Add(TextureRect sprite, Vector2 size)
	{
		sprite.Modulate = Colors.Transparent;
		AddChild(sprite);
		sprite.Size = size;
		sprite.PivotOffset = size / 2;
		return sprite;
	}

	/// <summary>A new picture of <paramref name="texture"/>, not yet on the layer or sized.</summary>
	private static TextureRect Rect(Texture2D texture, Color tint) => new()
	{
		Texture = texture,
		// Can be given any size, not only the art's own.
		ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
		// The art is scaled to fit that size without being squashed.
		StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
		// Taps go straight through to the pots underneath.
		MouseFilter = MouseFilterEnum.Ignore,
		// Tints the art: its colours are multiplied by this, so white leaves it as drawn. Fading in and out uses
		// Modulate instead, so the tint and the fade don't overwrite each other.
		SelfModulate = tint,
	};

	/// <summary>Where the middle of <paramref name="control"/> is on this layer, wherever it sits in the scene.</summary>
	private Vector2 Centre(Control control)
	{
		// Its middle in the game's global coordinates, the same for every node...
		var global = control.GetGlobalRect().GetCenter();
		// ...turned into this layer's own coordinates, by undoing the layer's position and scale.
		return GetGlobalTransform().AffineInverse() * global;
	}

	private static void SetCentre(Control control, Vector2 centre) => control.Position = centre - control.Size / 2;

	private static float Random(float from, float to) => (float)GD.RandRange(from, to);
}
