using Godot;

namespace Petal;

/// <summary>
/// A tool on the greenhouse shelf (GREENHOUSE.md §3): its art with its name under it, and for Seeds and Fertilize a
/// count badge.
/// </summary>
public partial class ToolButton : Button
{
	[Export] private Texture2D _art;
	/// <summary>Translation key.</summary>
	[Export] private string _label = "";

	private Control _badge;
	private TextureRect _badgeIcon;
	private Label _badgeCount;

	public override void _Ready()
	{
		GetNode<TextureRect>("%Art").Texture = _art;
		GetNode<Label>("%Name").Text = _label;
		_badge = GetNode<Control>("%Badge");
		_badgeIcon = GetNode<TextureRect>("%BadgeIcon");
		_badgeCount = GetNode<Label>("%BadgeCount");
	}

	/// <summary>Shows "×N" on the badge, with the icon only while there's at least one.</summary>
	public void ShowBadge(int count, Texture2D icon, Color tint)
	{
		_badge.Show();
		_badgeIcon.Visible = count > 0;
		_badgeIcon.Texture = icon;
		_badgeIcon.SelfModulate = tint;
		_badgeCount.Text = $"×{count}";
	}
}
