using TPLib;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;

namespace TheLastStand.Controller.Tutorial;

public class DuringNightTutorialConditionController : TutorialConditionController
{
	protected DuringNightTutorialConditionDefinition DuringNightConditionDefinition => base.ConditionDefinition as DuringNightTutorialConditionDefinition;

	public DuringNightTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && (!DuringNightConditionDefinition.BossNightOnly || SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsBossWave);
		if (!base.ConditionDefinition.Invert)
		{
			return flag;
		}
		return !flag;
	}
}
