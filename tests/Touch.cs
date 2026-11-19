using System.Threading.Tasks;
using GdUnit4;
using Godot;

namespace Petal.Tests;

/// <summary>
/// Taps, holds and drags on screen with real mouse input, as a player would. Events go straight to the scene's viewport in
/// its own (720×1280 layout) coordinates, so they land where a control is drawn whatever the window size.
/// </summary>
public static class Touch
{
	private const uint HoldMillis = (uint)(Greenhouse.HoldSeconds * 1000) + 200;

	public static Task Tap(this ISceneRunner runner, Control target) =>
		runner.TapAt(target.GetGlobalRect().GetCenter());

	public static async Task TapAt(this ISceneRunner runner, Vector2 position)
	{
		Press(runner, position, true);
		Press(runner, position, false);
		await runner.AwaitInputProcessed();
	}

	public static async Task Hold(this ISceneRunner runner, Control target)
	{
		var position = target.GetGlobalRect().GetCenter();
		Press(runner, position, true);
		await runner.AwaitInputProcessed();
		await runner.AwaitMillis(HoldMillis);
		Press(runner, position, false);
		await runner.AwaitInputProcessed();
	}

	/// <summary>Presses on the target, slides by <paramref name="by"/> in a few steps and lets go.</summary>
	public static async Task Drag(this ISceneRunner runner, Control target, Vector2 by)
	{
		const int steps = 5;
		// Scroll containers only follow drags on a touchscreen; this makes them follow the mouse as they would a finger.
		bool emulating = Input.EmulateTouchFromMouse;
		Input.EmulateTouchFromMouse = true;
		var from = target.GetGlobalRect().GetCenter();
		Press(runner, from, true);
		await runner.AwaitInputProcessed();
		for (int i = 1; i <= steps; i++)
		{
			var position = from + by * i / steps;
			var motion = new InputEventMouseMotion
			{
				ButtonMask = MouseButtonMask.Left,
				Position = position,
				GlobalPosition = position,
				Relative = by / steps,
			};
			runner.Scene().GetViewport().PushInput(motion, true);
			await runner.AwaitInputProcessed();
		}
		Press(runner, from + by, false);
		await runner.AwaitInputProcessed();
		Input.EmulateTouchFromMouse = emulating;
	}

	private static void Press(ISceneRunner runner, Vector2 position, bool down)
	{
		var press = new InputEventMouseButton
		{
			ButtonIndex = MouseButton.Left,
			ButtonMask = down ? MouseButtonMask.Left : 0,
			Pressed = down,
			Position = position,
			GlobalPosition = position,
		};
		runner.Scene().GetViewport().PushInput(press, true);
	}
}
