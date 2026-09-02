using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainMaterialDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI materialGainValue;

	public override void Init(int materialGain)
	{
		base.Init();
		materialGainValue.text = string.Format("{0}{1}</style> <style=Materials></style>", (materialGain > 0) ? "<style=GoodNb>+" : "<style=BadNb>", materialGain);
	}
}
