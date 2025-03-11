using Godot;
using System;
namespace Game;
public partial class Main : Node2D
{
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private Vector2? hoverGridCell;
	private TileMapLayer HighlightTileMapLayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
		cursor = GetNode<Sprite2D>("Cursor");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
		placeBuildingButton.Pressed += OnButtonPressed;
		HighlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");
		cursor.Visible = false;

	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (cursor.Visible && evt.IsActionPressed("left_click"))
		{
			PlaceBuildingAtMousePosition();
			cursor.Visible = false;
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		var gridPosition = GetMouseGridCellPosition();
		cursor.GlobalPosition = gridPosition * 64;
		if (cursor.Visible && (!hoverGridCell.HasValue || hoverGridCell.Value != gridPosition))
		{
			hoverGridCell = gridPosition;
			UpdateHighlightTileMapLayer();
		}
	}

	private Vector2 GetMouseGridCellPosition()
	{
		var mousePosition = GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		gridPosition = gridPosition.Floor();
		return gridPosition;
	}

	private void PlaceBuildingAtMousePosition()
	{
		var building = buildingScene.Instantiate<Node2D>();
		AddChild(building);
		var gridPosition = GetMouseGridCellPosition();
		building.GlobalPosition = gridPosition * 64;
		hoverGridCell = null;
		UpdateHighlightTileMapLayer();
	}

	private void UpdateHighlightTileMapLayer()
	{
		HighlightTileMapLayer.Clear();
		if (!hoverGridCell.HasValue)
		{
			return;
		}
		for (var x = hoverGridCell.Value.X - 3; x <= hoverGridCell.Value.X + 3; x++)
		{
			for (var y = hoverGridCell.Value.Y - 3; y <= hoverGridCell.Value.Y + 3; y++)
			{
				HighlightTileMapLayer.SetCell(new Vector2I((int)x,(int)y), 0, Vector2I.Zero);
			}
		}
	}

	private void OnButtonPressed()
	{
		cursor.Visible = true;
	}
}
