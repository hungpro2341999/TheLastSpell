using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Skill.SkillAction;

public class EnemySpawnData
{
	public Node Amount { get; }

	public UnitCreationSettings UnitCreationSettings { get; }

	public string Id { get; }

	public TileFlagDefinition.E_TileFlagTag TileFlag { get; }

	public int Weight { get; }

	public EnemySpawnData(string id, int weight, Node amount, TileFlagDefinition.E_TileFlagTag tileFlag = TileFlagDefinition.E_TileFlagTag.None, UnitCreationSettings unitCreationSettings = null)
	{
		Id = id;
		Weight = weight;
		Amount = amount;
		TileFlag = tileFlag;
		UnitCreationSettings = unitCreationSettings;
	}
}
