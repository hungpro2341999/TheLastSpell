using TheLastStand.Model.Building;

namespace TheLastStand.Model.Unit;

public class UnitCreationSettings
{
	public readonly string BossPhaseActorId;

	public readonly bool CastSpawnSkill;

	public readonly bool IgnoreFromEnemyUnitsCount;

	public readonly bool PlaySpawnAnim;

	public readonly bool PlaySpawnCutscene;

	public readonly bool WaitSpawnAnim;

	public int OverrideVariantId;

	public readonly TheLastStand.Model.Building.Building LinkedBuilding;

	public readonly bool IsGuardian;

	public UnitCreationSettings(string bossPhaseActorId = null, bool castSpawnSkill = true, bool playSpawnAnim = true, bool playSpawnCutscene = true, bool waitSpawnAnim = false, int overrideVariantId = -1, TheLastStand.Model.Building.Building linkedBuilding = null, bool isGuardian = false, bool ignoreFromEnemyUnitsCount = false)
	{
		BossPhaseActorId = bossPhaseActorId;
		CastSpawnSkill = castSpawnSkill;
		IgnoreFromEnemyUnitsCount = ignoreFromEnemyUnitsCount || isGuardian;
		PlaySpawnAnim = playSpawnAnim;
		PlaySpawnCutscene = playSpawnCutscene;
		WaitSpawnAnim = waitSpawnAnim;
		OverrideVariantId = overrideVariantId;
		LinkedBuilding = linkedBuilding;
		IsGuardian = isGuardian;
	}
}
