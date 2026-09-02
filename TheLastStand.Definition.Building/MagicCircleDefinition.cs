using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class MagicCircleDefinition : BuildingDefinition
{
	public int MageCountInit { get; set; }

	public int MageSlotInit { get; set; }

	public int MageSlotMax { get; set; }

	public int OpenSealsInit { get; set; }

	public int SealsToClose { get; set; }

	public MagicCircleDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("MagicCircleSettings");
		if (xElement == null)
		{
			return;
		}
		XElement xElement2 = xElement.Element("MageSlotMax");
		if (xElement2.IsNullOrEmpty())
		{
			Debug.Log("ConstructionDefinition must have a MageSlotMax");
			return;
		}
		if (!int.TryParse(xElement2.Value, out var result))
		{
			Debug.Log("MagicCircle MageSlotMax must be a valid int");
			return;
		}
		MageSlotMax = result;
		XElement xElement3 = xElement.Element("MageSlotInit");
		if (xElement3.IsNullOrEmpty())
		{
			Debug.Log("ConstructionDefinition must have a MageSlotInit");
			return;
		}
		if (!int.TryParse(xElement3.Value, out var result2))
		{
			Debug.Log("MagicCircle MageSlotInit must be a valid int");
			return;
		}
		MageSlotInit = result2;
		XElement xElement4 = xElement.Element("MageCountInit");
		if (xElement4.IsNullOrEmpty())
		{
			Debug.Log("MagicCircle must have a MageCountInit");
			return;
		}
		if (!int.TryParse(xElement4.Value, out var result3))
		{
			Debug.Log("MagicCircle MageCountInit must be a valid int");
			return;
		}
		MageCountInit = result3;
		XElement xElement5 = xElement.Element("OpenSealsInit");
		if (xElement5.IsNullOrEmpty())
		{
			Debug.Log("MagicCircle must have a OpenSealsInit");
			return;
		}
		if (!int.TryParse(xElement5.Value, out var result4))
		{
			Debug.Log("MagicCircle OpenSealsInit must be a valid int");
			return;
		}
		OpenSealsInit = result4;
		XElement xElement6 = xElement.Element("SealsToClose");
		int result5;
		if (xElement6.IsNullOrEmpty())
		{
			Debug.Log("ConstructionDefinition must have a SealsToClose");
		}
		else if (!int.TryParse(xElement6.Value, out result5))
		{
			Debug.Log("MagicCircle SealsToClose must be a valid int");
		}
		else
		{
			SealsToClose = result5;
		}
	}
}
