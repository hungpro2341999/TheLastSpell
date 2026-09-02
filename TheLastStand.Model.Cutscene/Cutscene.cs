using TPLib.Log;
using TheLastStand.Controller.Cutscene;
using TheLastStand.Definition.Cutscene;
using UnityEngine;

namespace TheLastStand.Model.Cutscene;

public class Cutscene
{
	private CutsceneDefinition sequenceDefinition;

	public CutsceneController[] SequenceControllers { get; private set; }

	public Cutscene(CutsceneDefinition cutsceneDefinition)
	{
		sequenceDefinition = cutsceneDefinition;
		Init();
	}

	private void Init()
	{
		SequenceControllers = new CutsceneController[sequenceDefinition.CutsceneElements.Count];
		for (int i = 0; i < sequenceDefinition.CutsceneElements.Count; i++)
		{
			ICutsceneDefinition cutsceneDefinition = sequenceDefinition.CutsceneElements[i];
			CutsceneController cutsceneController = ((cutsceneDefinition is BarkCutsceneDefinition) ? new BarkCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is ChangeMusicCutsceneDefinition) ? new ChangeMusicCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is ExileAllEnemiesCutsceneDefinition) ? new ExileAllEnemiesCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition) ? new EvolutiveLevelArtSetActiveCurrentStageCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is EvolutiveLevelArtSetStageCutsceneDefinition) ? new EvolutiveLevelArtSetStageCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is FadeInCutsceneDefinition) ? new FadeInCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is FadeOutCutsceneDefinition) ? new FadeOutCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is FocusMagicCircleCutsceneDefinition) ? new FocusMagicCircleCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is FocusTileCutsceneDefinition) ? new FocusTileCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is FocusUnitCutsceneDefinition) ? new FocusUnitCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is IncreaseFogCutsceneDefinition) ? new IncreaseFogCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is InitUnitVisualsCutsceneDefinition) ? new InitUnitVisualsCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is InstantiateParticlesCutsceneDefinition) ? new InstantiateParticlesCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is InvertImageCutsceneDefinition) ? new InvertImageCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is LookAtCircleCutsceneDefinition) ? new LookAtCircleCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is OverrideCurrentSpawnWaveCutsceneDefinition) ? new OverrideCurrentSpawnWaveCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayAnimatedCutsceneDefinition) ? new PlayAnimatedCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayCamShakeEffectCutsceneDefinition) ? new PlayCamShakeEffectCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayDeathAnimCutsceneDefinition) ? new PlayDeathAnimCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayEnemyTurnCutsceneDefinition) ? new PlayEnemyTurnCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayFXCutsceneDefinition) ? new PlayFXCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayMageDeathCutsceneDefinition) ? new PlayMageDeathCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayMageDeathStepCutsceneDefinition) ? new PlayMageDeathStepCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayPillarsCutsceneDefinition) ? new PlayPillarsCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlayRippleEffectCutsceneDefinition) ? new PlayRippleEffectCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlaySealAnticipationCutsceneDefinition) ? new PlaySealAnticipationCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlaySealDestructionCutsceneDefinition) ? new PlaySealDestructionCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is PlaySoundCutsceneDefinition) ? new PlaySoundCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is StopMagicCircleIdleCutsceneDefinition) ? new StopMagicCircleIdleCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is StopMusicCutsceneDefinition) ? new StopMusicCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is ToggleHUDCutsceneDefinition) ? new ToggleHUDCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is WaitCutsceneDefinition) ? new WaitCutsceneController(cutsceneDefinition) : ((cutsceneDefinition is ZoomCutsceneDefinition) ? ((CutsceneController)new ZoomCutsceneController(cutsceneDefinition)) : ((CutsceneController)((!(cutsceneDefinition is PlayCommanderFadeOutAnimCutsceneDefinition)) ? null : new PlayCommanderFadeOutAnimCutsceneController(cutsceneDefinition))))))))))))))))))))))))))))))))))));
			CutsceneController[] sequenceControllers = SequenceControllers;
			int num = i;
			sequenceControllers[num] = cutsceneController;
			if (SequenceControllers[i] == null)
			{
				CLoggerManager.Log(cutsceneDefinition.GetType().Name + " is not an implemented cutscene element to build a controller.", LogType.Error);
			}
		}
	}
}
