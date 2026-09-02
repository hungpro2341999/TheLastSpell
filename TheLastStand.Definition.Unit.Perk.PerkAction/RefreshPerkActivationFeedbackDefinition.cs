using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class RefreshPerkActivationFeedbackDefinition : APerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "RefreshPerkActivationFeedback";
	}

	public bool RefreshView { get; private set; }

	public RefreshPerkActivationFeedbackDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("RefreshView");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				RefreshView = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse RefreshView attribute into a bool in RefreshPerkActivationFeedbackDefinition", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "RefreshPerkActivationFeedbackDefinition");
			}
		}
		else
		{
			RefreshView = true;
		}
	}
}
