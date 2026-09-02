using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainGoldDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI goldGainValue;

	public override void Init(int goldGain)
	{
		base.Init();
		goldGainValue.text = string.Format("{0}{1}</style> <style=Gold></style>", (goldGain > 0) ? "<style=GoodNb>+" : "<style=BadNb>", goldGain);
	}
}
