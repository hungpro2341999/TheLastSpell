using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> Arguments { get; private set; } = new List<string>();

	public int ConditionsGroupIndex { get; }

	public bool Hidden { get; }

	public string LocalizationKey { get; private set; }

	public string Name { get; private set; }

	public int Occurences { get; private set; } = 1;

	public MetaConditionDefinition(XContainer container, bool hidden, int conditionsGroupIndex)
		: base(container)
	{
		ConditionsGroupIndex = conditionsGroupIndex;
		Hidden = hidden;
	}

	public override void Deserialize(XContainer container)
	{
		XElement metaConditionElement = container as XElement;
		Name = metaConditionElement.Name.LocalName;
		if (!TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary.ContainsKey(Name))
		{
			throw new Exception("Invalid condition function " + Name + " - Please pick a function in the following list: " + string.Join(", ", TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary.Keys));
		}
		XAttribute xAttribute = metaConditionElement.Attribute("Occurences");
		if (xAttribute != null)
		{
			if (int.TryParse(xAttribute.Value, out var result))
			{
				Occurences = result;
			}
			else
			{
				CLoggerManager.Log("Occurences attribute has an invalid value " + xAttribute.Value + ". Setting it to 1.", LogType.Error);
			}
		}
		LocalizationKey = metaConditionElement.Attribute("LocalizationKey")?.Value;
		Arguments = (from index in Enumerable.Range(65, 26)
			select metaConditionElement.Attribute(((char)index).ToString())?.Value.Trim() into value
			where value != null
			select value).ToList();
	}

	public override string ToString()
	{
		return string.Format("{0} ({1}) (Occurences={2})", Name, string.Join(", ", Arguments), Occurences);
	}
}
