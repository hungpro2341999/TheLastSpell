using UnityEngine;

namespace TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;

public class AttackSkillActionExecutionTileData : SkillActionExecutionTileData
{
	public enum E_AttackDataParameter
	{
		ArmorDamage,
		Dodged,
		HealthDamage,
		IsCrit
	}

	public float ComputationRandom { get; set; }

	public DamageRangeData DamageRangeData { get; set; }

	public IDamageable Damageable { get; set; }

	public float ArmorDamage { get; set; }

	public float BlockedDamage { get; set; }

	public bool Dodged { get; set; }

	public float HealthDamage { get; set; }

	public bool IsCrit { get; set; }

	public bool IsInstantKill { get; set; }

	public bool IsPoison { get; set; }

	public bool IsPropagatedTile { get; set; }

	public float TargetArmorTotal { get; set; }

	public float TargetHealthTotal { get; set; }

	public float TargetRemainingArmor { get; set; }

	public float TargetRemainingHealth { get; set; }

	public float TotalDamage { get; set; }

	public float HealthDamageIncludingOverkill => HealthDamage + OverkillDamage;

	public float OverkillDamage => Mathf.Max(0f, TotalDamage - HealthDamage - ArmorDamage);

	public override string ToString()
	{
		string text = (IsCrit ? "Critical " : string.Empty) + (IsInstantKill ? "InstantKill " : string.Empty) + (IsPoison ? "Poison " : string.Empty) + (IsPropagatedTile ? "Propagated " : string.Empty);
		return $"ComputationRandom\t:\t{ComputationRandom}\n" + "DamageRangeData\t:\t" + DamageRangeData?.ToString("\n\t\t\t\t") + "\n" + $"Dodged\t\t\t:\t{Dodged}\n" + "Specifics\t\t:\t" + text + "\n" + $"ArmorDamage\t\t:\t{ArmorDamage}\n" + $"HealthDamage\t\t:\t{HealthDamage}\n" + $"TargetArmorTotal\t:\t{TargetArmorTotal}\n" + $"TargetHealthTotal\t:\t{TargetHealthTotal}\n" + $"TargetRemainingArmor\t:\t{TargetRemainingArmor}\n" + $"TargetRemainingHealth\t:\t{TargetRemainingHealth}\n" + $"TotalDamage\t\t:\t{TotalDamage}\n" + $"OverkillDamage\t\t:\t{OverkillDamage}\n" + $"BlockedDamage\t\t:\t{BlockedDamage}";
	}
}
