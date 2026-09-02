using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public class CastFxDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class CamShakeDefinition : TheLastStand.Framework.Serialization.Definition
	{
		public Node Delay { get; private set; }

		public string Id { get; private set; }

		public CamShakeDefinition(XContainer container)
			: base(container)
		{
		}

		public override void Deserialize(XContainer container)
		{
			XElement xElement = container.Element("Id");
			if (xElement != null)
			{
				Id = xElement.Value;
				XElement xElement2 = container.Element("Delay");
				Delay = ((xElement2 != null) ? Parser.Parse(xElement2.Value) : new NodeNumber(0.0));
			}
			else
			{
				Debug.LogError("CamShakeDefinition needs to define an Id!");
			}
		}
	}

	public List<CamShakeDefinition> CamShakeDefinitions { get; private set; } = new List<CamShakeDefinition>();

	public Node CastTotalDuration { get; private set; } = new NodeNumber(0.20000000298023224);

	public List<SoundEffectDefinition> SoundEffectDefinitionsOnCast { get; } = new List<SoundEffectDefinition>();

	public List<SoundEffectDefinition> SoundEffectDefinitionsOnImpact { get; } = new List<SoundEffectDefinition>();

	public List<VisualEffectDefinition> VisualEffectDefinitions { get; } = new List<VisualEffectDefinition>();

	public CastFxDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		if (!(xContainer is XElement xElement))
		{
			return;
		}
		XAttribute xAttribute = xElement.Attribute("TotalDuration");
		if (!xAttribute.IsNullOrEmpty())
		{
			CastTotalDuration = Parser.Parse(xAttribute.Value);
		}
		foreach (XElement item in xElement.Elements("VisualEffect"))
		{
			VisualEffectDefinitions.Add(new StandardVisualEffectDefinition(item));
		}
		foreach (XElement item2 in xElement.Elements("SoundEffect"))
		{
			XElement xElement2 = item2.Element("OnImpact");
			if (xElement2 != null)
			{
				if (bool.TryParse(xElement2.Value, out var result))
				{
					if (result)
					{
						SoundEffectDefinitionsOnImpact.Add(new SoundEffectDefinition(item2));
					}
					else
					{
						SoundEffectDefinitionsOnCast.Add(new SoundEffectDefinition(item2));
					}
				}
			}
			else
			{
				SoundEffectDefinitionsOnCast.Add(new SoundEffectDefinition(item2));
			}
		}
		CamShakeDefinitions = new List<CamShakeDefinition>();
		foreach (XElement item3 in xElement.Elements("CamShake"))
		{
			CamShakeDefinitions.Add(new CamShakeDefinition(item3));
		}
	}
}
