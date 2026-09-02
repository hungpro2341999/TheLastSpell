using Sirenix.OdinInspector;
using TheLastStand.Framework;
using TheLastStand.Manager.Sound;
using TheLastStand.View.Sound;

namespace TheLastStand.Manager;

public abstract class ABuildingBehaviorManager<T> : BehaviorManager<T> where T : SerializedMonoBehaviour
{
	private static class Constants
	{
		public static class Sound
		{
			public const string BuildingSkillAudioSourcePoolId = "BuildingSkillSFX";

			public const string BuildingSkillSpatializedAudioSourcePoolId = "BuildingSkillSFX Spatialized";

			public const string BuildingSkillSoundClipPathFormat = "Sounds/SFX/Buildings/Skills/{0}/SFX_{0}_{1}";

			public const string BuildingSkillSoundLaunchClipPathFormat = "Sounds/SFX/Buildings/Skills/{0}/SFX_{0}_{1}_Launch";

			public const string BuildingSkillSoundImpactClipPathFormat = "Sounds/SFX/Buildings/Skills/{0}/SFX_{0}_{1}_Impact";

			public const string BuildingSkillSoundAssetPrefix = "Sounds/SFX/Buildings/Skills/";
		}
	}

	protected override OneShotSound GetPooledSkillSoundAudioSource()
	{
		return ObjectPooler.GetPooledComponent("BuildingSkillSFX", SoundManager.BuildingSkillSFXPrefab);
	}

	protected override OneShotSound GetSpatializedPooledSkillSoundAudioSource()
	{
		return ObjectPooler.GetPooledComponent("BuildingSkillSFX Spatialized", SoundManager.BuildingSkillSpatializedSFXPrefab);
	}

	protected override string GetSkillSoundClipPathFormat()
	{
		return "Sounds/SFX/Buildings/Skills/{0}/SFX_{0}_{1}";
	}

	protected override string GetSkillSoundLaunchPathFormat()
	{
		return "Sounds/SFX/Buildings/Skills/{0}/SFX_{0}_{1}_Launch";
	}

	protected override string GetSkillSoundImpactPathFormat()
	{
		return "Sounds/SFX/Buildings/Skills/{0}/SFX_{0}_{1}_Impact";
	}
}
