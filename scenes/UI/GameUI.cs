using Game.Resources.Building;
using Godot;
namespace Game.UI;

public partial class GameUI : MarginContainer
{
	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);
	
	private HBoxContainer hBoxContainer;
		
	[Export]
	private BuildingResource[] buildingResources;

	public override void _Ready()
	{
		hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");
		CreateBuildingButtons();	

	}

	private void CreateBuildingButtons()
	{
		foreach (var buildingResource in buildingResources)
		{
			var buildingbutton = new Button();
			buildingbutton.Text = $"Place {buildingResource.DisplayName}";
			hBoxContainer.AddChild(buildingbutton);
			buildingbutton.Pressed += () => 
			{
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}

}
