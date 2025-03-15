using System;
using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;
namespace Game.Manager;

public partial class BuildingManager : Node
{
	[Export]
	private GridManager gridManager;
	[Export]
	private GameUI gameUI;
	[Export]
	private Node2D ySortRoot;
	[Export]
	private PackedScene buildingGhostScene;
	
	private int currentResourceCount;
	private int startingResourceCount = 4;
	private int currentlyUsedResourceCount;
	private BuildingResource toPlaceBuildingResource;
	private Vector2I? hoverGridCell;
	private BuildingGhost buildingGhost;

	private int AvailableResourceCount => currentResourceCount + startingResourceCount - currentlyUsedResourceCount;
	public override void _Ready()
	{
		gridManager.ResourceTilesUpated += OnResourceTilesUpated;
		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;

	}

		public override void _UnhandledInput(InputEvent evt)
	{
		if (
			hoverGridCell.HasValue &&
			toPlaceBuildingResource != null &&
			evt.IsActionPressed("left_click") &&
			IsBuildingPlacableAtTile(hoverGridCell.Value)
			)
		{
			PlaceBuildingAtHoveredCellPosition();
		}
	}
	public override void _Process(double delta)
	{
		if (!IsInstanceValid(buildingGhost)) return;

		var gridPosition = gridManager.GetMouseGridCellPosition();
		buildingGhost.GlobalPosition = gridPosition * 64;
		if (toPlaceBuildingResource!= null && (!hoverGridCell.HasValue || hoverGridCell.Value != gridPosition))
		{
			hoverGridCell = gridPosition;
			UpdateGridDisplay();
		}
	}

	private void UpdateGridDisplay()
	{
		if (hoverGridCell == null)
		{
			return;
		}
		gridManager.ClearHiglightedTiles();
		gridManager.HighlightbuildableTiles();

		if(IsBuildingPlacableAtTile(hoverGridCell.Value))
		{
			gridManager.HighlightExpandableBuildableTiles(hoverGridCell.Value, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoverGridCell.Value, toPlaceBuildingResource.ResourceRadius);
			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInvalid();
		}

	}
    private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!hoverGridCell.HasValue)
		{
			return;
		}
		var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		ySortRoot.AddChild(building);
		
		building.GlobalPosition = hoverGridCell.Value * 64;
		hoverGridCell = null;
		gridManager.ClearHiglightedTiles();
		currentlyUsedResourceCount += toPlaceBuildingResource.ResourceCost;
		buildingGhost.QueueFree();
		buildingGhost = null;
	}

	private bool IsBuildingPlacableAtTile(Vector2I tileposition)
	{
		return 	gridManager.IsTilePositionBuildable(tileposition) &&
			AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
		
	}

	private void OnResourceTilesUpated(int resourceCount)
	{
		currentResourceCount = resourceCount;
	}

	
	private void OnBuildingResourceSelected(BuildingResource buildingResource)
	{
		if (IsInstanceValid(buildingGhost))
		{
			buildingGhost.QueueFree();
		}
		buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
		ySortRoot.AddChild(buildingGhost);

		var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddChild(buildingSprite);

		toPlaceBuildingResource = buildingResource;
		UpdateGridDisplay();
	}
}
