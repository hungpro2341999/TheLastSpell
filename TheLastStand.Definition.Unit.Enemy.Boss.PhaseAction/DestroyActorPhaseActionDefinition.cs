using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class DestroyActorPhaseActionDefinition : ABossPhaseActionDefinition
{
	public string ActorId { get; private set; }

	public int Amount { get; private set; }

	public bool CameraFocus { get; private set; }

	public bool WaitDeathAnim { get; private set; }

	public DestroyActorPhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		ActorId = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("Amount");
		if (xAttribute2 != null)
		{
			Amount = int.Parse(xAttribute2.Value);
		}
		XAttribute xAttribute3 = obj.Attribute("CameraFocus");
		if (xAttribute3 != null)
		{
			if (bool.TryParse(xAttribute3.Value, out var result))
			{
				CameraFocus = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute3.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		XAttribute xAttribute4 = obj.Attribute("WaitDeathAnim");
		if (xAttribute4 != null)
		{
			if (bool.TryParse(xAttribute4.Value, out var result2))
			{
				WaitDeathAnim = result2;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute4.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
