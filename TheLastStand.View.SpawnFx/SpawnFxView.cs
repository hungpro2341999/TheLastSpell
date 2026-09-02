using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.CastFx;
using TheLastStand.Definition.SpawnFx;
using TheLastStand.Framework;
using TheLastStand.Framework.Animation;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.SpawnFx;
using TheLastStand.View.Camera;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.SpawnFx;

public class SpawnFxView : MonoBehaviour
{
	public static class Constants
	{
		public static class Fx
		{
			public const string AnimPlayerPrefabPath = "Prefab/Spawns FXs/Spawn FX";

			public const string AnimRootFolderPath = "Animation/Spawns FXs/";

			public const int SortingOrderBefore = 100;

			public const int SortingOrderBehind = 11;

			public const int SortingOrderAboveAll = 150;
		}

		public static class Sound
		{
			public const string SkillSFXSpatializedObjectPoolName = "Spawn SFX Spatialized";

			public const string SFXAudioClipPathPrefix = "Sounds/SFX/Spawns SFXs/";

			public const string SFXPrefabPath = "Prefab/Spawns SFXs/Spawn SFX";
		}
	}

	private static GameObject animPlayerPrefab;

	private static GameObject AnimPlayerPrefab
	{
		get
		{
			if (animPlayerPrefab == null)
			{
				animPlayerPrefab = ResourcePooler.LoadOnce<GameObject>("Prefab/Spawns FXs/Spawn FX");
			}
			return animPlayerPrefab;
		}
	}

	public static void PlaySpawnFxs(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		PlaySpawnVisualEffects(spawnFx);
		PlaySpawnCamShakes(spawnFx);
		PlaySpawnSoundEffects(spawnFx);
	}

	private static int ComputeSortingOrder(TheLastStand.Model.SpawnFx.SpawnFx spawnFx, SpawnVisualEffectDefinition spawnVisualEffectDefinition, Vector2Int position)
	{
		switch (spawnVisualEffectDefinition.SortingDepth)
		{
		default:
			return 100;
		case VisualEffectDefinition.E_Depth.Behind:
			return 11;
		case VisualEffectDefinition.E_Depth.Dynamic:
			if (position.y < spawnFx.SourceTile.Y || position.x < spawnFx.SourceTile.X)
			{
				return 100;
			}
			return 11;
		case VisualEffectDefinition.E_Depth.AboveAll:
			return 150;
		}
	}

	private static AudioClip GetSoundEffectAudioClip(SoundEffectDefinition soundEffectDefinition)
	{
		if (!string.IsNullOrEmpty(soundEffectDefinition.FolderPath))
		{
			AudioClip[] list = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.FolderPath);
			return RandomManager.GetRandomElement(TPSingleton<SkillManager>.Instance, list);
		}
		if (!string.IsNullOrEmpty(soundEffectDefinition.Path))
		{
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.Path);
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
					return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(j).Key);
				}
			}
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(count - 1).Key);
		}
		CLoggerManager.Log("A sound effect should have a Path or some random paths !!");
		return null;
	}

	private static void PlaySpawnVisualEffects(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		int i = 0;
		for (int count = spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition.Count; i < count; i++)
		{
			AnimationClip clip = ResourcePooler.LoadOnce<AnimationClip>("Animation/Spawns FXs/" + spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition[i].Path);
			SpawnVisualEffectDefinition spawnVisualEffectDefinition = spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition[i];
			if (spawnVisualEffectDefinition != null)
			{
				SingleAnimPlayer component = Object.Instantiate(AnimPlayerPrefab, GameManager.ViewTransform).GetComponent<SingleAnimPlayer>();
				component.DestroyGoOnFinish = true;
				component.Clip = clip;
				Vector2Int vector2Int = TileMapController.GetRotatedTilemapPosition(pivotTilemapPosition: spawnFx.SourceTile.Position, offsetFromPivot: Vector2Int.zero, angle: 0f);
				component.transform.position = TileMapView.GetWorldPosition(vector2Int);
				component.GetComponentInChildren<SpriteRenderer>().sortingOrder = ComputeSortingOrder(spawnFx, spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition[i], vector2Int);
				component.gameObject.name = $"SpawnFX_{vector2Int.x}-{vector2Int.y}";
				float delay = spawnVisualEffectDefinition.Delay.EvalToFloat();
				component.Play(delay);
			}
		}
	}

	private static void PlaySpawnCamShakes(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		foreach (CastFxDefinition.CamShakeDefinition camShakeDefinition in spawnFx.SpawnFxDefinition.CamShakeDefinitions)
		{
			ACameraView.Shake(camShakeDefinition.Id, camShakeDefinition.Delay.EvalToFloat());
		}
	}

	private static void PlaySpawnSoundEffects(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		foreach (SoundEffectDefinition soundEffectDefinition in spawnFx.SpawnFxDefinition.SoundEffectDefinitions)
		{
			OneShotSound component = ObjectPooler.GetPooledGameObject("Spawn SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Spawns SFXs/Spawn SFX")).GetComponent<OneShotSound>();
			if (GetSoundEffectAudioClip(soundEffectDefinition) == null)
			{
				TPSingleton<SoundManager>.Instance.LogError("Failed at loading AudioClip on path Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.Path);
			}
			else
			{
				component.PlaySpatialized(GetSoundEffectAudioClip(soundEffectDefinition), spawnFx.SourceTile, soundEffectDefinition.Delay.EvalToFloat());
			}
		}
	}
}
