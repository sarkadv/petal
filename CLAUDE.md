# Petal

A cosy 2D florist game for **Android and iOS**: grow flowers in a greenhouse, make bouquets for townsfolk,
spend earnings on upgrades. No timers that punish, no game over.

- Engine: **Godot 4.7 (.NET / C#)**, **Mobile** renderer.
- Portrait only: base viewport 720×1280, stretch `canvas_items` / aspect `expand`, so screens can be taller or wider than the base.
- Touch-first. Mouse should also work, which is how it's tested on desktop (450×800 window).

## Design doc

`docs/GAMEPLAY.md` is the source of truth for gameplay, UI flow and art direction. Read the relevant section
before building a feature, and refer to it from code comments as `GAMEPLAY.md §N`. Screens with their own detailed
spec have a doc next to it (`docs/GREENHOUSE.md`); keep it in step when the screen changes. If an implementation needs to
differ from the doc, say so rather than silently diverging.

## Layout

- `core/`: game-wide code that isn't tied to one scene, one subfolder per concern:
  - `core/app/`: running the game (`Startup` autoload, `Language`, `Scenes` paths).
  - `core/catalogue/`: what exists in the game, loaded from `data/game_data.tres` (`GameData`, `FlowerData`,
	`FlowerKind`, `FlowerColour`, `FertilizerKind`, `FertilizerData`).
  - `core/game/`: the game being played and its rules (`GameState`, `Plot`, `Seed`, `Stem`).
  - `core/ui/`: helpers shared by screens (`ButtonExtensions`, `ControlExtensions` for the bounce, `TweenPaths` for
	tweened properties, `Overlay` for windows and popups that Back closes).
- `scenes/<area>/`: a scene's `.tscn`, its script and its assets together (e.g. `scenes/menu/`). A screen's sub-scenes
  go in subfolders (`scenes/greenhouse/popovers/`, `scenes/greenhouse/seed_stall/`), its art in `art/`.
- `data/`: balancing data as Godot resources (`game_data.tres`: flower catalogue, prices, starting values).
- `ui/`: the global theme (`petal_theme.tres`) and shared icons.
- `localization/`: `translations.csv` plus Godot's generated `.translation` files.
- `tests/`: GdUnit4 tests, one suite per screen (`MainMenuTest`, `SettingsTest`, `GreenhouseTest`) plus game rules
  (`PlotRulesTest`).
- `docs/`: design docs. `docs/reference/` holds agreed visual mockups (e.g. `greenhouse_concept.svg`); it has a
  `.gdignore` so Godot doesn't import them.

## Building and checking

- `dotnet build` checks the C# compiles.
- Development happens on both Windows and macOS. Godot isn't on PATH; the `GODOT_BIN` environment variable points to
  the Godot 4.7.2 mono build on each machine:
  - Windows: `~/Desktop/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe`
  - macOS: `/Applications/Godot_mono.app/Contents/MacOS/Godot`
- Run the game headless for a few frames to catch load errors (missing resources, broken scenes, script exceptions):
  `"$GODOT_BIN" --headless --path . --quit-after 120`. Add `--scene res://path/to/scene.tscn` to start a
  specific scene.
- Run the tests (see [Tests](#tests)) after changing behaviour.
- Headless runs can't show how things look or feel. Say what still needs checking by eye in the editor.
- Godot generates `.uid` and `.import` files next to scripts and assets. Commit them along with the files they
  belong to. `.godot/` is the editor's local cache and is ignored.

## Tests

GdUnit4 (`gdUnit4.api`, `gdUnit4.test.adapter`) run through `dotnet test`. Add or update tests when behaviour changes.

### Running

- `dotnet test --settings .runsettings` builds the project and runs every suite in `tests/`. Each run starts Godot
  headless from `GODOT_BIN`; the test host needs the .NET 8 runtime.
- One suite or test: add `--filter "FullyQualifiedName~GreenhouseTest"` (or `~GreenhouseTest.ExactlyOneToolIsAlwaysSelected`).
- If `GODOT_BIN` is empty, every test is reported as "not executed" rather than failing. Check it's set first.
- `.runsettings` runs suites one at a time (`MaxCpuCount` 1) and treats "no tests found" as an error.
- The adapter generates `gdunit4_testadapter_v5/` and `TestResults/` on each run; both are git-ignored, except
  `gdunit4_testadapter_v5/.gdignore`. Keep that file: if the editor sees the generated runner script, every C# rebuild
  fails with ".NET: Failed to unload assemblies".

### Suites

- `MainMenuTest`, `SettingsTest`, `GreenhouseTest`: one per screen, driven through the UI.
- `PlotRulesTest`: game rules (GAMEPLAY.md §4) on a `GameState.NewGame()`, with no scene loaded.
- `Touch.cs`: tap, hold and drag helpers, not a suite.

### Writing a suite

- Namespace `Petal.Tests`, `using static GdUnit4.Assertions;`, class marked `[TestSuite]`, tests `[TestCase]`, set-up
  and tear-down `[BeforeTest]` / `[AfterTest]`. Name tests as a sentence about behaviour
  (`UnwateredSeedlingsDieOvernight`), and give the suite a `/// <summary>` naming the doc section it checks.
- Mark suites that use Godot (scenes, nodes, `GameData`, `TranslationServer`) `[RequireGodotRuntime]`.
- Start from a fresh game: `GameState.StartNewGame()` for screen tests (screens read `GameState.Current`),
  `GameState.NewGame()` for rules tests that hold their own state.
- Set `TranslationServer.SetLocale("en")` before loading a scene so text assertions use the English strings.
- Load screens with `ISceneRunner.Load(Scenes.X, true)` and `Dispose()` the runner in `[AfterTest]`.
- `await runner.AwaitIdleFrame()` after loading a scene so containers have laid out their children.
- Drive screens like a player with `runner.Tap(control)` / `runner.Hold(control)` (`tests/Touch.cs`), which push real
  mouse input at the control's centre, rather than emitting signals or calling handlers directly.
- Find nodes by unique name, as the game does (`GetNode<Button>("%PlayButton")`).
- Tests must not leave anything behind in `user://`. Back up any file a test writes (e.g. `settings.cfg` in
  `SettingsTest`) and restore or delete it in `[AfterTest]`.
- Test packages are only referenced outside export builds, and `tests/` is left out of exported games
  (`Petal.csproj`).

## Code conventions

- C# only, no GDScript. Namespace `Petal` with file-scoped `namespace Petal;`. Tabs for indentation.
- Node scripts are `public partial class X : <NodeType>`.
- Look nodes up by unique name (`GetNode<Button>("%PlayButton")`) and connect signals in code (`Pressed += ...`),
  not in the scene file.
- Each class gets a short `/// <summary>` saying what it is and which GAMEPLAY.md section it implements.
- Things that must happen once at game start go in the `Startup` autoload (`core/app/Startup.cs`), not in a scene's `_Ready`,
  so they also run when a scene is launched directly.
- Hand-editing `.tscn` / `.tres` is fine; keep Godot's format, and expect the editor to add `unique_id`s and uids on save.
- Do not create unneccessary comments. Most code should be self-commenting, if not possible, add a short explanatory comment.

## UI

- All styling goes through `ui/petal_theme.tres` and its type variations (`TitleLabel`, `HeaderLabel`,
  `SecondaryButton`). Avoid per-node theme overrides when a theme type would do.
- Fonts live in `ui/fonts/` with their OFL licences: Lora for everything (semibold for buttons, headings and the
  title), Caveat for handwriting (the day chalkboard). The `OFL.txt` files must
  ship with the game (export filter `*.txt` or a credits screen).
- Tap targets are big (main buttons are 88 px tall at the 720-wide base). Keep that for anything tappable.
- Leave room for notches and rounded corners at the top and bottom of the screen.
- Android's back button/gesture arrives as `NotificationWMGoBackRequest` (`quit_on_go_back` is off), so every
  screen decides what back does.

## Localization

- Languages: English (`en`) and Czech (`cs`). Every player-visible string needs both.
- Strings live in `localization/translations.csv`; the key is the English text (`Play,Play,Hrát`). Controls
  translate their text automatically, so put the English key in the scene.
- Language names are shown in their own language and are never translated (`Language.All`).
- The `.translation` files are regenerated by the editor when the CSV changes.

## Settings and saves

- Player settings live in `user://settings.cfg` (Godot `ConfigFile`). Load before saving so other values are kept,
  and never save over a file that failed to load: that would wipe it.
- Saves follow GAMEPLAY.md §2: three slots, autosave at the start of each day.

## Mobile and performance

- Target low-end phones. Keep per-frame work and shaders cheap; prefer static textures over full-screen shaders.
- Code must work on both Android and iOS. Avoid platform-specific APIs unless both platforms are handled.
- iOS exports need macOS with Xcode. The Android build targets `net9.0` (see `Petal.csproj`).

## Art

- 2D, hand-painted, Ghibli-*inspired* (GAMEPLAY.md §1.1): original art only, never copied characters or scenes.
- Placeholders are fine and should be simple (e.g. the SVG menu background) and marked as placeholders.
- Do not create unnecessary shaders, all graphics are placeholders. Use SVG if possible.
