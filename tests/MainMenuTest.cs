using System.Threading.Tasks;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

namespace Petal.Tests;

/// <summary>The main menu's home screen and moving to and from Settings (GAMEPLAY.md §2).</summary>
[TestSuite]
[RequireGodotRuntime]
public class MainMenuTest
{
	private ISceneRunner _runner;
	private Node _menu;
	private Control _home;
	private Control _settings;

	[BeforeTest]
	public void Setup()
	{
		TranslationServer.SetLocale("en");
		_runner = ISceneRunner.Load(Scenes.MainMenu, true);
		_menu = _runner.Scene();
		_home = _menu.GetNode<Control>("%Home");
		_settings = _menu.GetNode<Control>("%Settings");
	}

	[AfterTest]
	public void TearDown() => _runner.Dispose();

	[TestCase]
	public void OpensOnHome()
	{
		AssertThat(_home.Visible).IsTrue();
		AssertThat(_settings.Visible).IsFalse();
		AssertThat(_menu.GetNode<Button>("%PlayButton").Text).IsEqual("Play");
		AssertThat(_menu.GetNode<Button>("%SettingsButton").Text).IsEqual("Settings");
	}

	[TestCase]
	public async Task SettingsButtonOpensSettings()
	{
		await _runner.Tap(_menu.GetNode<Button>("%SettingsButton"));

		AssertThat(_settings.Visible).IsTrue();
		AssertThat(_home.Visible).IsFalse();
	}

	[TestCase]
	public async Task BackButtonReturnsHome()
	{
		await _runner.Tap(_menu.GetNode<Button>("%SettingsButton"));
		await _runner.Tap(_settings.GetNode<Button>("%BackButton"));

		AssertThat(_home.Visible).IsTrue();
		AssertThat(_settings.Visible).IsFalse();
	}

	[TestCase]
	public async Task AndroidBackLeavesSettings()
	{
		await _runner.Tap(_menu.GetNode<Button>("%SettingsButton"));
		_menu.Notification((int)Node.NotificationWMGoBackRequest);

		AssertThat(_home.Visible).IsTrue();
		AssertThat(_settings.Visible).IsFalse();
	}
}
