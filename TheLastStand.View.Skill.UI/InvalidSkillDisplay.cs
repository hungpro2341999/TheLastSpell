using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Manager.Skill;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class InvalidSkillDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/InvalidSkillDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI invalidityCauseText;

	private float initY;

	public void Init(SkillManager.E_InvalidSkillCause cause)
	{
		StopAllCoroutines();
		translateTarget.localPosition = new Vector3(translateTarget.localPosition.x, initY);
		invalidityCauseText.text = Localizer.Get("InvalidSkillCause_" + cause);
	}

	protected override IEnumerator DisplayCoroutine()
	{
		if (translateDuration < 0f)
		{
			translateDuration = displayDuration;
		}
		if (fadeInDuration < 0f)
		{
			fadeInDuration = displayDuration;
		}
		if (fadeInDuration > 0f && fadeTarget != null)
		{
			fadeTarget.alpha = 0f;
			fadeTarget.DOFade(1f, fadeInDuration);
		}
		if (translateDuration > 0f && translateTarget != null)
		{
			initY = translateTarget.position.y;
			translateTarget.DOLocalMoveY(0f - translateOffsetY, translateDuration).From(isRelative: true).SetEase(translateEasing);
		}
		_ = DisplayDuration;
		if (displayDuration > 0f)
		{
			yield return SharedYields.WaitForSeconds(displayDuration);
		}
		if (fadeOutDuration > 0f && fadeTarget != null)
		{
			yield return fadeTarget.DOFade(0f, fadeOutDuration).WaitForCompletion();
		}
		translateTarget.localPosition = new Vector3(translateTarget.localPosition.x, initY);
	}

	protected override IEnumerator DisplayAndDestroyCoroutine()
	{
		yield return StartCoroutine(DisplayCoroutine());
	}
}
