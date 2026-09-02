using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.BonePile;

public class BoneZonesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<BoneZoneDefinition> BoneZoneDefinitions { get; private set; }

	public BoneZonesDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		BoneZoneDefinitions = new List<BoneZoneDefinition>();
		foreach (XElement item2 in (container as XElement).Elements("BoneZoneDefinition"))
		{
			BoneZoneDefinition item = new BoneZoneDefinition(item2);
			BoneZoneDefinitions.Add(item);
		}
		BoneZoneDefinitions.Sort((BoneZoneDefinition a, BoneZoneDefinition b) => (a.MaxMagicCircleDistance <= -1 || b.MaxMagicCircleDistance <= -1) ? b.MaxMagicCircleDistance.CompareTo(a.MaxMagicCircleDistance) : a.MaxMagicCircleDistance.CompareTo(b.MaxMagicCircleDistance));
	}
}
