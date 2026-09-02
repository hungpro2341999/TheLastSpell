using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeIncreaseMagicCircleHealthDefinition : EasyModeModifierDefinition
{
	public const string Name = "IncreaseMagicCircleHealth";

	public override string LocalizedModifier => Localizer.Format("EasyMode_Modifier_IncreaseMagicCircleHealth", Value);

	public int Value { get; private set; }

	public EasyModeIncreaseMagicCircleHealthDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (!int.TryParse(xElement.Value, out var result))
		{
			CLoggerManager.Log("Could not parse " + xElement.Value + " to an integer value!", LogType.Error);
		}
		else
		{
			Value = result;
		}
	}
}
