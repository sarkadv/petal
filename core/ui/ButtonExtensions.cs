using Godot;

namespace Petal;

/// <summary>Helpers for buttons built in code.</summary>
public static class ButtonExtensions
{
	private static readonly StringName[] IconColours =
	{
		"icon_normal_color", "icon_hover_color", "icon_pressed_color", "icon_hover_pressed_color", "icon_focus_color",
	};

	/// <summary>Tints the button's icon in every state, e.g. a white dot in a flower's colour.</summary>
	public static void SetIconTint(this Button button, Color tint)
	{
		foreach (var name in IconColours)
			button.AddThemeColorOverride(name, tint);
	}
}
