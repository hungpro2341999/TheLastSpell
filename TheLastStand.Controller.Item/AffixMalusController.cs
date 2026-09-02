using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization;

namespace TheLastStand.Controller.Item;

public class AffixMalusController
{
	public AffixMalus AffixMalus { get; private set; }

	public AffixMalusController(SerializedAffixMalus container)
	{
		AffixMalus = new AffixMalus(container, this);
	}

	public AffixMalusController(AffixMalusDefinition affixMalusDefinition)
	{
		AffixMalus = new AffixMalus(affixMalusDefinition, this);
	}

	public void SetLevel(AffixMalusDefinition.E_MalusLevel malusLevel)
	{
		AffixMalus.MalusLevel = malusLevel;
	}
}
