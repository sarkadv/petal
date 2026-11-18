using Godot;

namespace Petal;

/// <summary>
/// The game's languages. The chosen one is remembered in user://settings.cfg;
/// on first launch the phone's language is used if the game supports it.
/// </summary>
public static class Language
{
	// Names are written in their own language, so they're never translated.
	public static readonly (string Code, string Name)[] All =
	{
		("en", "English"),
		("cs", "Čeština"),
	};

	public const string SettingsPath = "user://settings.cfg";

	// Starts out matching what TranslationServer shows (the phone's language), so the two
	// agree even if something reads the language before LoadSaved runs.
	private static int _current = IndexOf(TranslationServer.GetLocale().Split('_')[0]);

	public static string CurrentName => All[_current].Name;

	/// <summary>Applies the saved language (or the phone's language on first launch).</summary>
	public static void LoadSaved()
	{
		var cfg = new ConfigFile();
		string code = cfg.Load(SettingsPath) == Error.Ok
			? (string)cfg.GetValue("general", "language", "")
			: "";
		if (code == "")
			code = OS.GetLocaleLanguage();
		Apply(IndexOf(code));
	}

	/// <summary>Moves to the next (+1) or previous (-1) language, wrapping around, and saves it.</summary>
	public static void Step(int direction)
	{
		Apply((_current + direction + All.Length) % All.Length);

		// Load first to keep anything else already in the file. If it exists but can't be
		// read, don't save: writing now would replace it with just the language.
		var cfg = new ConfigFile();
		var loaded = cfg.Load(SettingsPath);
		if (loaded != Error.Ok && loaded != Error.FileNotFound)
		{
			GD.PushError($"Couldn't read {SettingsPath} ({loaded}); language change not saved.");
			return;
		}

		cfg.SetValue("general", "language", All[_current].Code);
		var saved = cfg.Save(SettingsPath);
		if (saved != Error.Ok)
			GD.PushError($"Couldn't save {SettingsPath} ({saved}); language change not saved.");
	}

	private static void Apply(int index)
	{
		_current = index;
		TranslationServer.SetLocale(All[_current].Code);
	}

	// Unsupported language → English.
	private static int IndexOf(string code) =>
		System.Math.Max(0, System.Array.FindIndex(All, l => l.Code == code));
}
