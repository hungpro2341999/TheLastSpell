using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class LockItemsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "LockItems";

	public const string ChildName = "Item";

	public readonly List<string> ItemsToLock = new List<string>();

	public LockItemsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Item"))
		{
			if (!string.IsNullOrEmpty(item.Value))
			{
				ItemsToLock.Add(item.Value);
			}
		}
	}
}
