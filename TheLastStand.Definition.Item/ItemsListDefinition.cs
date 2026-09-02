using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.DLC;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string DLCId { get; private set; }

	public string Id { get; private set; }

	public bool IsEmpty => ItemsWithOdd.Count == 0;

	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	public Dictionary<string, int> ItemsWithOdd { get; } = new Dictionary<string, int>();

	public ItemsListDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("xItemCategoriesListDefinition must have a valid Id", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("DLCId");
		if (!xAttribute2.IsNullOrEmpty())
		{
			DLCId = xAttribute2.Value;
		}
		if (IsLinkedToDLC && !TPSingleton<DLCManager>.Instance.IsDLCOwned(DLCId))
		{
			return;
		}
		foreach (XElement item in xElement.Elements("Item"))
		{
			XAttribute xAttribute3 = item.Attribute("Odd");
			if (xAttribute3.IsNullOrEmpty() || !int.TryParse(xAttribute3.Value, out var result))
			{
				CLoggerManager.Log(Id + " Invalid odd!", LogType.Error);
				continue;
			}
			XAttribute xAttribute4 = item.Attribute("Id");
			if (xAttribute4.IsNullOrEmpty())
			{
				CLoggerManager.Log(Id + " Invalid category!", LogType.Error);
			}
			else
			{
				ItemsWithOdd.Add(xAttribute4.Value, result);
			}
		}
	}
}
