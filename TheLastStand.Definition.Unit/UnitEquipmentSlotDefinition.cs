using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitEquipmentSlotDefinition : TheLastStand.Framework.Serialization.Definition
{
	public ItemSlotDefinition.E_ItemSlotId Id { get; set; }

	public int Min { get; set; }

	public int Max { get; set; }

	public int Base { get; set; }

	public UnitEquipmentSlotDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("An UnitEquipmentSlotDefinition has an invalid Id " + xAttribute.Value + "!", LogType.Error);
		}
		Id = result;
		XAttribute xAttribute2 = xElement.Attribute("Min");
		int result2;
		if (xAttribute2.IsNullOrEmpty())
		{
			Debug.LogError("UniEquipmentSlotDefinition must have a Min");
		}
		else if (int.TryParse(xAttribute2.Value, out result2))
		{
			Min = result2;
			XAttribute xAttribute3 = xElement.Attribute("Max");
			int result3;
			if (xAttribute3.IsNullOrEmpty())
			{
				Debug.LogError("UniEquipmentSlotDefinition must have a Max");
			}
			else if (int.TryParse(xAttribute3.Value, out result3))
			{
				Max = result3;
				XAttribute xAttribute4 = xElement.Attribute("Base");
				int result4;
				if (xAttribute4.IsNullOrEmpty())
				{
					Debug.LogError("UniEquipmentSlotDefinition must have a Base");
				}
				else if (int.TryParse(xAttribute4.Value, out result4))
				{
					Base = result4;
				}
				else
				{
					Debug.LogError("UniEquipmentSlotDefinition Base must have a valid integer");
				}
			}
			else
			{
				Debug.LogError("UniEquipmentSlotDefinition Max must have a valid integer");
			}
		}
		else
		{
			Debug.LogError("UniEquipmentSlotDefinition Min must have a valid integer");
		}
	}
}
