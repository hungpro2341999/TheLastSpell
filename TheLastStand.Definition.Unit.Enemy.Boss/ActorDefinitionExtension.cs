using TheLastStand.Database.Building;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public static class ActorDefinitionExtension
{
	public static TheLastStand.Framework.Serialization.Definition GetCorrespondingDefinition(this ActorDefinition actorDefinition)
	{
		return actorDefinition.ActorType switch
		{
			DamageableType.Enemy => EnemyUnitDatabase.EnemyUnitTemplateDefinitions.GetValueOrDefault(actorDefinition.UnitId), 
			DamageableType.Boss => BossUnitDatabase.BossUnitTemplateDefinitions.GetValueOrDefault(actorDefinition.UnitId), 
			DamageableType.Building => BuildingDatabase.BuildingDefinitions.GetValueOrDefault(actorDefinition.UnitId), 
			_ => null, 
		};
	}
}
