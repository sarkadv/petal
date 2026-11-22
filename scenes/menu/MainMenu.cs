using Godot;

namespace Petal;

/// <summary>
/// Title screen (GAMEPLAY.md §2) with Play and Settings. The <see cref="SettingsScreen"/> lives in
/// this scene; tapping Settings swaps it in with a short fade, Back swaps home back in.
/// </summary>
public partial class MainMenu : Control
{
	private Control _home;
	private SettingsScreen _settings;
	private Tween _fade;

	public override void _Ready()
	{
		_home = GetNode<Control>("%Home");
		_settings = GetNode<SettingsScreen>("%Settings");

		GetNode<Button>("%PlayButton").Pressed += OnPlayPressed;
		GetNode<Button>("%SettingsButton").Pressed += OnSettingsPressed;
		_settings.BackRequested += OnBackPressed;
	}

	public override void _Notification(int what)
	{
		// Android back button/gesture: leave Settings, or quit from the home screen.
		if (what != NotificationWMGoBackRequest) return;
		if (_settings.Visible)
			ShowScreen(_home);
		else
			GetTree().Quit();
	}

	private void OnPlayPressed()
	{
		GetTree().ChangeSceneToFile(Scenes.Greenhouse);
	}

	private void OnSettingsPressed()
	{
		ShowScreen(_settings);
	}

	private void OnBackPressed()
	{
		ShowScreen(_home);
	}

	private void ShowScreen(Control screen)
	{
		var other = screen == _home ? _settings : _home;
		other.Hide();

		screen.Modulate = Colors.Transparent;
		screen.Show();
		_fade?.Kill();
		_fade = CreateTween();
		_fade.TweenProperty(screen, TweenPaths.Modulate, Colors.White, 0.2f)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
	}
}
