using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition;

public class PerkSinkDataDefinition : SinkDataDefinition
{
	public int PricePerPerk { get; private set; }

	public float RerollBaseChances { get; private set; }

	public PerkSinkDataDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("PricePerPerk");
		if (xElement2.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition " + base.Id + " needs a PricePerPerk.");
			return;
		}
		if (!int.TryParse(xElement2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			TPDebug.LogError("SinkDataDefinition " + base.Id + "'s PricePerPerk " + HasAnInvalidInt(xElement2.Value));
			return;
		}
		PricePerPerk = result;
		XElement xElement3 = xElement.Element("RerollBaseChances");
		float result2;
		if (xElement3.IsNullOrEmpty())
		{
			TPDebug.LogError("SinkDataDefinition " + base.Id + " needs a RerollBaseChances.");
		}
		else if (!float.TryParse(xElement3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result2))
		{
			TPDebug.LogError("SinkDataDefinition " + base.Id + "'s RerollBaseChances " + HasAnInvalidFloat(xElement3.Value));
		}
		else
		{
			RerollBaseChances = result2;
		}
	}
}
