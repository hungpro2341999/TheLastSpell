using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class CutsceneDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public List<ICutsceneDefinition> CutsceneElements { get; private set; }

	public bool ContainsInitVisuals => CutsceneElements.Any((ICutsceneDefinition x) => x is InitUnitVisualsCutsceneDefinition);

	public bool ShouldHideHUD { get; private set; } = true;

	public bool ShouldSetState { get; private set; } = true;

	public CutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		CutsceneDefinition cutsceneDefinition = null;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			cutsceneDefinition = GameDatabase.CutsceneDefinitions[xAttribute2.Value];
		}
		if (cutsceneDefinition != null)
		{
			CutsceneElements = cutsceneDefinition.CutsceneElements;
		}
		else
		{
			CutsceneElements = new List<ICutsceneDefinition>();
			foreach (XElement item2 in xElement.Elements())
			{
				switch (item2.Name.LocalName)
				{
				case "Bark":
					CutsceneElements.Add(new BarkCutsceneDefinition(item2));
					break;
				case "ChangeMusic":
					CutsceneElements.Add(new ChangeMusicCutsceneDefinition(item2));
					break;
				case "EvolutiveLevelArtSetActiveCurrentStage":
					CutsceneElements.Add(new EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition(item2));
					break;
				case "EvolutiveLevelArtSetStage":
					CutsceneElements.Add(new EvolutiveLevelArtSetStageCutsceneDefinition(item2));
					break;
				case "ExileAllEnemies":
					CutsceneElements.Add(new ExileAllEnemiesCutsceneDefinition(item2));
					break;
				case "FadeIn":
					CutsceneElements.Add(new FadeInCutsceneDefinition(item2));
					break;
				case "FadeOut":
					CutsceneElements.Add(new FadeOutCutsceneDefinition(item2));
					break;
				case "FocusMagicCircle":
					CutsceneElements.Add(new FocusMagicCircleCutsceneDefinition(item2));
					break;
				case "FocusTile":
					CutsceneElements.Add(new FocusTileCutsceneDefinition(item2));
					break;
				case "FocusUnit":
					CutsceneElements.Add(new FocusUnitCutsceneDefinition(item2));
					break;
				case "InitUnitVisuals":
					CutsceneElements.Add(new InitUnitVisualsCutsceneDefinition(item2));
					break;
				case "InstantiateParticles":
					CutsceneElements.Add(new InstantiateParticlesCutsceneDefinition(item2));
					break;
				case "InvertImage":
					CutsceneElements.Add(new InvertImageCutsceneDefinition(item2));
					break;
				case "LookAtCircle":
					CutsceneElements.Add(new LookAtCircleCutsceneDefinition(item2));
					break;
				case "OverrideCurrentSpawnWave":
					CutsceneElements.Add(new OverrideCurrentSpawnWaveCutsceneDefinition(item2));
					break;
				case "PlayAnimatedCutscene":
					CutsceneElements.Add(new PlayAnimatedCutsceneDefinition(item2));
					break;
				case "PlayCamShakeEffect":
					CutsceneElements.Add(new PlayCamShakeEffectCutsceneDefinition(item2));
					break;
				case "PlayDeathAnim":
					CutsceneElements.Add(new PlayDeathAnimCutsceneDefinition(item2));
					break;
				case "PlayEnemyTurn":
					CutsceneElements.Add(new PlayEnemyTurnCutsceneDefinition(item2));
					break;
				case "PlayMageDeath":
					CutsceneElements.Add(new PlayMageDeathCutsceneDefinition(item2));
					break;
				case "PlayMageDeathStep":
					CutsceneElements.Add(new PlayMageDeathStepCutsceneDefinition(item2));
					break;
				case "PlayPillarsCutscene":
				{
					PlayPillarsCutsceneDefinition item = new PlayPillarsCutsceneDefinition(item2);
					CutsceneElements.Add(item);
					break;
				}
				case "PlayRippleEffect":
					CutsceneElements.Add(new PlayRippleEffectCutsceneDefinition(item2));
					break;
				case "PlaySealAnticipation":
					CutsceneElements.Add(new PlaySealAnticipationCutsceneDefinition(item2));
					break;
				case "PlaySealDestruction":
					CutsceneElements.Add(new PlaySealDestructionCutsceneDefinition(item2));
					break;
				case "PlayFX":
					CutsceneElements.Add(new PlayFXCutsceneDefinition(item2));
					break;
				case "StopMagicCircleIdle":
					CutsceneElements.Add(new StopMagicCircleIdleCutsceneDefinition(item2));
					break;
				case "StopMusic":
					CutsceneElements.Add(new StopMusicCutsceneDefinition(item2));
					break;
				case "PlaySound":
					CutsceneElements.Add(new PlaySoundCutsceneDefinition(item2));
					break;
				case "Wait":
					CutsceneElements.Add(new WaitCutsceneDefinition(item2));
					break;
				case "IncreaseFog":
					CutsceneElements.Add(new IncreaseFogCutsceneDefinition(item2));
					break;
				case "ToggleHUD":
					CutsceneElements.Add(new ToggleHUDCutsceneDefinition(item2));
					break;
				case "Zoom":
					CutsceneElements.Add(new ZoomCutsceneDefinition(item2));
					break;
				case "CommanderPlayFadeOutAnim":
					CutsceneElements.Add(new PlayCommanderFadeOutAnimCutsceneDefinition(item2));
					break;
				default:
					CLoggerManager.Log(item2.Name.LocalName + " is not an implemented cutscene element.", LogType.Error);
					break;
				}
			}
		}
		XAttribute xAttribute3 = xElement.Attribute("ShouldHideHUD");
		if (xAttribute3 != null && xAttribute3.Value != null)
		{
			if (bool.TryParse(xAttribute3.Value, out var result))
			{
				ShouldHideHUD = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute3.Value + " into bool", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else if (cutsceneDefinition != null)
		{
			ShouldHideHUD = cutsceneDefinition.ShouldHideHUD;
		}
		XAttribute xAttribute4 = xElement.Attribute("ShouldSetState");
		if (xAttribute4 != null && xAttribute4.Value != null)
		{
			if (bool.TryParse(xAttribute4.Value, out var result2))
			{
				ShouldSetState = result2;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute4.Value + " into bool", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else if (cutsceneDefinition != null)
		{
			ShouldSetState = cutsceneDefinition.ShouldSetState;
		}
	}
}
