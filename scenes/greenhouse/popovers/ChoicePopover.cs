using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Petal;

/// <summary>
/// The choices for the Seeds or Fertilize tool (GAMEPLAY.md §4.2), shown below the held tool. Tapping a choice
/// picks it.
/// </summary>
public partial class ChoicePopover : Popover
{
	public readonly record struct Choice(Texture2D Icon, Color IconTint, string Text, bool Selected, Action Picked);

	private const float MaxWidth = 640;

	private HFlowContainer _content;

	public override void _Ready()
	{
		base._Ready();
		_content = GetNode<HFlowContainer>("%Content");
	}

	/// <param name="emptyMessage">Translation key, shown when there is nothing to choose.</param>
	public void ShowChoices(Control anchor, IReadOnlyList<Choice> choices, string emptyMessage)
	{
		foreach (var child in _content.GetChildren())
		{
			_content.RemoveChild(child);
			child.QueueFree();
		}
		foreach (var choice in choices)
			_content.AddChild(CreateButton(choice));
		if (choices.Count == 0)
			_content.AddChild(CreateLabel(Tr(emptyMessage)));

		// One row of content, wrapping past MaxWidth.
		var margins = Panel.GetThemeStylebox("panel").GetMinimumSize();
		float separation = _content.GetThemeConstant("h_separation");
		float row = _content.GetChildren().Cast<Control>().Sum(c => c.GetCombinedMinimumSize().X + separation) - separation;
		ShowBy(anchor, Math.Min(MaxWidth, row + margins.X));
	}

	protected override Vector2 PositionBy(Rect2 anchor, Vector2 size) =>
		new(anchor.GetCenter().X - size.X / 2, anchor.End.Y + Gap);

	private Button CreateButton(Choice choice)
	{
		var button = new Button
		{
			Text = choice.Text,
			Icon = choice.Icon,
			ToggleMode = true,
			ButtonPressed = choice.Selected,
			ThemeTypeVariation = "ChoiceButton",
			FocusMode = FocusModeEnum.None,
			AutoTranslateMode = AutoTranslateModeEnum.Disabled,
		};
		button.SetIconTint(choice.IconTint);
		button.Pressed += () =>
		{
			Hide();
			choice.Picked();
		};
		return button;
	}

	private static Label CreateLabel(string text) => new()
	{
		Text = text,
		ThemeTypeVariation = "PopoverLabel",
		HorizontalAlignment = HorizontalAlignment.Center,
		AutoTranslateMode = AutoTranslateModeEnum.Disabled,
	};
}
