using System;
using Godot;

namespace Petal;

/// <summary>
/// The Settings screen (GAMEPLAY.md §2), shown in the main menu. Its one option, Language, is changed with the arrows
/// either side of its name. Back raises <see cref="BackRequested"/>; the menu decides where it goes.
/// </summary>
public partial class SettingsScreen : Control
{
	public event Action BackRequested;

	private Label _languageName;

	public override void _Ready()
	{
		_languageName = GetNode<Label>("%LanguageName");
		_languageName.Text = Language.CurrentName;
		GetNode<Button>("%PreviousLanguage").Pressed += () => ChangeLanguage(-1);
		GetNode<Button>("%NextLanguage").Pressed += () => ChangeLanguage(+1);
		GetNode<Button>("%BackButton").Pressed += () => BackRequested?.Invoke();
	}

	private void ChangeLanguage(int direction)
	{
		Language.Step(direction);
		_languageName.Text = Language.CurrentName;
	}
}
