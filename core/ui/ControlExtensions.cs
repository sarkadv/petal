using Godot;

namespace Petal;

/// <summary>Small animations shared by screens.</summary>
public static class ControlExtensions
{
	private static readonly StringName BounceTween = "bounce_tween";

	/// <summary>A quick bounce to show something worked or arrived, e.g. a pot after a tool (GREENHOUSE.md §4).</summary>
	public static void Bounce(this Control control, float size = 1.05f, float delay = 0f)
	{
		if (control.HasMeta(BounceTween))
			control.GetMeta(BounceTween).As<Tween>().Kill();
		control.PivotOffset = control.Size / 2;
		var tween = control.CreateTween();
		tween.TweenInterval(delay);
		tween.TweenProperty(control, TweenPaths.Scale, new Vector2(size, size), 0.08f);
		tween.TweenProperty(control, TweenPaths.Scale, Vector2.One, 0.12f);
		control.SetMeta(BounceTween, tween);
	}
}
