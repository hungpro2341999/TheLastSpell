using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.View.Tooltip;
using UnityEngine;

namespace TheLastStand.View.HUD.UnitManagement;

public class MomentumIconDisplay : UnitModifiersIconDisplay
{
	public static class Constants
	{
		public const string TraveledTilesFormat = "Momentum_TraveledTiles";

		public const string SkillCurrentBonusFormat = "Momentum_SkillCurrentBonus";

		public const string SkillMaxBonusSuffix = "Momentum_SkillMaxBonusSuffix";
	}

	[SerializeField]
	private TextMeshProUGUI valueDisplay;

	private PlayableUnit playableUnit;

	public void Refresh(PlayableUnit newPlayableUnit)
	{
		playableUnit = newPlayableUnit;
		valueDisplay.text = playableUnit.MomentumTilesActive.ToString();
	}

	protected override void DiplayTooltip()
	{
		UnitHUDStatusesAndInjuriesTooltip unitHUDStatusesTooltip = UIManager.UnitHUDStatusesTooltip;
		unitHUDStatusesTooltip.TitleLocalized = GetTitle();
		unitHUDStatusesTooltip.DescriptionLocalized = GetDescription();
		unitHUDStatusesTooltip.LinesLocalized = GetLines();
		unitHUDStatusesTooltip.UseInjuryBox = false;
		unitHUDStatusesTooltip.FollowElement.ChangeTarget(base.transform);
		unitHUDStatusesTooltip.Display();
	}

	protected override void HideTooltip()
	{
		UIManager.UnitHUDStatusesTooltip.Hide();
	}

	private string GetTitle()
	{
		return "<style=Momentum>" + SkillManager.GetSkillEffectName("Momentum") + "</style>";
	}

	private string GetDescription()
	{
		return SkillManager.GetSkillEffectDescription("Momentum");
	}

	private string GetLines()
	{
		string text = Localizer.Format("Momentum_TraveledTiles", playableUnit.MomentumTilesActive);
		List<TheLastStand.Model.Skill.Skill> momentumSkills = playableUnit.MomentumSkills;
		Dictionary<string, HashSet<float>> dictionary = new Dictionary<string, HashSet<float>>();
		foreach (TheLastStand.Model.Skill.Skill item in momentumSkills)
		{
			MomentumEffectDefinition firstEffect = item.SkillAction.GetFirstEffect<MomentumEffectDefinition>("Momentum");
			float num = AttackSkillActionController.ComputeMomentumPercentage(playableUnit, firstEffect);
			if (!dictionary.TryGetValue(item.SkillDefinition.Id, out var value) || !value.Contains(num))
			{
				text = text + "\n" + Localizer.Format("Momentum_SkillCurrentBonus", item.Name, num * 100f) + ((num >= 4f) ? Localizer.Get("Momentum_SkillMaxBonusSuffix") : string.Empty);
			}
			if (!dictionary.ContainsKey(item.SkillDefinition.Id))
			{
				dictionary.Add(item.SkillDefinition.Id, new HashSet<float>());
			}
			dictionary[item.SkillDefinition.Id].Add(num);
		}
		return text;
	}
}
