using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class AfterXProductionPhasesTriggerDefinition : PassiveTriggerDefinition
{
	public int NumberOfProductionPhases { get; private set; }

	public AfterXProductionPhasesTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnStartProductionTurn;
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("NumberOfProductionPhases");
		if (xAttribute == null || !int.TryParse(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("The NumberOfProductionPhases attribute (in AfterXProductionPhasesTrigger) is missing or could not be parsed into an int.", LogType.Error, CLogLevel.MAJOR);
		}
		else
		{
			NumberOfProductionPhases = result;
		}
	}
}
