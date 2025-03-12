using Game.Manager;
using Godot;
namespace Game;
public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private Vector2? hoverGridCell;

	private TileMapLayer HighlightTileMapLayer;
	
	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
		placeBuildingButton.Pressed += OnButtonPressed;
		cursor.Visible = false;

	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (hoverGridCell.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePositionValid(hoverGridCell.Value))
		{
			PlaceBuildingAtHoveredCellPosition();
			cursor.Visible = false;
		}
	}
	public override void _Process(double delta)
	{

		var gridPosition = gridManager.GetMouseGridCellPosition();
		cursor.GlobalPosition = gridPosition * 64;
		if (cursor.Visible && (!hoverGridCell.HasValue || hoverGridCell.Value != gridPosition))
		{
			hoverGridCell = gridPosition;
			gridManager.HighlightValidTilesInRadius(hoverGridCell.Value, 3);
		}
	}



	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!hoverGridCell.HasValue)
		{
			return;
		}
		var building = buildingScene.Instantiate<Node2D>();
		AddChild(building);
		
		building.GlobalPosition = hoverGridCell.Value * 64;
		gridManager.MarkTileAsOccupied(hoverGridCell.Value);	
		hoverGridCell = null;
		gridManager.ClearHiglightedTiles();
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
	}
}
