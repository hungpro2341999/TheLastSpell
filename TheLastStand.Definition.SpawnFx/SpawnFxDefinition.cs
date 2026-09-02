using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.SpawnFx;

public class SpawnFxDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<CastFxDefinition.CamShakeDefinition> CamShakeDefinitions { get; private set; } = new List<CastFxDefinition.CamShakeDefinition>();

	public Node CastTotalDuration { get; private set; } = new NodeNumber(0.20000000298023224);

	public List<SoundEffectDefinition> SoundEffectDefinitions { get; private set; } = new List<SoundEffectDefinition>();

	public List<SpawnVisualEffectDefinition> SpawnVisualEffectDefinition { get; private set; } = new List<SpawnVisualEffectDefinition>();

	public SpawnFxDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("TotalDuration");
		if (!xAttribute.IsNullOrEmpty())
		{
			CastTotalDuration = Parser.Parse(xAttribute.Value);
		}
		SpawnVisualEffectDefinition = new List<SpawnVisualEffectDefinition>();
		foreach (XElement item in xElement.Elements("SpawnVisualEffect"))
		{
			if (item != null)
			{
				SpawnVisualEffectDefinition.Add(new SpawnVisualEffectDefinition(item));
			}
		}
		SoundEffectDefinitions = new List<SoundEffectDefinition>();
		foreach (XElement item2 in xElement.Elements("SoundEffect"))
		{
			if (item2 != null)
			{
				SoundEffectDefinitions.Add(new SoundEffectDefinition(item2));
			}
		}
		CamShakeDefinitions = new List<CastFxDefinition.CamShakeDefinition>();
		foreach (XElement item3 in xElement.Elements("CamShake"))
		{
			if (item3 != null)
			{
				CamShakeDefinitions.Add(new CastFxDefinition.CamShakeDefinition(item3));
			}
		}
	}
}
