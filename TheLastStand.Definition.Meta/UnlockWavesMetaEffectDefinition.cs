using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockWavesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockWaves";

	public const string ChildName = "Wave";

	public List<string> WavesToUnlock = new List<string>();

	public UnlockWavesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		foreach (XElement item in (container as XElement).Elements("Wave"))
		{
			if (item.Value != string.Empty)
			{
				WavesToUnlock.Add(item.Value);
			}
		}
	}
}
