namespace TheLastStand.Definition.Skill.SkillAction;

public static class AttackTypeExtensions
{
	public static string GetValueStylized(this AttackSkillActionDefinition.E_AttackType attackType, string value)
	{
		if (attackType == AttackSkillActionDefinition.E_AttackType.None)
		{
			return value;
		}
		return $"<style={attackType}Damage>{value}</style>";
	}
}
