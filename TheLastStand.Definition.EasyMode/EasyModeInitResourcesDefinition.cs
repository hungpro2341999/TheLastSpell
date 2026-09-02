using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeInitResourcesDefinition : EasyModeModifierDefinition
{
	public const string Name = "InitResources";

	public int InitGold { get; private set; }

	public int InitMaterials { get; private set; }

	public override string LocalizedModifier => Localizer.Format("EasyMode_Modifier_InitResources", InitGold, InitMaterials);

	public EasyModeInitResourcesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("Gold");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse " + xElement2.Value + " to an integer value!", LogType.Error);
				return;
			}
			InitGold = result;
		}
		XElement xElement3 = xElement.Element("Materials");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse " + xElement3.Value + " to an integer value!", LogType.Error);
			}
			else
			{
				InitMaterials = result2;
			}
		}
	}
}
