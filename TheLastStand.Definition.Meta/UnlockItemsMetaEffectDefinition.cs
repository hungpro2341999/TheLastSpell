using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Manager.Item;

namespace TheLastStand.Definition.Meta;

public class UnlockItemsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockItems";

	public const string ChildName = "Item";

	public readonly List<string> ItemsToUnlock = new List<string>();

	public UnlockItemsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Item"))
		{
			if (!string.IsNullOrEmpty(item.Value))
			{
				ItemsToUnlock.Add(item.Value);
			}
		}
	}

	public override void OnMetaEffectActivated(bool hasBeenActivated)
	{
		base.OnMetaEffectActivated(hasBeenActivated);
		foreach (string item in ItemsToUnlock)
		{
			if (ItemDatabase.ItemDefinitions.TryGetValue(item, out var value))
			{
				ItemRestrictionManager.RefreshItemFamiliesLockedItemsFromCategory(value.Category, hasBeenActivated);
			}
		}
	}
}
