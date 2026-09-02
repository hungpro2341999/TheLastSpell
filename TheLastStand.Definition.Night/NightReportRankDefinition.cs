using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Night;

public class NightReportRankDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public float MaxHPsLostRatio { get; set; }

	public NightReportRankDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("NightReportDefinition Id is null or empty!", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("MaxHPsLostRatio");
		float result;
		if (xElement2.IsNullOrEmpty())
		{
			CLoggerManager.Log("NightReportDefinition with Id " + Id + " must have a MaxHPsLostRatio!", LogType.Error);
		}
		else if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			CLoggerManager.Log("NightReportRankDefinition with Id " + Id + " MaxHPsLostRatio " + xElement2.Value + " must be a valid float value!", LogType.Error);
		}
		else
		{
			MaxHPsLostRatio = result;
		}
	}
}
