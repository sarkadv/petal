using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// The seeds to choose from when Seeds is held (GAMEPLAY.md §4.2, GREENHOUSE.md §3), shown below the tool: a packet
/// per seed in stock, in a row per flower under its name. A row with more colours than fit scrolls sideways, and
/// more rows than fit scroll down, each with a scrollbar. Tapping a packet closes it and raises <see cref="SeedPicked"/>.
/// </summary>
public partial class SeedPopover : ChoicesPopover
{
	private const float Width = 580;
	private const float MaxRowsHeight = 600;
	/// <summary>How far a press may scroll the packets before letting go no longer picks one.</summary>
	private const float ScrollSlop = 8;

	[Export] private PackedScene _packetScene;

	public event Action<Seed> SeedPicked;

	private ScrollContainer _scroll;
	private Vector2 _scrollAtPress;

	public override void _Ready()
	{
		base._Ready();
		_scroll = GetNode<ScrollContainer>("%Scroll");
	}

	public async void ShowSeeds(Control anchor, IEnumerable<(Seed Seed, int Count)> seeds, Seed? chosen)
	{
		Rows.Clear();
		float rowWidth = RowWidth();
		(SeedPacket Packet, ScrollContainer Row)? chosenPacket = null;
		foreach (var flower in seeds.GroupBy(s => s.Seed.Kind))
		{
			var row = Rows.AddRow<ItemRow>();
			row.Heading = flower.First().Seed.Flower.Name;
			var rowScroll = row.Scroll;
			var packets = row.Items;
			foreach (var (seed, count) in flower)
			{
				var packet = _packetScene.Instantiate<SeedPacket>();
				packet.ButtonPressed = seed == chosen;
				packets.AddChild(packet);
				packet.SetSeed(seed, count);
				packet.ButtonDown += () => _scrollAtPress = ScrollOf(rowScroll);
				packet.Pressed += () => OnPacketPressed(packet, rowScroll);
				if (seed == chosen)
					chosenPacket = (packet, rowScroll);
			}
			// Decided up front, so the panel is measured with the scrollbars it will have.
			rowScroll.HorizontalScrollMode = packets.GetCombinedMinimumSize().X > rowWidth
				? ScrollContainer.ScrollMode.ShowAlways
				: ScrollContainer.ScrollMode.Disabled;
		}

		_scroll.Visible = !Rows.IsEmpty;
		_scroll.CustomMinimumSize = new Vector2(0, Mathf.Min(Rows.GetCombinedMinimumSize().Y, MaxRowsHeight));
		_scroll.ScrollVertical = 0;
		await ShowRows(anchor, Width);

		if (!IsInstanceValid(this) || !Visible || chosenPacket is not { } shown || !IsInstanceValid(shown.Packet))
			return;
		shown.Row.EnsureControlVisible(shown.Packet);
		_scroll.EnsureControlVisible(shown.Packet);
	}

	private void OnPacketPressed(SeedPacket packet, ScrollContainer row)
	{
		// Letting go after dragging the packets along is a scroll, not a pick.
		if (ScrollOf(row).DistanceTo(_scrollAtPress) > ScrollSlop)
		{
			packet.SetPressedNoSignal(!packet.ButtonPressed);
			return;
		}
		Hide();
		SeedPicked?.Invoke(packet.Seed);
	}

	/// <summary>Room for a row's packets: the panel less its padding and the space kept for the scrollbar.</summary>
	private float RowWidth()
	{
		var margin = GetNode<Control>("%Margin");
		return Width - Panel.GetThemeStylebox("panel").GetMinimumSize().X
			- margin.GetThemeConstant("margin_left") - margin.GetThemeConstant("margin_right")
			- _scroll.GetVScrollBar().GetCombinedMinimumSize().X;
	}

	private Vector2 ScrollOf(ScrollContainer row) => new(row.ScrollHorizontal, _scroll.ScrollVertical);
}
