using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingSlotEffect;

public abstract class BuildingSlotEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public int ManaCost { get; set; }

	public BuildingSlotEffectDefinition(string id, XContainer container)
		: base(container)
	{
		Id = id;
	}

	public virtual BuildingSlotEffectDefinition Clone()
	{
		return MemberwiseClone() as BuildingSlotEffectDefinition;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
