using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockCitiesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockCities";

	public const string ChildName = "City";

	public List<string> CitiesToUnlock = new List<string>();

	public UnlockCitiesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("City"))
		{
			if (item.Value != string.Empty)
			{
				CitiesToUnlock.Add(item.Value);
			}
		}
	}
}
