using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Model;

public static class SKillCasterExtensions
{
	public static DamageableType GetDamageableType(this ISkillCaster skillCaster)
	{
		if (!(skillCaster is BattleModule))
		{
			if (!(skillCaster is BossUnit))
			{
				if (!(skillCaster is EnemyUnit))
				{
					if (skillCaster is PlayableUnit)
					{
						return DamageableType.Playable;
					}
					return DamageableType.Other;
				}
				return DamageableType.Enemy;
			}
			return DamageableType.Boss;
		}
		return DamageableType.Building;
	}
}
