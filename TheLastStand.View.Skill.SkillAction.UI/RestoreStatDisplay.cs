using TheLastStand.Definition.Unit;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class RestoreStatDisplay : WavyEffectDisplay
{
	public new static class Constants
	{
		public const string RestoreStatDisplayPrefab = "Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay";
	}

	public void Init(UnitStatDefinition.E_Stat stat, int restoreValue)
	{
		Init(restoreValue);
		valueLbl.text = "<style=RegenStat></style> " + stat.GetValueStylized(restoreValue) + " " + stat.GetStylizedCustomContent(string.Empty);
	}
}
