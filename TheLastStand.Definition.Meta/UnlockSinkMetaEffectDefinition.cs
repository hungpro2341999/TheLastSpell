using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockSinkMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockSink";

	public UnlockSinkMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
