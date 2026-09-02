using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockBuildingActionMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuildingAction";

	public string BuildingActionId { get; private set; }

	public UnlockBuildingActionMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Id");
		BuildingActionId = xAttribute.Value;
	}
}
