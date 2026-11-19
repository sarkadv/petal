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

	private CountBadge _badge;

	public override void _Ready()
	{
		GetNode<TextureRect>("%Art").Texture = _art;
		GetNode<Label>("%Name").Text = _label;
		_badge = GetNode<CountBadge>("%Badge");
	}

	public void ShowBadge(int count, Texture2D icon, Color tint)
	{
		_badge.Show();
		_badge.SetCount(count, icon, tint);
	}
}
