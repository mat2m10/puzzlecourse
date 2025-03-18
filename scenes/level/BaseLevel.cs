using Game.Building;
using Game.Manager;
using Godot;

namespace Game;
public partial class BaseLevel : Node
{
	private GridManager gridManager;
	private GoldMine goldMine;
	private GameCamera gameCamera;
	private Node2D baseBuilding;
	private TileMapLayer baseTerrainTileMapLayer;
	public override void _Ready()
	{
		gridManager = GetNode<GridManager>("GridManager");
		goldMine = GetNode<GoldMine>("%GoldMine");
		gameCamera = GetNode<GameCamera>("GameCamera");
		baseBuilding = GetNode<Node2D>("%Base");
		baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");

		gameCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
		gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

		gridManager.GridStateUpdated += OnGridStateUpdated;

	}
	private void OnGridStateUpdated()
	{

		var goldMinePosition = gridManager.ConvertWorldPositiontoGridCellPosition(goldMine.GlobalPosition);
		if (gridManager.IsTilePositionBuildable(goldMinePosition)){
			goldMine.SetActive();
			GD.Print("Win");
		
		}
	}
}
