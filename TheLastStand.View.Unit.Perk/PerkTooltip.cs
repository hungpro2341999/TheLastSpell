using TPLib;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Generic;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;

namespace TheLastStand.View.Unit.Perk;

public class PerkTooltip : TooltipBase
{
	[SerializeField]
	private UnitPerkDisplay unitPerkDisplay;

	[SerializeField]
	private SkillWithoutCompendiumTooltip skillTooltip;

	[SerializeField]
	private CompendiumPanel compendiumPanel;

	[SerializeField]
	private RectTransform tooltipRectTransform;

	[SerializeField]
	private GameObject perkPanel;

	[SerializeField]
	private GameObject cycleHelper;

	private TheLastStand.Model.Skill.Skill currentSkill;

	private int currentSkillIndex;

	public SkillWithoutCompendiumTooltip SkillTooltip => skillTooltip;

	public CompendiumPanel CompendiumPanel => compendiumPanel;

	public RectTransform TooltipPanel => tooltipPanel;

	public RectTransform TooltipRectTransform => tooltipRectTransform;

	public void SetContent(TheLastStand.Model.Unit.Perk.Perk perk, PerkDefinition perkDefinition = null)
	{
		unitPerkDisplay.SetContent(perk, perkDefinition);
	}

	public void UpdateAnchors(bool displayTowardsRight, bool displayTop = false)
	{
		Vector2 vector;
		if (displayTowardsRight)
		{
			CompendiumPanel.transform.SetAsLastSibling();
			vector = Vector2.up;
		}
		else
		{
			CompendiumPanel.transform.SetAsFirstSibling();
			vector = Vector2.one;
		}
		if (displayTop)
		{
			vector.y = 0f;
		}
		TooltipRectTransform.anchorMin = vector;
		TooltipRectTransform.anchorMax = vector;
		TooltipRectTransform.pivot = vector;
	}

	protected override void Awake()
	{
		perkPanel.SetActive(value: true);
		base.Awake();
	}

	protected override bool CanBeDisplayed()
	{
		if (unitPerkDisplay.Perk == null)
		{
			return unitPerkDisplay.PerkDefinition != null;
		}
		return true;
	}

	protected override void RefreshContent()
	{
		unitPerkDisplay.Init();
		cycleHelper.SetActive(unitPerkDisplay.PerkDefinition != null && unitPerkDisplay.PerkDefinition.SkillsToShow.Count > 1);
		DisplaySkillTooltip();
		DisplayCompendium();
	}

	protected override void OnHide()
	{
		base.OnHide();
		currentSkill = null;
		currentSkillIndex = 0;
		compendiumPanel.Hide();
		skillTooltip.Hide();
	}

	private void DisplayCompendium()
	{
		if (currentSkill != null)
		{
			if (currentSkill.SkillDefinition.SkillActionDefinition.SkillEffectDefinitions != null)
			{
				compendiumPanel.AddSkillEffectIds(currentSkill.SkillDefinition.SkillActionDefinition.SkillEffectDefinitions);
			}
			if (currentSkill.SkillAction is AttackSkillAction attackSkillAction)
			{
				compendiumPanel.AddDamageType(attackSkillAction);
			}
		}
		foreach (CompendiumEntryDefinition compendiumEntry in unitPerkDisplay.PerkDefinition.CompendiumEntries)
		{
			compendiumPanel.AddCompendiumEntry(compendiumEntry.Id, compendiumEntry.DisplayLinkedEntries);
		}
		if (compendiumPanel.CompendiumEntries.Count > 0 && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			compendiumPanel.Display();
		}
	}

	private void DisplaySkillTooltip()
	{
		if (unitPerkDisplay.PerkDefinition.SkillsToShow.Count > currentSkillIndex && SkillDatabase.SkillDefinitions.ContainsKey(unitPerkDisplay.PerkDefinition.SkillsToShow[currentSkillIndex].Item1))
		{
			SkillDefinition skillDefinition = SkillDatabase.SkillDefinitions[unitPerkDisplay.PerkDefinition.SkillsToShow[currentSkillIndex].Item1];
			currentSkill = new SkillController(skillDefinition, unitPerkDisplay.Perk, unitPerkDisplay.PerkDefinition.SkillsToShow[currentSkillIndex].Item2).Skill;
			SkillTooltip.SetContent(currentSkill, TileObjectSelectionManager.SelectedPlayableUnit);
			SkillTooltip.DisplayInvalidityPanel = false;
			SkillTooltip.Display();
		}
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(48) && base.Displayed && unitPerkDisplay.PerkDefinition != null && unitPerkDisplay.PerkDefinition.SkillsToShow.Count > 1)
		{
			if (++currentSkillIndex >= unitPerkDisplay.PerkDefinition.SkillsToShow.Count)
			{
				currentSkillIndex = 0;
			}
			DisplaySkillTooltip();
			compendiumPanel.Clear();
			DisplayCompendium();
		}
	}
}
