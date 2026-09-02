using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockRacesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockRaces";

	public const string ChildName = "Race";

	public List<string> RacesToUnlock = new List<string>();

	public UnlockRacesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Race"))
		{
			if (item.Value != string.Empty)
			{
				RacesToUnlock.Add(item.Value);
			}
		}
	}
}
