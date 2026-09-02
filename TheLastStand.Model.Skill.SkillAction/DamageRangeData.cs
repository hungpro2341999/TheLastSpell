using UnityEngine;

namespace TheLastStand.Model.Skill.SkillAction;

public class DamageRangeData
{
	public Vector2Int BaseDamageRange;

	public Vector2Int IsolatedDamageRange = Vector2Int.zero;

	public Vector2Int OpportunismDamageRange = Vector2Int.zero;

	public Vector2Int MomentumDamageRange = Vector2Int.zero;

	public Vector2Int PerksDamageRange = Vector2Int.zero;

	public Vector2Int ResistanceReductionRange = Vector2Int.zero;

	public Vector2Int BlockReductionRange = Vector2Int.zero;

	public Vector2Int BlockableDamageRange
	{
		get
		{
			Vector2Int damageToClamp = Vector2Int.Min(AllDamageSumRange + ResistanceReductionRange, new Vector2Int(Mathf.Abs(BlockReductionRange.x), Mathf.Abs(BlockReductionRange.y)));
			ClampDamage(ref damageToClamp);
			return damageToClamp;
		}
	}

	public Vector2Int FinalDamageRange
	{
		get
		{
			Vector2Int damageToClamp = AllDamageSumRange + ResistanceReductionRange + BlockReductionRange;
			ClampDamage(ref damageToClamp);
			return damageToClamp;
		}
	}

	private Vector2Int AllDamageSumRange => BaseDamageRange + IsolatedDamageRange + OpportunismDamageRange + MomentumDamageRange + PerksDamageRange;

	public override string ToString()
	{
		return ToString(" | ");
	}

	public string ToString(string separators)
	{
		return $"BaseDamageRange: {BaseDamageRange}{separators}IsolatedDamageRange: {IsolatedDamageRange}{separators}OpportunismDamageRange: {OpportunismDamageRange}{separators}MomentumDamageRange: {MomentumDamageRange}{separators}PerksDamageRange: {PerksDamageRange}{separators}ResistanceReductionRange: {ResistanceReductionRange}{separators}BlockReductionRange: {BlockReductionRange}{separators}BlockableDamageRange: {BlockableDamageRange}{separators}FinalDamageRange: {FinalDamageRange}";
	}

	private void ClampDamage(ref Vector2Int damageToClamp)
	{
		damageToClamp.x = Mathf.Max(damageToClamp.x, 0);
		damageToClamp.y = Mathf.Max(damageToClamp.y, damageToClamp.x);
	}
}
