using Godot;

namespace Petal;

/// <summary>Property paths for tweens, built from Godot's generated names so a typo doesn't compile.</summary>
public static class TweenPaths
{
	public static readonly NodePath Position = Control.PropertyName.Position.ToString();
	public static readonly NodePath Rotation = Control.PropertyName.Rotation.ToString();
	public static readonly NodePath Scale = Control.PropertyName.Scale.ToString();
	public static readonly NodePath Modulate = CanvasItem.PropertyName.Modulate.ToString();
	public static readonly NodePath ModulateAlpha = $"{CanvasItem.PropertyName.Modulate}:a";
}
