using System;
using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Brazier;

public class GuardiansGroupsToSpawnPerNight : NightIndexedItem
{
	public GuardiansGroupsToSpawn GuardiansGroupsToSpawn;

	public override void Init(int nightIndex, XElement xElement)
	{
		base.Init(nightIndex, xElement);
		GuardiansGroupsToSpawn = new GuardiansGroupsToSpawn();
		foreach (XElement item2 in xElement.Elements("GuardiansGroupToSpawn"))
		{
			XAttribute xAttribute = item2.Attribute("Id");
			if (!BraziersDefinition.GuardiansGroups.TryGetValue(xAttribute.Value, out var value))
			{
				CLoggerManager.Log("Guardians group " + xAttribute.Value + " could not be found in the database.");
				continue;
			}
			XAttribute xAttribute2 = item2.Attribute("Weight");
			int item = ((xAttribute2 == null) ? 1 : int.Parse(xAttribute2.Value));
			GuardiansGroupsToSpawn.Add(new Tuple<BraziersDefinition.GuardiansGroup, int>(value, item));
		}
	}
}
