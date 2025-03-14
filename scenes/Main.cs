using Game.Manager;
using Game.Resources.Building;
using Godot;
namespace Game;
public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;

	private BuildingResource towerResource;
	private BuildingResource villageResource;

	private Button placeTowerButton;
	private Button placeVillageButton;

	private Node2D ySortRoot;
	private Vector2I? hoverGridCell;
	private BuildingResource toPlaceBuildingResource;

	private TileMapLayer HighlightTileMapLayer;
	
	public override void _Ready()
	{
		towerResource = GD.Load<BuildingResource>("res://resources/building/tower.tres");
		villageResource = GD.Load<BuildingResource>("res://resources/building/village.tres");

		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		placeTowerButton = GetNode<Button>("PlaceTowerButton");
		placeVillageButton = GetNode<Button>("PlaceVillageButton");
		ySortRoot = GetNode<Node2D>("YSortRoot");
		cursor.Visible = false;

		placeTowerButton.Pressed += OnPlacedTowerButtonPressed;
		placeVillageButton.Pressed += OnPlacedVillageButtonPressed;
		gridManager.ResourceTilesUpated += OnResourceTilesUpated;

	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (hoverGridCell.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePositionBuildable(hoverGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			cursor.Visible = false;
		}
	}
	public override void _Process(double delta)
	{

		var gridPosition = gridManager.GetMouseGridCellPosition();
		cursor.GlobalPosition = gridPosition * 64;
		if (toPlaceBuildingResource!= null && cursor.Visible && (!hoverGridCell.HasValue || hoverGridCell.Value != gridPosition))
		{
			hoverGridCell = gridPosition;
			gridManager.ClearHiglightedTiles();
			gridManager.HighlightExpandableBuildableTiles(hoverGridCell.Value, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoverGridCell.Value, toPlaceBuildingResource.ResourceRadius);

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
	}

	private void OnPlacedTowerButtonPressed()
	{
		toPlaceBuildingResource = towerResource;
		cursor.Visible = true;
		gridManager.HighlightbuildableTiles();
	}

	private void OnPlacedVillageButtonPressed()
	{
		toPlaceBuildingResource = villageResource;
		cursor.Visible = true;
		gridManager.HighlightbuildableTiles();
	}

	private void OnResourceTilesUpated(int resourceCount)
	{
		GD.Print("Resource count: " + resourceCount);
	}
}
