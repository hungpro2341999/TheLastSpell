using System;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillEffectFeedback : TooltipBase
{
	private readonly string[] displayedEffects = new string[2] { "MultiHit", "Inaccurate" };

	[SerializeField]
	private TextMeshProUGUI text;

	private TheLastStand.Model.Skill.Skill SelectedSkill => PlayableUnitManager.SelectedSkill ?? BuildingManager.SelectedSkill;

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	protected override bool CanBeDisplayed()
	{
		if (SelectedSkill != null)
		{
			if (!SelectedSkill.SkillAction.HasAnyEffect(displayedEffects) && !SelectedSkill.SkillDefinition.CanRotate && !SkillManager.DebugSkillsForceCanRotate && !SelectedSkill.SkillDefinition.CanFlip && !SkillManager.DebugSkillsForceCanFlip)
			{
				if (SelectedSkill.SkillAction is AttackSkillAction attackSkillAction)
				{
					return attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Ranged;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void OnDisplay()
	{
		base.OnDisplay();
		base.FollowElement.ChangeTarget(InputManager.IsLastControllerJoystick ? TPSingleton<CursorView>.Instance.JoystickCursorView.transform : null);
		base.FollowElement.ConvertToScreenSpace = InputManager.IsLastControllerJoystick;
	}

	protected override void RefreshContent()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string skillFlipOrRotationFeedbackText = GetSkillFlipOrRotationFeedbackText();
		if (!string.IsNullOrEmpty(skillFlipOrRotationFeedbackText))
		{
			stringBuilder.AppendLine(skillFlipOrRotationFeedbackText);
		}
		string[] array = displayedEffects;
		foreach (string text in array)
		{
			if (SelectedSkill.SkillAction.TryGetFirstEffect<SkillEffectDefinition>(text, out var effect) && text != null && text == "MultiHit")
			{
				stringBuilder.AppendLine(GetFeedbackText(effect as MultiHitSkillEffectDefinition));
			}
		}
		string dodgeModifierFeedbackText = GetDodgeModifierFeedbackText();
		if (!string.IsNullOrEmpty(dodgeModifierFeedbackText))
		{
			stringBuilder.AppendLine(dodgeModifierFeedbackText);
		}
		this.text.text = stringBuilder.ToString();
	}

	private string GetFeedbackText(MultiHitSkillEffectDefinition definition)
	{
		int num = ((SelectedSkill.SkillAction.SkillActionExecution.Caster is PlayableUnit playableUnit) ? playableUnit.PlayableUnitController.GetModifiedMultiHitsCount(definition.GetHitsCount(definition.GetPerkContext(playableUnit))) : definition.HitsCount);
		return $"{SkillManager.GetSkillEffectName(definition.Id)} {SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count + 1}/{num}";
	}

	private string GetDodgeModifierFeedbackText()
	{
		float selectedSkillDodgeMultiplierWithDistance = SkillManager.GetSelectedSkillDodgeMultiplierWithDistance();
		if (selectedSkillDodgeMultiplierWithDistance == 1f)
		{
			return string.Empty;
		}
		return Localizer.Get("DamageTypeModifierName_DodgeMultiplier") + " x" + selectedSkillDodgeMultiplierWithDistance;
	}

	private string GetSkillFlipOrRotationFeedbackText()
	{
		bool flag = SelectedSkill != null && (SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT);
		bool flag2 = SelectedSkill != null && (SelectedSkill.SkillDefinition.CanFlip || SkillManager.DebugSkillsForceCanFlip);
		if (SelectedSkill == null || (!flag2 && !flag))
		{
			return string.Empty;
		}
		string text = string.Empty;
		string[] localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction("RotateSkill");
		if (localizedHotkeysForAction != null && localizedHotkeysForAction.Length != 0)
		{
			text = localizedHotkeysForAction[0];
		}
		if (flag)
		{
			return Localizer.Format("SkillRotation_FeedbackText", text);
		}
		return Localizer.Format("SkillFlip_FeedbackText", text);
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshContent();
		}
	}
}
