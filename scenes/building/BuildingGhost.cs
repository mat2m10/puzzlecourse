using Godot;
namespace Game.Building;
public partial class BuildingGhost : Node2D
{
	public void SetInvalid()
	{
		Modulate = new Color(1, 0, 0, 0.5f);
	}

	public void SetValid()
	{
		Modulate = new Color(1, 1, 1, 0.9f);
	}

}
