using System.Threading.Tasks;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;
using static Petal.Language;

namespace Petal.Tests;

/// <summary>
/// The Settings screen's language picker (GAMEPLAY.md §2). The player's real settings file is backed up before
/// each test and put back afterwards.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class SettingsTest
{
	private byte[] _savedSettings;
	private ISceneRunner _runner;
	private Control _settings;
	private Label _languageName;

	[BeforeTest]
	public async Task Setup()
	{
		_savedSettings = FileAccess.FileExists(SettingsPath) ? FileAccess.GetFileAsBytes(SettingsPath) : null;
		var cfg = new ConfigFile();
		cfg.SetValue("general", "language", "en");
		cfg.Save(SettingsPath);
		Language.LoadSaved();

		_runner = ISceneRunner.Load(Scenes.MainMenu, true);
		var menu = _runner.Scene();
		_settings = menu.GetNode<Control>("%Settings");
		_languageName = _settings.GetNode<Label>("%LanguageName");
		await _runner.Tap(menu.GetNode<Button>("%SettingsButton"));
	}

	[AfterTest]
	public void TearDown()
	{
		_runner.Dispose();
		if (_savedSettings != null)
		{
			using var file = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Write);
			file.StoreBuffer(_savedSettings);
		}
		else
		{
			DirAccess.RemoveAbsolute(SettingsPath);
		}
		Language.LoadSaved();
	}

	[TestCase]
	public void ShowsTheSavedLanguage()
	{
		AssertThat(_languageName.Text).IsEqual("English");
	}

	[TestCase]
	public async Task NextArrowSwitchesToCzechAndSavesIt()
	{
		await _runner.Tap(_settings.GetNode<Button>("%NextLanguage"));

		AssertThat(_languageName.Text).IsEqual("Čeština");
		AssertThat(TranslationServer.GetLocale()).IsEqual("cs");
		AssertThat(TranslationServer.Translate("Play").ToString()).IsEqual("Hrát");

		var cfg = new ConfigFile();
		AssertThat((int)cfg.Load(SettingsPath)).IsEqual((int)Error.Ok);
		AssertThat((string)cfg.GetValue("general", "language")).IsEqual("cs");
	}

	[TestCase]
	public async Task ArrowsWrapAround()
	{
		await _runner.Tap(_settings.GetNode<Button>("%PreviousLanguage"));
		AssertThat(_languageName.Text).IsEqual("Čeština");

		await _runner.Tap(_settings.GetNode<Button>("%NextLanguage"));
		AssertThat(_languageName.Text).IsEqual("English");
	}
}
