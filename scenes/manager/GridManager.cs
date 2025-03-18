using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Components;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	private const string IS_BUILDABLE = "is_buildable";
	private const string IS_WOOD = "is_wood";
	private const string IS_IGNORED = "is_ignored";

	[Signal]
	public delegate void ResourceTilesUpatedEventHandler(int collectedTiles);

	[Signal]
	public delegate void GridStateUpdatedEventHandler();
	private HashSet<Vector2I> validBuildableTiles = new();
	private HashSet<Vector2I> collectedResourceTiles = new();
	private HashSet<Vector2I> occupiedTiles = new();


	[Export]
	private TileMapLayer HighlightTileMapLayer;
	[Export]
	private TileMapLayer BaseTerrainTileMapLayer;

	private List<TileMapLayer> allTimemapLayers = new();
	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;

		allTimemapLayers = GetAllTilemapLayers(BaseTerrainTileMapLayer);
		foreach (var layer in allTimemapLayers)
		{
			// GD.Print(layer.Name);
		}
	}

	public bool TileHasCustomData(Vector2I tileposition, string dataName)
	{
		foreach (var layer in allTimemapLayers)
		{
			var customData = layer.GetCellTileData(tileposition);
			if (customData == null || (bool)customData.GetCustomData(IS_IGNORED)) continue;
			return (bool)customData.GetCustomData(dataName);

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
		var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(occupiedTiles);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tileposition in expandedTiles)
		{
			HighlightTileMapLayer.SetCell(tileposition, 0, atlasCoords);
		}
	}

	public void HighlightResourceTiles(Vector2I rootCell, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(rootCell, radius);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tileposition in resourceTiles)
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
		return ConvertWorldPositiontoGridCellPosition(mousePosition);

	}

	public Vector2I ConvertWorldPositiontoGridCellPosition(Vector2 worldPosition)
	{
		var tilePosition = worldPosition / 64;
		tilePosition = tilePosition.Floor();
		return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
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
		occupiedTiles.Add(buildingComponent.GetGridCellPosition());
		var rootCell = buildingComponent.GetGridCellPosition();
		var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildingResource.BuildableRadius);
		validBuildableTiles.UnionWith(validTiles);
		validBuildableTiles.ExceptWith(occupiedTiles);
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var resourceTiles = GetResourceTilesInRadius(rootCell, buildingComponent.BuildingResource.ResourceRadius);
		var old_resource_tiles = collectedResourceTiles.Count;
		collectedResourceTiles.UnionWith(resourceTiles);
		if (old_resource_tiles != collectedResourceTiles.Count) {
			EmitSignal(SignalName.ResourceTilesUpated, collectedResourceTiles.Count);
		}
		EmitSignal(SignalName.GridStateUpdated);
	}

	private void RecalculateGrid(BuildingComponent excludeBuildingComponent)
	{
		occupiedTiles.Clear();
		validBuildableTiles.Clear();
		collectedResourceTiles.Clear();
		
		var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
		.Where((buildingComponent) => buildingComponent != excludeBuildingComponent);
		foreach (var building in buildingComponent)
		{
			UpdateValidBuildableTiles(building);
			UpdateCollectedResourceTiles(building);
		}
		EmitSignal(SignalName.ResourceTilesUpated, collectedResourceTiles.Count);
		EmitSignal(SignalName.GridStateUpdated);
	}
	
	private List<Vector2I> GetTilesInRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tileposition = new Vector2I(x,y);
				if (!filterFn(tileposition)) continue;
				result.Add(tileposition);
			}
		}
		return result;
	}
	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell, radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_BUILDABLE);
		});

	}

	private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell, radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_WOOD);
		});

	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
	}

	private void OnBuildingDestroyed(BuildingComponent buildingComponent)
	{
		RecalculateGrid(buildingComponent);
	}
}
