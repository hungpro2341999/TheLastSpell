using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class DamageDisplay : EffectDisplay
{
	public new static class Constants
	{
		public const string DodgedLabel = "DodgeFeedback";

		public const string CriticalFormatting = "DamageDisplay_CriticalHitFormatting";
	}

	[SerializeField]
	private float critFontSizeMult = 2f;

	[SerializeField]
	private TextMeshProUGUI damageText;

	[SerializeField]
	private DataColor blinkColor;

	[SerializeField]
	private DataColor damageColor;

	[SerializeField]
	private DataColor poisonDamageColor;

	[SerializeField]
	private DataColor dodgeColor;

	[SerializeField]
	private DataColor critColor;

	[SerializeField]
	private float blinkDuration = 0.1f;

	[SerializeField]
	private Ease blinkEasing = Ease.InCirc;

	[SerializeField]
	private Ease fadeEasing = Ease.OutQuart;

	[SerializeField]
	private Transform translateTarget;

	[SerializeField]
	private Vector2 translateOffsetYRange = new Vector2(30f, 60f);

	[SerializeField]
	private Vector2 translateOffsetYRangeSpecial = new Vector2(30f, 60f);

	[SerializeField]
	private Vector2 translateOffsetXRange = new Vector2(-2f, 40f);

	[SerializeField]
	private AnimationCurve translateYEasingCurve;

	[SerializeField]
	private Ease translateXEasing = Ease.OutQuart;

	private float translateYOffset;

	private float translateXOffset;

	private float defaultFontSize = 18f;

	private void Awake()
	{
		defaultFontSize = damageText.fontSize;
	}

	public void Init(int damage)
	{
		damageText.text = damage.ToString();
		damageText.fontSize = defaultFontSize;
		damageText.color = damageColor._Color;
		translateYOffset = translateOffsetYRange.RandomIntInRange();
	}

	public void Init(AttackSkillActionExecutionTileData attackData)
	{
		translateXOffset = translateOffsetXRange.RandomIntInRange() * ((!TPHelpers.RandomBool()) ? 1 : (-1));
		if (attackData.Dodged)
		{
			damageText.fontSize = defaultFontSize;
			damageText.text = Localizer.Get("DodgeFeedback");
			damageText.color = dodgeColor._Color;
			translateYOffset = translateOffsetYRangeSpecial.RandomIntInRange();
			return;
		}
		float num = attackData.HealthDamage + attackData.ArmorDamage + attackData.OverkillDamage;
		damageText.text = ((int)num).ToString();
		if (attackData.IsCrit)
		{
			damageText.text = Localizer.Format("DamageDisplay_CriticalHitFormatting", damageText.text);
			damageText.fontSize = defaultFontSize * critFontSizeMult;
			damageText.color = critColor._Color;
			translateYOffset = translateOffsetYRangeSpecial.RandomIntInRange();
		}
		else
		{
			damageText.fontSize = defaultFontSize;
			damageText.color = (attackData.IsPoison ? poisonDamageColor._Color : damageColor._Color);
			translateYOffset = translateOffsetYRange.RandomIntInRange();
		}
	}

	protected override IEnumerator DisplayCoroutine()
	{
		Color color = damageText.color;
		color.a = 0f;
		damageText.DOBlendableColor(blinkColor._Color, blinkDuration).From().SetEase(blinkEasing);
		damageText.DOBlendableColor(color, DisplayDuration).SetEase(fadeEasing);
		translateTarget?.DOBlendableLocalMoveBy(new Vector3(0f, translateYOffset), DisplayDuration).SetEase(translateYEasingCurve);
		translateTarget?.DOBlendableLocalMoveBy(new Vector3(translateXOffset, 0f), DisplayDuration).SetEase(translateXEasing);
		yield return base.DisplayCoroutine();
	}
}
