using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.DLC;

namespace TheLastStand.Definition.Unit;

public class UnitFaceIdDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string DLCId { get; private set; }

	public string FaceId { get; private set; }

	public bool IsLinkedDLCOwned
	{
		get
		{
			if (IsLinkedToDLC)
			{
				return TPSingleton<DLCManager>.Instance.IsDLCOwned(DLCId);
			}
			return false;
		}
	}

	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	public string RestrictedToRaceId { get; private set; }

	public int Weight { get; private set; }

	public UnitFaceIdDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("DLCId");
		if (xAttribute != null && !string.IsNullOrEmpty(xAttribute.Value))
		{
			DLCId = xAttribute.Value;
		}
		XAttribute xAttribute2 = xElement.Attribute("RestrictedToRaceId");
		if (xAttribute2 != null && !string.IsNullOrEmpty(xAttribute2.Value))
		{
			RestrictedToRaceId = xAttribute2.Value;
		}
		else
		{
			RestrictedToRaceId = "Human";
		}
		XAttribute xAttribute3 = xElement.Attribute("Weight");
		if (xAttribute3 != null && int.TryParse(xAttribute3.Value, out var result))
		{
			Weight = result;
		}
		FaceId = xElement.Value;
	}
}
