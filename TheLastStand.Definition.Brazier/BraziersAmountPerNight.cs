using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Brazier;

public class BraziersAmountPerNight : NightIndexedItem
{
	private static class Constants
	{
		public const string BraziersAmountElement = "BraziersAmount";

		public const string AmountAttribute = "Amount";
	}

	public int Amount;

	public override void Init(int nightIndex, XElement xElement)
	{
		base.Init(nightIndex, xElement);
		XAttribute xAttribute = xElement.Attribute("Amount");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Amount attribute could not be parsed into an int (" + xAttribute.Value + "). Skipped.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "BrazierDefinition");
		}
		else
		{
			Amount = result;
		}
	}
}
