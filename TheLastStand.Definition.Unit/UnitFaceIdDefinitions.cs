using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit;

public class UnitFaceIdDefinitions : List<UnitFaceIdDefinition>
{
	private List<string> ids = new List<string>();

	public UnitFaceIdDefinitions(XDocument idsDocument)
	{
		foreach (XElement item in idsDocument.Element("UnitFaceIdsDefinition").Elements("UnitFaceId"))
		{
			UnitFaceIdDefinition unitFaceIdDefinition = new UnitFaceIdDefinition(item);
			if (!unitFaceIdDefinition.IsLinkedToDLC || unitFaceIdDefinition.IsLinkedDLCOwned)
			{
				Add(unitFaceIdDefinition);
			}
		}
	}

	public List<string> GetWeightedFaceIds()
	{
		if (ids != null && ids.Count > 0)
		{
			return ids;
		}
		for (int i = 0; i < base.Count; i++)
		{
			for (int j = 0; j < base[i].Weight; j++)
			{
				ids.Add(base[i].FaceId);
			}
		}
		return ids;
	}

	public List<string> ToStringList()
	{
		if (ids != null && ids.Count > 0)
		{
			return ids;
		}
		for (int i = 0; i < base.Count; i++)
		{
			ids.Add(base[i].FaceId);
		}
		return ids;
	}
}
