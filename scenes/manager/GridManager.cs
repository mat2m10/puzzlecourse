using System.Collections.Generic;
using Godot;
namespace Game.Manager;

public partial class GridManager : Node
{
	private HashSet<Vector2> occupiedCells = new();

	[Export]
	private TileMapLayer HighlightTileMapLayer;
	[Export]
	private TileMapLayer BaseTerrainTileMapLayer;

	public override void _Ready()
	{
	}
	public bool IsTilePositionValid(Vector2 tileposition)
	{
		return !occupiedCells.Contains(tileposition);
	}

	public void MarkTileAsOccupied(Vector2 tileposition)
	{
		occupiedCells.Add(tileposition);
	}
	public void HighlightValidTilesInRadius(Vector2 rootcell, int radius) 
	{
		ClearHiglightedTiles();

		for (var x = rootcell.X - radius; x <= rootcell.X + radius; x++)
		{
			for (var y = rootcell.Y - radius; y <= rootcell.Y + radius; y++)
			{
				if (!IsTilePositionValid(new Vector2(x, y)))
				{
					continue;
				}
				HighlightTileMapLayer.SetCell(new Vector2I((int)x,(int)y), 0, Vector2I.Zero);
			}
		}

	}
	public void ClearHiglightedTiles()
	{
		HighlightTileMapLayer.Clear();
	}

	public Vector2 GetMouseGridCellPosition()
	{
		var mousePosition = HighlightTileMapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		gridPosition = gridPosition.Floor();
		return gridPosition;
	}
}
