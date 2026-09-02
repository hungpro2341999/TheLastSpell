using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockAffixesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockAffixes";

	public const string ChildName = "Affix";

	public List<string> AffixesToUnlock = new List<string>();

	public UnlockAffixesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Affix"))
		{
			if (item.Value != string.Empty)
			{
				AffixesToUnlock.Add(item.Value);
			}
		}
	}
}
