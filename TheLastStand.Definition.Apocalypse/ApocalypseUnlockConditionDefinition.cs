using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Apocalypse;

namespace TheLastStand.Definition.Apocalypse;

public abstract class ApocalypseUnlockConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public virtual string Name { get; }

	public ApocalypseUnlockCondition ApocalypseUnlockCondition { get; set; }

	public ApocalypseUnlockConditionDefinition(XContainer container)
		: base(container)
	{
	}
}
