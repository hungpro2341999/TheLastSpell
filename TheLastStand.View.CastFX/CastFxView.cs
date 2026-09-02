using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework;
using TheLastStand.Framework.Animation;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.CastFx;
using TheLastStand.View.Camera;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.CastFX;

public class CastFxView : MonoBehaviour
{
	public static class Constants
	{
		public static class Fx
		{
			public const string AnimPlayerPrefabPath = "Prefab/Skills FXs/Skill FX";

			public const string AnimRootFolderPath = "Animation/Skills FXs/";

			public const int SortingOrderBefore = 100;

			public const int SortingOrderBehind = 11;

			public const int SortingOrderAboveAll = 150;
		}

		public static class Sound
		{
			public const string SkillSFXSpatializedObjectPoolName = "Skill SFX Spatialized";

			public const string SkillSFXObjectPoolName = "Skill SFX";

			public const string BuildingsSkillSFXSpatializedObjectPoolName = "BuildingSkillSFX Spatialized";

			public const string BuildingsSkillSFXObjectPoolName = "BuildingSkillSFX";

			public const string SFXAudioClipPathPrefix = "Sounds/SFX/Skills SFXs/";

			public const string SFXPrefabPath = "Prefab/Skills SFXs/Skill SFX";

			public const string SFXSpatializedPrefabPath = "Prefab/Skills SFXs/Skill SFX Spatialized";
		}
	}

	private static GameObject animPlayerPrefab;

	private static GameObject AnimPlayerPrefab
	{
		get
		{
			if (animPlayerPrefab == null)
			{
				animPlayerPrefab = ResourcePooler.LoadOnce<GameObject>("Prefab/Skills FXs/Skill FX");
			}
			return animPlayerPrefab;
		}
	}

	public static void PlayCastFxs(CastFx castFx, TileObjectSelectionManager.E_Orientation specificOrientation = TileObjectSelectionManager.E_Orientation.NONE, Vector2 offset = default(Vector2), ITileObject source = null, bool isMirroredOnYAxis = false)
	{
		PlayCastVisualEffects(castFx, specificOrientation, offset, isMirroredOnYAxis);
		PlayCastCamShakes(castFx);
		PlayCastSoundEffects(castFx, source);
	}

	private static int ComputeSortingOrder(CastFx castFx, VisualEffectDefinition visualEffectDefinition, Vector2Int targetPosition)
	{
		return visualEffectDefinition.SortingDepth switch
		{
			VisualEffectDefinition.E_Depth.Before => 100, 
			VisualEffectDefinition.E_Depth.Behind => 11, 
			VisualEffectDefinition.E_Depth.Dynamic => (targetPosition.y < castFx.SourceTile.Y || targetPosition.x < castFx.SourceTile.X) ? 100 : 11, 
			VisualEffectDefinition.E_Depth.TargetDynamic => (targetPosition.y < castFx.TargetTile.Y || targetPosition.x < castFx.TargetTile.X) ? 100 : 11, 
			VisualEffectDefinition.E_Depth.AboveAll => 150, 
			_ => 100, 
		};
	}

	private static void PlayCastCamShakes(CastFx castFx)
	{
		foreach (CastFxDefinition.CamShakeDefinition camShakeDefinition in castFx.CastFxDefinition.CamShakeDefinitions)
		{
			ACameraView.Shake(camShakeDefinition.Id, camShakeDefinition.Delay.EvalToFloat(castFx.CastFXInterpreterContext));
		}
	}

	private static void PlayCastVisualEffects(CastFx castFx, TileObjectSelectionManager.E_Orientation specificOrientation = TileObjectSelectionManager.E_Orientation.NONE, Vector2 casterOffset = default(Vector2), bool isMirroredOnYAxis = false)
	{
		int i = 0;
		for (int count = castFx.CastFxDefinition.VisualEffectDefinitions.Count; i < count; i++)
		{
			if (castFx.AffectedTiles[i] == null || castFx.AffectedTiles[i].Count == 0)
			{
				continue;
			}
			GameDefinition.E_Direction e_Direction = ((specificOrientation != TileObjectSelectionManager.E_Orientation.NONE) ? TileObjectSelectionManager.GetDirectionFromOrientation(specificOrientation) : TileMapController.GetDirectionBetweenTiles(castFx.SourceTile, castFx.TargetTile));
			if (e_Direction == GameDefinition.E_Direction.None)
			{
				e_Direction = castFx.TargetTile.Unit?.LookDirection ?? GameDefinition.E_Direction.North;
			}
			AnimationClip animationClip = ResourcePooler.LoadOnce<AnimationClip>("Animation/Skills FXs/" + castFx.CastFxDefinition.VisualEffectDefinitions[i].GetPath(e_Direction, isMirroredOnYAxis));
			if (animationClip == null)
			{
				if (!castFx.CastFxDefinition.VisualEffectDefinitions[i].CanSkipIfMissingOrientation)
				{
					string arg = "Animation/Skills FXs/" + castFx.CastFxDefinition.VisualEffectDefinitions[i].GetAnyValidPath();
					CLoggerManager.Log($"Failed to launch a CastFx in direction: {e_Direction}, isMirroredOnYAxis: {isMirroredOnYAxis}, first valid path: {arg} !", LogType.Error, CLogLevel.MAJOR);
				}
			}
			else
			{
				if (!(castFx.CastFxDefinition.VisualEffectDefinitions[i] is StandardVisualEffectDefinition standardVisualEffectDefinition))
				{
					continue;
				}
				Vector3 vector = Vector3.zero;
				if (standardVisualEffectDefinition.Target.TargetType == StandardVisualEffectDefinition.TargetData.E_TargetType.Caster)
				{
					vector = casterOffset;
				}
				int j = 0;
				for (int count2 = castFx.AffectedTiles[i].Count; j < count2; j++)
				{
					SingleAnimPlayer component = Object.Instantiate(AnimPlayerPrefab, GameManager.ViewTransform).GetComponent<SingleAnimPlayer>();
					component.DestroyGoOnFinish = true;
					component.Clip = animationClip;
					Vector2Int vector2Int = TileMapController.GetRotatedTilemapPosition(pivotTilemapPosition: castFx.AffectedTiles[i][j].Position, angle: TileObjectSelectionManager.GetAngleFromOrientation(specificOrientation), offsetFromPivot: standardVisualEffectDefinition.Target.TileOffset);
					component.transform.position = TileMapView.GetWorldPosition(vector2Int) + vector;
					component.GetComponentInChildren<SpriteRenderer>().sortingOrder = ComputeSortingOrder(castFx, standardVisualEffectDefinition, vector2Int);
					component.gameObject.name = $"CastFX_{vector2Int.x}-{vector2Int.y}";
					if (standardVisualEffectDefinition.SpawnedParticlesPath != string.Empty)
					{
						ObjectPooler.GetPooledGameObject(standardVisualEffectDefinition.SpawnedParticlesPath, ResourcePooler.LoadOnce<GameObject>(standardVisualEffectDefinition.SpawnedParticlesPath)).transform.position = TileMapView.GetWorldPosition(vector2Int) + vector;
					}
					float num = castFx.CastFxDefinition.VisualEffectDefinitions[i].Delay.EvalToFloat(castFx.CastFXInterpreterContext);
					if (standardVisualEffectDefinition.Target.TargetType == StandardVisualEffectDefinition.TargetData.E_TargetType.PropagationTiles && castFx.CastFxDefinition is SkillCastFxDefinition skillCastFxDefinition)
					{
						num += skillCastFxDefinition.PropagationDelay * (float)j;
					}
					component.Play(num);
				}
			}
		}
	}

	private static void PlayCastSoundEffects(CastFx castFx, ITileObject source = null)
	{
		bool buildingCaster = source is TheLastStand.Model.Building.Building || source is BattleModule;
		string prefabPath;
		string poolName;
		foreach (SoundEffectDefinition item in castFx.CastFxDefinition.SoundEffectDefinitionsOnCast)
		{
			GetPoolNameAndPrefabPath(item, buildingCaster, out prefabPath, out poolName);
			OneShotSound component = ObjectPooler.GetPooledGameObject(poolName, ResourcePooler.LoadOnce<GameObject>(prefabPath)).GetComponent<OneShotSound>();
			component.name = (source?.Id ?? "UnknownSource") + "_Launch";
			if (item.IsSpatialized)
			{
				component.PlaySpatialized(GetSoundEffectAudioClip(item), castFx.SourceTile, item.Delay.EvalToFloat());
			}
			else
			{
				component.Play(GetSoundEffectAudioClip(item), item.Delay.EvalToFloat());
			}
		}
		foreach (SoundEffectDefinition item2 in castFx.CastFxDefinition.SoundEffectDefinitionsOnImpact)
		{
			GetPoolNameAndPrefabPath(item2, buildingCaster, out prefabPath, out poolName);
			OneShotSound component2 = ObjectPooler.GetPooledGameObject(poolName, ResourcePooler.LoadOnce<GameObject>(prefabPath)).GetComponent<OneShotSound>();
			component2.name = (source?.Id ?? "UnknownSource") + "_Impact";
			if (item2.IsSpatialized)
			{
				component2.PlaySpatialized(GetSoundEffectAudioClip(item2), castFx.SourceTile, item2.Delay.EvalToFloat());
			}
			else
			{
				component2.Play(GetSoundEffectAudioClip(item2), item2.Delay.EvalToFloat());
			}
		}
	}

	private static void GetPoolNameAndPrefabPath(SoundEffectDefinition soundEffectDefinition, bool buildingCaster, out string prefabPath, out string poolName)
	{
		prefabPath = (soundEffectDefinition.IsSpatialized ? "Prefab/Skills SFXs/Skill SFX Spatialized" : "Prefab/Skills SFXs/Skill SFX");
		if (buildingCaster)
		{
			poolName = (soundEffectDefinition.IsSpatialized ? "BuildingSkillSFX Spatialized" : "BuildingSkillSFX");
		}
		else
		{
			poolName = (soundEffectDefinition.IsSpatialized ? "Skill SFX Spatialized" : "Skill SFX");
		}
	}

	private static AudioClip GetSoundEffectAudioClip(SoundEffectDefinition soundEffectDefinition)
	{
		if (!string.IsNullOrEmpty(soundEffectDefinition.FolderPath))
		{
			AudioClip[] list = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.FolderPath);
			return RandomManager.GetRandomElement(TPSingleton<SkillManager>.Instance, list);
		}
		if (!string.IsNullOrEmpty(soundEffectDefinition.Path))
		{
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.Path);
		}
		if (soundEffectDefinition.RandomPaths.Count > 0)
		{
			int count = soundEffectDefinition.RandomPaths.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				num += soundEffectDefinition.RandomPaths.ElementAt(i).Value;
			}
			int num2 = RandomManager.GetRandomRange(TPSingleton<SkillManager>.Instance, 0, num);
			for (int j = 0; j < count; j++)
			{
				num2 -= soundEffectDefinition.RandomPaths.ElementAt(j).Value;
				if (num2 < 0)
				{
					return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(j).Key);
				}
			}
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(count - 1).Key);
		}
		CLoggerManager.Log("A sound effect should have a Path or some random paths !!");
		return null;
	}
}
