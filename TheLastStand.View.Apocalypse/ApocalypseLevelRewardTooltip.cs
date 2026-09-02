using System.Collections.Generic;
using System.Text;
using TMPro;
using TPLib.Localization;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseLevelRewardTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI levelToReachText;

	[SerializeField]
	private TextMeshProUGUI rewardsText;

	[SerializeField]
	private bool isGameOverVersion;

	private int[] levelsToDisplay;

	private List<ApocalypseRewardTextFormatting> rewardsTextFormatting = new List<ApocalypseRewardTextFormatting>();

	public bool IsGameOverVersion => isGameOverVersion;

	public void Init(int[] apocalypseLevelsToDisplay, List<ApocalypseRewardTextFormatting> apocalypseRewardsTextFormatting)
	{
		levelsToDisplay = apocalypseLevelsToDisplay;
		rewardsTextFormatting = apocalypseRewardsTextFormatting;
	}

	protected override bool CanBeDisplayed()
	{
		if (levelsToDisplay != null && levelsToDisplay.Length != 0)
		{
			return rewardsTextFormatting.Count > 0;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = levelsToDisplay.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(levelsToDisplay[i]);
			if (i + 1 < num)
			{
				stringBuilder.Append(", ");
			}
		}
		if (isGameOverVersion)
		{
			levelToReachText.text = Localizer.Format("ApocalypseLevelReward_LevelsCompleted", stringBuilder.ToString());
		}
		else
		{
			levelToReachText.text = stringBuilder.ToString();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		int num2 = 0;
		int count = rewardsTextFormatting.Count;
		foreach (ApocalypseRewardTextFormatting item in rewardsTextFormatting)
		{
			stringBuilder2.Append(item.GetLocalizedText() ?? "");
			num2++;
			if (num2 < count)
			{
				stringBuilder2.AppendLine();
			}
		}
		rewardsText.text = stringBuilder2.ToString();
	}
}
