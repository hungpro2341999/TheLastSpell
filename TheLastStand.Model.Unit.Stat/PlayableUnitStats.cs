using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit.Trait;

namespace TheLastStand.Model.Unit.Stat;

public class PlayableUnitStats : UnitStats
{
	public PlayableUnit PlayableUnit => base.Unit as PlayableUnit;

	public PlayableUnitStatsController PlayableUnitStatsController => base.UnitStatsController as PlayableUnitStatsController;

	public int ArmorMasterAffectedItems { get; set; }

	public override DamageableType UnitType => DamageableType.Playable;

	public PlayableUnitStats(UnitStatsController statsController, PlayableUnit playableUnit)
		: base(statsController, playableUnit)
	{
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		PlayableUnitStatsController.OnRaceGenerated(PlayableUnit.RaceDefinition);
		foreach (UnitTraitDefinition unitTraitDefinition in PlayableUnit.UnitTraitDefinitions)
		{
			PlayableUnitStatsController.OnTraitGenerated(unitTraitDefinition);
		}
		PlayableUnitStatsController.RefreshEquipmentValues();
		base.Deserialize(container, saveVersion);
	}
}
