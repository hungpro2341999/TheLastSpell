using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SpawnActorPhaseActionDefinition : ASpawnActorPhaseActionDefinition
{
	public int AmountToSpawn { get; private set; } = 1;

	public bool PrioritizeWaveSide { get; private set; }

	public SpawnActorPhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Amount");
		if (xAttribute != null && xAttribute.Value != null)
		{
			if (int.TryParse(xAttribute.Value, out var result) && result > 0)
			{
				AmountToSpawn = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute.Value + " into int or value <= 0", LogType.Error, CLogLevel.MAJOR);
			}
		}
		XAttribute xAttribute2 = obj.Attribute("PrioritizeWaveSide");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result2))
			{
				PrioritizeWaveSide = result2;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		if (base.CameraFocus && !unitCreationSettingsTemplate.WaitSpawnAnim)
		{
			CLoggerManager.Log(string.Join(",", base.ActorsIds) + " SpawnActor: CameraFocus was set to true, but WaitSpawnAnim is set to false. Are you sure that is intended ? -> weird behavior INC.", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
