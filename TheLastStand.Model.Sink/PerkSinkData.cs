using TheLastStand.Definition;

namespace TheLastStand.Model.Sink;

public class PerkSinkData : SinkData
{
	public PerkSinkDataDefinition PerkSinkDataDefinition => base.SinkDataDefinition as PerkSinkDataDefinition;

	public int PricePerPerk => PerkSinkDataDefinition.PricePerPerk;

	public float RerollBaseChances => PerkSinkDataDefinition.RerollBaseChances;

	public int Perks { get; set; }

	public PerkSinkData(PerkSinkDataDefinition sinkDataDefinition)
		: base(sinkDataDefinition)
	{
	}
}
