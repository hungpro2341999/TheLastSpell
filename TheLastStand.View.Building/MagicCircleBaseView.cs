using TPLib;
using TheLastStand.Framework;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Building;

public class MagicCircleBaseView : MonoBehaviour
{
	public static class Constants
	{
		public const string AnimationBaseMagicCirclePath = "Animation/MagicCircle/";

		public const string AnimationNamePrefix = "MagicCircle_";

		public const string IdleBaseSuffix = "IdleBase";
	}

	[SerializeField]
	private Animator animator;

	public void DisableAnimator()
	{
		animator.enabled = false;
	}

	public void InitAnimations()
	{
		AnimationClip animationClip = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_1", failSilently: true);
		AnimationClip animationClip2 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_2", failSilently: true);
		AnimationClip animationClip3 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_3", failSilently: true);
		AnimationClip animationClip4 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_4", failSilently: true);
		if (!(animationClip == null) || !(animationClip2 == null) || !(animationClip3 == null) || !(animationClip4 == null))
		{
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
			animatorOverrideController["MagicCircle_IdleBase_1"] = animationClip;
			animatorOverrideController["MagicCircle_IdleBase_2"] = animationClip2;
			animatorOverrideController["MagicCircle_IdleBase_3"] = animationClip3;
			animatorOverrideController["MagicCircle_IdleBase_4"] = animationClip4;
			animator.runtimeAnimatorController = animatorOverrideController;
		}
	}

	public void RefreshAnimationBaseWithMagesQuantity(int magesQuantity)
	{
		animator.SetTrigger("Slots" + magesQuantity);
	}
}
