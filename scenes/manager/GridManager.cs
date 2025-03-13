using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Components;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	private HashSet<Vector2I> validBuildableTiles = new();


	[Export]
	private TileMapLayer HighlightTileMapLayer;
	[Export]
	private TileMapLayer BaseTerrainTileMapLayer;

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
	}

	public bool IsTilePositionValid(Vector2I tileposition)
	{
		var customData = BaseTerrainTileMapLayer.GetCellTileData(tileposition);
		if (customData == null) return false;
		return (bool)customData.GetCustomData("buildable");
	
	}

	public bool IsTilePositionBuildable(Vector2I tileposition)
	{
		return validBuildableTiles.Contains(tileposition);
	}

	public void HighlightbuildableTiles()
	{
		foreach (var tileposition in validBuildableTiles)
		{
			HighlightTileMapLayer.SetCell(tileposition, 0, Vector2I.Zero);
		}
	}


	public void ClearHiglightedTiles()
	{
		HighlightTileMapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPosition()
	{
		var mousePosition = HighlightTileMapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootcell = buildingComponent.GetGridCellPosition();
		for (var x = rootcell.X - buildingComponent.BuildableRadius; x <= rootcell.X + buildingComponent.BuildableRadius; x++)
		{
			for (var y = rootcell.Y - buildingComponent.BuildableRadius; y <= rootcell.Y + buildingComponent.BuildableRadius; y++)
			{
				var tileposition = new Vector2I(x,y);
				if (!IsTilePositionValid(tileposition)) continue;
				validBuildableTiles.Add(tileposition);
			}
		}

		validBuildableTiles.Remove(buildingComponent.GetGridCellPosition());
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
	}
}
