using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Status;

public struct StatusCreationInfo
{
	public ISkillCaster Source;

	public UnitStatDefinition.E_Stat Stat;

	public int TurnsCount;

	public float Value;

	public bool IsFromInjury;

	public bool IsFromPerk;

	public bool HideDisplayEffect;

	public StatusSourceInfo DelayedSourceInfo;
}
