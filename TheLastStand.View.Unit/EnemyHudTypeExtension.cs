using System;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Unit.UI;

namespace TheLastStand.View.Unit;

public static class EnemyHudTypeExtension
{
	public static string GetPoolId(this EnemyUnitView.E_EnemyHudType enemyHudType)
	{
		return enemyHudType switch
		{
			EnemyUnitView.E_EnemyHudType.Small => "EnemySmallHUDs", 
			EnemyUnitView.E_EnemyHudType.Large => "EnemyLargeHUDs", 
			EnemyUnitView.E_EnemyHudType.BossSmall => "BossSmallHUDs", 
			EnemyUnitView.E_EnemyHudType.BossLarge => "BossLargeHUDs", 
			_ => throw new ArgumentOutOfRangeException("enemyHudType", enemyHudType, null), 
		};
	}

	public static UnitHUD GetPrefab(this EnemyUnitView.E_EnemyHudType enemyHudType)
	{
		return enemyHudType switch
		{
			EnemyUnitView.E_EnemyHudType.Small => EnemyUnitManager.EnemyUnitHUDSmall, 
			EnemyUnitView.E_EnemyHudType.Large => EnemyUnitManager.EnemyUnitHUDLarge, 
			EnemyUnitView.E_EnemyHudType.BossSmall => BossManager.BossUnitHUDSmall, 
			EnemyUnitView.E_EnemyHudType.BossLarge => BossManager.BossUnitHUDLarge, 
			_ => throw new ArgumentOutOfRangeException("enemyHudType", enemyHudType, null), 
		};
	}
}
