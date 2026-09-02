using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillTooltip : SkillWithoutCompendiumTooltip
{
	[SerializeField]
	[Tooltip("If left to null, it will automatically use the one from SkillManager")]
	private CompendiumPanel compendiumPanel;

	[SerializeField]
	private Transform compendiumLeftBottomAnchor;

	[SerializeField]
	private Transform compendiumRightBottomAnchor;

	public bool CompendiumFollowRight { get; set; }

	protected override void Awake()
	{
		if (compendiumPanel == null)
		{
			compendiumPanel = SkillManager.CompendiumPanel;
		}
		base.Awake();
	}

	protected override void OnHide()
	{
		base.OnHide();
		compendiumPanel.Clear();
		compendiumPanel.Hide();
	}

	protected override void RefreshContent()
	{
		base.RefreshContent();
		compendiumPanel.Clear();
		DisplayCompendiumPanel();
	}

	private void DisplayCompendiumPanel()
	{
		if (skillDisplay.Skill.SkillAction is AttackSkillAction attackSkillAction)
		{
			compendiumPanel.AddDamageType(attackSkillAction);
		}
		Dictionary<string, List<SkillEffectDefinition>> dictionary = (skillDisplay.DisplayPerkSkillEffects ? skillDisplay.Skill.SkillAction.GetAllEffects() : skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions);
		if (dictionary != null)
		{
			compendiumPanel.AddSkillEffectIds(dictionary);
		}
		if (compendiumPanel.CompendiumEntries.Count > 0 && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			RefreshCompendiumPanelFollowTarget();
			RefreshCompendiumPanelOffset();
			compendiumPanel.UpdateAnchor(CompendiumFollowRight ? CompendiumPanel.AnchorType.LeftBot : CompendiumPanel.AnchorType.RightBot);
			compendiumPanel.Display();
		}
	}

	private void RefreshCompendiumPanelFollowTarget()
	{
		compendiumPanel.FollowElement.ChangeTarget(CompendiumFollowRight ? compendiumRightBottomAnchor : compendiumLeftBottomAnchor);
	}

	private void RefreshCompendiumPanelOffset()
	{
		if (skillDisplay.SkillAreaOfEffectGrid.Displayed)
		{
			skillDisplay.SkillAreaOfEffectGridPlacedEvent += UpdateSkillEffectTooltipsPanelOffset;
			return;
		}
		Vector3 offset = new Vector3(0f, compendiumPanel.FollowElement.FollowElementDatas.Offset.y, compendiumPanel.FollowElement.FollowElementDatas.Offset.z);
		compendiumPanel.FollowElement.ChangeOffset(offset);
	}

	private void UpdateSkillEffectTooltipsPanelOffset()
	{
		skillDisplay.SkillAreaOfEffectGridPlacedEvent -= UpdateSkillEffectTooltipsPanelOffset;
		Vector3 offset = new Vector3(0f, compendiumPanel.FollowElement.FollowElementDatas.Offset.y, compendiumPanel.FollowElement.FollowElementDatas.Offset.z);
		if (compendiumPanel.FollowElement.FollowElementDatas.FollowTarget == compendiumRightBottomAnchor)
		{
			float b = skillDisplay.SkillAreaOfEffectGrid.RectTransform.localPosition.x + skillDisplay.SkillParametersContainer.localPosition.x + skillDisplay.SkillAreaOfEffectGrid.RectTransform.sizeDelta.x - base.RectTransform.sizeDelta.x;
			b = Mathf.Max(0f, b);
			offset = new Vector3(b, compendiumPanel.FollowElement.FollowElementDatas.Offset.y, compendiumPanel.FollowElement.FollowElementDatas.Offset.z);
			compendiumPanel.FollowElement.ChangeOffset(offset);
		}
		else
		{
			compendiumPanel.FollowElement.ChangeOffset(offset);
		}
	}
}
