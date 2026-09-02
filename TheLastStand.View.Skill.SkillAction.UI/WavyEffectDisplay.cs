using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class WavyEffectDisplay : AppearingEffectDisplay
{
	[SerializeField]
	private Vector2 spawnPosOffsetXRange = new Vector2(-10f, 10f);

	[SerializeField]
	protected TextMeshProUGUI valueLbl;

	[SerializeField]
	private float translateXOffsetValue = 2f;

	[SerializeField]
	private AnimationCurve translateXEasingCurve;

	[SerializeField]
	private Vector2 translateYOffsetRange = new Vector2(20f, 35f);

	private float internalTranslateXOffset;

	private float originalAlpha = -1f;

	public override void Init(int value)
	{
		base.Init(value);
		if (originalAlpha != -1f)
		{
			valueLbl.alpha = originalAlpha;
		}
		else
		{
			originalAlpha = valueLbl.alpha;
		}
		base.transform.localPosition += new Vector3(spawnPosOffsetXRange.RandomIntInRange(), 0f, 0f);
		internalTranslateXOffset = translateXOffsetValue * (float)((!TPHelpers.RandomBool()) ? 1 : (-1));
		internalTranslateOffsetY += translateYOffsetRange.RandomIntInRange();
	}

	protected override IEnumerator DisplayCoroutine()
	{
		if (translateTarget != null)
		{
			translateTarget.DOBlendableLocalMoveBy(new Vector3(internalTranslateXOffset, 0f), translateDuration).SetEase(translateXEasingCurve);
		}
		yield return base.DisplayCoroutine();
	}
}
