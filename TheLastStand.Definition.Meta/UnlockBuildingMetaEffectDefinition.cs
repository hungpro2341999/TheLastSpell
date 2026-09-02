using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockBuildingMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuilding";

	public string BuildingId { get; private set; }

	public UnlockBuildingMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingId = xElement.Value;
	}

	public override string ToString()
	{
		return "UnlockBuilding (" + BuildingId + ")";
	}
}
