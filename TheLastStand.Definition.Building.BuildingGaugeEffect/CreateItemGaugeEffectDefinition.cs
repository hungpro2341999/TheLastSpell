using System.Xml.Linq;
using TheLastStand.Definition.Item;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class CreateItemGaugeEffectDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "CreateItem";

	public CreateItemDefinition CreateItemDefinition { get; set; }

	public CreateItemGaugeEffectDefinition(XContainer container)
		: base("CreateItem", container)
	{
	}

	public override BuildingGaugeEffectDefinition Clone()
	{
		CreateItemGaugeEffectDefinition obj = base.Clone() as CreateItemGaugeEffectDefinition;
		obj.CreateItemDefinition = CreateItemDefinition;
		return obj;
	}
}
