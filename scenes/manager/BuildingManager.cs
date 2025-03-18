using System;
using System.Linq;
using Game.Building;
using Game.Components;
using Game.Resources.Building;
using Game.UI;
using Godot;
namespace Game.Manager;

public partial class BuildingManager : Node
{
	private readonly StringName ACTION_LEFT_CLICK = "left_click";
	private readonly StringName ACTION_RIGHT_CLICK = "right_click";
	private readonly StringName ACTION_CANCEL = "cancel";
	[Export]
	private int startingResourceCount = 4;
	[Export]
	private GridManager gridManager;
	[Export]
	private GameUI gameUI;
	[Export]
	private Node2D ySortRoot;
	[Export]
	private PackedScene buildingGhostScene;

	private enum State {
		Normal,
		PlacingBuilding
	}
	
	private int currentResourceCount;
	private int currentlyUsedResourceCount;
	private BuildingResource toPlaceBuildingResource;
	private Vector2I hoverGridCell;
	private BuildingGhost buildingGhost;
	private State currentState;

	private int AvailableResourceCount => currentResourceCount + startingResourceCount - currentlyUsedResourceCount;
	public override void _Ready()
	{
		gridManager.ResourceTilesUpated += OnResourceTilesUpated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;

	}

		public override void _UnhandledInput(InputEvent evt)
	{
		switch (currentState)
		{
			case State.Normal:
			if (evt.IsActionPressed(ACTION_RIGHT_CLICK))
			{
				DestroyBuildingatHoveredCellPosition();
			}
				break;
			case State.PlacingBuilding:
				if (evt.IsActionPressed(ACTION_CANCEL)){
					ChangeState(State.Normal);
				}
				else if (
					toPlaceBuildingResource != null &&
					evt.IsActionPressed(ACTION_LEFT_CLICK) &&
					IsBuildingPlacableAtTile(hoverGridCell)
				){
					PlaceBuildingAtHoveredCellPosition();
				}
				
				break;
			default:
				break;
		}
		
	}
	public override void _Process(double delta)
	{
		var gridPosition = gridManager.GetMouseGridCellPosition();
		
		if (hoverGridCell != gridPosition)
		{
			hoverGridCell = gridPosition;
			UpdateHoveredGridCell();
		}

		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost.GlobalPosition = gridPosition * 64;
				break;
		}
	}

	private void UpdateGridDisplay()
	{

		gridManager.ClearHiglightedTiles();
		gridManager.HighlightbuildableTiles();

		if(IsBuildingPlacableAtTile(hoverGridCell))
		{
			gridManager.HighlightExpandableBuildableTiles(hoverGridCell, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoverGridCell, toPlaceBuildingResource.ResourceRadius);
			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInvalid();
		}

	}
    private void PlaceBuildingAtHoveredCellPosition()
	{

		var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		ySortRoot.AddChild(building);
		
		building.GlobalPosition = hoverGridCell * 64;

		currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
		ChangeState(State.Normal);
	}

	private void DestroyBuildingatHoveredCellPosition()
	{
		var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
		.FirstOrDefault((buildingComponent) => buildingComponent.GetGridCellPosition() == hoverGridCell);
		if (buildingComponent == null) return;
		
		currentlyUsedResourceCount -= buildingComponent.BuildingResource.ResourceCost;
		buildingComponent.Destroy();

	}

	private void ClearBuildingGhost()
	{
		gridManager.ClearHiglightedTiles();
		if (IsInstanceValid(buildingGhost)) {
			buildingGhost.QueueFree();
		}
		buildingGhost = null;
	}

	private bool IsBuildingPlacableAtTile(Vector2I tileposition)
	{
		return 	gridManager.IsTilePositionBuildable(tileposition) &&
			AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
		
	}

	private void UpdateHoveredGridCell()
	{
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				UpdateGridDisplay();
				break;
		}
	}

	private void ChangeState(State toState){
		switch(currentState)
		// Cleanup
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				ClearBuildingGhost();
				toPlaceBuildingResource = null;
				break;
		}
		currentState = toState;
		switch(currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
				ySortRoot.AddChild(buildingGhost);
				break;
		}
	}
	private void OnResourceTilesUpated(int resourceCount)
	{
		currentResourceCount = resourceCount;
	}

	
	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{

		ChangeState(State.PlacingBuilding);
		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddChild(buildingSprite);

		toPlaceBuildingResource = buildingResource;
		UpdateGridDisplay();
	}
}
