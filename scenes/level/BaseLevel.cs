using Game.Building;
using Game.Manager;
using Godot;

namespace Game;
public partial class BaseLevel : Node
{
	private GridManager gridManager;
	private GoldMine goldMine;	
	public override void _Ready()
	{
		gridManager = GetNode<GridManager>("GridManager");
		goldMine = GetNode<GoldMine>("%GoldMine");
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
