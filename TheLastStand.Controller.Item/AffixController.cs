using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization;

namespace TheLastStand.Controller.Item;

public class AffixController
{
	public Affix Affix { get; private set; }

	public AffixController(SerializedAffix container)
	{
		Affix = new Affix(container, this);
	}

	public AffixController(AffixDefinition affixDefinition)
	{
		Affix = new Affix(affixDefinition, this);
	}
}
