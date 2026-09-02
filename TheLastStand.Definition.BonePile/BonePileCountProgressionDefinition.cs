using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.BonePile;

public class BonePileCountProgressionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public struct ProgressionData
	{
		public int BaseValue;

		public int Limit;

		public int Delay;

		public int IncreaseEveryXDays;

		public int IncreaseValue;
	}

	public string CityId { get; private set; }

	public string TemplateCityId { get; private set; }

	public bool HasTemplate => !string.IsNullOrEmpty(TemplateCityId);

	public Dictionary<string, ProgressionData> BonePileProgressions { get; private set; } = new Dictionary<string, ProgressionData>();

	public BonePileCountProgressionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("CityId");
		CityId = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("TemplateCityId");
		if (xAttribute2 != null)
		{
			TemplateCityId = xAttribute2.Value;
		}
		foreach (XElement item in obj.Elements("Progression"))
		{
			XAttribute xAttribute3 = item.Attribute("BonePileId");
			if (!int.TryParse(item.Attribute("BaseValue").Value, out var result))
			{
				CLoggerManager.Log("Could not parse BaseValue value to a valid int! (Bone Pile " + xAttribute3.Value + ", CityId " + CityId + ")");
				break;
			}
			XAttribute xAttribute4 = item.Attribute("Limit");
			int result2 = -1;
			if (xAttribute4 != null && !int.TryParse(xAttribute4.Value, out result2))
			{
				CLoggerManager.Log("Could not parse Limit value to a valid int! (Bone Pile " + xAttribute3.Value + ", CityId " + CityId + ")");
				break;
			}
			if (!int.TryParse(item.Attribute("Delay").Value, out var result3))
			{
				CLoggerManager.Log("Could not parse Delay value to a valid int! (Bone Pile " + xAttribute3.Value + ", CityId " + CityId + ")");
				break;
			}
			if (!int.TryParse(item.Attribute("IncreaseEveryXDays").Value, out var result4))
			{
				CLoggerManager.Log("Could not parse IncreaseEveryXDays value to a valid int! (Bone Pile " + xAttribute3.Value + ", CityId " + CityId + ")");
				break;
			}
			if (!int.TryParse(item.Attribute("IncreaseValue").Value, out var result5))
			{
				CLoggerManager.Log("Could not parse IncreaseValue value to a valid int! (Bone Pile " + xAttribute3.Value + ", CityId " + CityId + ")");
				break;
			}
			ProgressionData value = new ProgressionData
			{
				BaseValue = result,
				Limit = result2,
				Delay = result3,
				IncreaseEveryXDays = result4,
				IncreaseValue = result5
			};
			BonePileProgressions.Add(xAttribute3.Value, value);
		}
	}

	public void DeserializeUsingTemplate(BonePileCountProgressionDefinition template)
	{
		foreach (KeyValuePair<string, ProgressionData> bonePileProgression in template.BonePileProgressions)
		{
			if (!BonePileProgressions.ContainsKey(bonePileProgression.Key))
			{
				BonePileProgressions.Add(bonePileProgression.Key, bonePileProgression.Value);
			}
		}
	}
}
