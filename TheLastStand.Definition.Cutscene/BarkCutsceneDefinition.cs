using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class BarkCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public enum E_BarkerType
	{
		Unit,
		MagicCircle,
		BossUnit,
		CutsceneUnit
	}

	public enum E_LookAtTarget
	{
		None,
		Barker,
		MagicCircle
	}

	public enum E_MoveCamera
	{
		Never,
		IfOutOfScreen,
		Always
	}

	public static class Constants
	{
		public const string Id = "Bark";
	}

	public BarkDefinition BarkDefinition { get; private set; }

	public string BarkerId { get; private set; }

	public E_BarkerType BarkerType { get; private set; }

	public E_LookAtTarget LookAtTarget { get; private set; }

	public E_MoveCamera MoveCamera { get; private set; }

	public BarkCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement container2 = obj.Element("BarkDefinition");
		BarkDefinition = new BarkDefinition(container2);
		BarkDatabase.BarkDefinitions.Add(BarkDefinition.Id, BarkDefinition);
		XAttribute xAttribute = obj.Attribute("BarkerType");
		if (Enum.TryParse<E_BarkerType>(xAttribute.Value, out var result))
		{
			BarkerType = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse " + xAttribute.Value + " as a valid BarkerType.");
		}
		XAttribute xAttribute2 = obj.Attribute("BarkerId");
		if (!string.IsNullOrEmpty(xAttribute2?.Value))
		{
			BarkerId = xAttribute2.Value;
		}
		XElement xElement = obj.Element("LookAt");
		if (xElement != null)
		{
			if (Enum.TryParse<E_LookAtTarget>(xElement.Value, out var result2))
			{
				LookAtTarget = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse " + xElement.Value + " as a valid LookAtTarget.");
			}
		}
		else
		{
			LookAtTarget = E_LookAtTarget.None;
		}
		XAttribute xAttribute3 = obj.Attribute("MoveCamera");
		if (xAttribute3 != null)
		{
			if (Enum.TryParse<E_MoveCamera>(xAttribute3.Value, out var result3))
			{
				MoveCamera = result3;
			}
			else
			{
				CLoggerManager.Log("Could not parse " + xAttribute3.Value + " as a valid E_MoveCamera.");
			}
		}
		else
		{
			MoveCamera = E_MoveCamera.Never;
		}
	}
}
