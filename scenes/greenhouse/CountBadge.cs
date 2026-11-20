using Godot;

namespace Petal;

/// <summary>
/// The small "×N" chip on the Seeds and Fertilize tools, the inventory button, seed packets, inventory items and
/// fertilizer rows (GREENHOUSE.md §2, §3), optionally with an icon before the count.
/// </summary>
public partial class CountBadge : PanelContainer
{
	private TextureRect _icon;
	private Label _count;

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
		_count = GetNode<Label>("%Count");
	}

	/// <summary>Shows "×N", with the icon (if any) only while there's at least one.</summary>
	public void SetCount(int count, Texture2D icon = null, Color? tint = null)
	{
		_icon.Visible = icon != null && count > 0;
		_icon.Texture = icon;
		_icon.SelfModulate = tint ?? Colors.White;
		_count.Text = $"×{count}";
	}
}
