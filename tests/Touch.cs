using System.Threading.Tasks;
using GdUnit4;
using Godot;

namespace Petal.Tests;

/// <summary>
/// Taps and holds on screen with real mouse input, as a player would. Events go straight to the scene's viewport in
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
