using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class ResourceDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public static class Ids
		{
			public const string Gold = "Gold";

			public const string Materials = "Materials";

			public const string Workers = "Workers";
		}
	}

	public int Gold { get; private set; }

	public string Id { get; private set; }

	public int Materials { get; private set; }

	public ResourceDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		ResourceDefinition resourceDefinition = null;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("A ResourceDefinition must have an Id attribute.", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			resourceDefinition = ResourceDatabase.ResourceDefinitions[xAttribute2.Value];
		}
		XElement xElement2 = xElement.Element("Gold");
		if (xElement2.IsNullOrEmpty())
		{
			if (resourceDefinition == null)
			{
				CLoggerManager.Log("ResourceDefinition " + Id + " has no Gold and no Template to copy it from!", LogType.Error);
				return;
			}
			Gold = resourceDefinition.Gold;
		}
		else
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse Gold element value " + xElement2.Value + " to a valid int value.", LogType.Error);
				return;
			}
			Gold = result;
		}
		XElement xElement3 = xElement.Element("Materials");
		int result2;
		if (xElement3.IsNullOrEmpty())
		{
			if (resourceDefinition == null)
			{
				CLoggerManager.Log("ResourceDefinition " + Id + " has no Materials and no Template to copy it from!", LogType.Error);
			}
			else
			{
				Materials = resourceDefinition.Materials;
			}
		}
		else if (!int.TryParse(xElement3.Value, out result2))
		{
			CLoggerManager.Log("Could not parse Materials element value " + xElement3.Value + " to a valid int value.", LogType.Error);
		}
		else
		{
			Materials = result2;
		}
	}
}
