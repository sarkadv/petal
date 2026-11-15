using Godot;

namespace Petal;

/// <summary>
/// Autoload that runs once when the game starts, before the first scene, whichever scene
/// that is. Applies the player's saved settings.
/// </summary>
public partial class Startup : Node
{
	public override void _EnterTree()
	{
		Language.LoadSaved();
	}
}
