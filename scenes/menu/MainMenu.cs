using Godot;

namespace Petal;

/// <summary>
/// Title screen (GAMEPLAY.md §2) with Play and Settings. The Settings screen lives in
/// this scene; tapping Settings swaps it in with a short fade, Back swaps home back in.
/// Settings has one option, Language, changed with the arrows either side of its name.
/// </summary>
public partial class MainMenu : Control
{
	private Control _home;
	private Control _settings;
	private Label _languageName;
	private Tween _fade;

	public override void _Ready()
	{
		_home = GetNode<Control>("%Home");
		_settings = GetNode<Control>("%Settings");

		GetNode<Button>("%PlayButton").Pressed += OnPlayPressed;
		GetNode<Button>("%SettingsButton").Pressed += OnSettingsPressed;
		_settings.GetNode<Button>("%BackButton").Pressed += OnBackPressed;

		_languageName = _settings.GetNode<Label>("%LanguageName");
		_languageName.Text = Language.CurrentName;
		_settings.GetNode<Button>("%PreviousLanguage").Pressed += OnPreviousLanguagePressed;
		_settings.GetNode<Button>("%NextLanguage").Pressed += OnNextLanguagePressed;
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

	private void OnPreviousLanguagePressed()
	{
		ChangeLanguage(-1);
	}

	private void OnNextLanguagePressed()
	{
		ChangeLanguage(+1);
	}

	private void ChangeLanguage(int direction)
	{
		Language.Step(direction);
		_languageName.Text = Language.CurrentName;
	}

	private void ShowScreen(Control screen)
	{
		var other = screen == _home ? _settings : _home;
		other.Hide();

		screen.Modulate = Colors.Transparent;
		screen.Show();
		_fade?.Kill();
		_fade = CreateTween();
		_fade.TweenProperty(screen, "modulate", Colors.White, 0.2f)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
	}
}
