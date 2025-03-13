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

	private List<TileMapLayer> allTimemapLayers = new();
	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		allTimemapLayers = GetAllTilemapLayers(BaseTerrainTileMapLayer);
		foreach (var layer in allTimemapLayers)
		{
			GD.Print(layer.Name);
		}
	}

	public bool IsTilePositionValid(Vector2I tileposition)
	{
		foreach (var layer in allTimemapLayers)
		{
			var customData = layer.GetCellTileData(tileposition);
			if (customData == null) continue;
			return (bool)customData.GetCustomData("buildable");

		}
		return false;
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

	public void HighlightExpandableBuildableTiles(Vector2I rootCell, int radius)
	{
		ClearHiglightedTiles();
		HighlightbuildableTiles();

		var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tileposition in expandedTiles)
		{
			HighlightTileMapLayer.SetCell(tileposition, 0, atlasCoords);
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

	private List<TileMapLayer> GetAllTilemapLayers(TileMapLayer rootTilemapLayer)
	{
		var result = new List<TileMapLayer>();
		var children = rootTilemapLayer.GetChildren();
		children.Reverse();
		foreach (var child in children)
		{
			if (child is TileMapLayer childLayer)
			{
				result.AddRange(GetAllTilemapLayers(childLayer));
			}
		}
		result.Add(rootTilemapLayer);
		return result;

	}
	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildableRadius);
		validBuildableTiles.UnionWith(validTiles);
		validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		var result = new List<Vector2I>();
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tileposition = new Vector2I(x,y);
				if (!IsTilePositionValid(tileposition)) continue;
				result.Add(tileposition);
			}
		}
		return result;

	}
	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		var occupiedTiles = buildingComponents.Select(x => x.GetGridCellPosition());
		return occupiedTiles;
	}


	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
	}
}
