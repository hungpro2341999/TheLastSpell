using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ModifyBuildingsDeadZoneRangeApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public List<string> BuildingsIds { get; private set; }

	public int Range { get; private set; }

	public ModifyBuildingsDeadZoneRangeApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Range");
		Range = Parser.Parse(xAttribute.Value, base.TokenVariables).EvalToInt();
		BuildingsIds = new List<string>();
		foreach (XElement item in xElement.Elements("BuildingList"))
		{
			string key = item.Value.Replace(base.TokenVariables);
			foreach (string id in GenericDatabase.IdsListDefinitions[key].Ids)
			{
				AddBuildingId(id);
			}
		}
		foreach (XElement item2 in xElement.Elements("BuildingId"))
		{
			AddBuildingId(item2.Value.Replace(base.TokenVariables));
		}
	}

	private void AddBuildingId(string id)
	{
		if (!BuildingsIds.Contains(id))
		{
			BuildingsIds.Add(id);
		}
	}
}
