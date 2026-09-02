using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

public abstract class MetaEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public MetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public virtual void OnMetaEffectActivated(bool hasBeenActivated)
	{
	}
}
