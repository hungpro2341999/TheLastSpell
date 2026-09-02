using TheLastStand.Manager.Building;
using TheLastStand.Model;

namespace TheLastStand.Controller.Building;

public class BuildingInterpreterContext : FormulaInterpreterContext
{
	private int MageCount => BuildingManager.MagicCircle.MageCount;
}
