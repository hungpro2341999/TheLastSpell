using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockShopRerollMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockShopReroll";

	public UnlockShopRerollMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
