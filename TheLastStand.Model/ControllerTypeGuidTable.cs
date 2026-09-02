using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheLastStand.Model;

[CreateAssetMenu(fileName = "New Controller Type Guid Table", menuName = "TLS/Joystick Config/Controller Type Guid Table", order = 100)]
public class ControllerTypeGuidTable : SerializedScriptableObject
{
	[Serializable]
	private struct ControllerTypeGuids
	{
		[SerializeField]
		public ControllerType ControllerType;

		[SerializeField]
		public string[] Guids;
	}

	[SerializeField]
	private ControllerTypeGuids[] controllerTypeByHardwareGuid;

	public ControllerType GetControllerType(Guid controllerGuid)
	{
		if (!TryGetControllerType(controllerGuid, out var controllerType))
		{
			return ControllerType.Default;
		}
		return controllerType;
	}

	public bool TryGetControllerType(Guid controllerGuid, out ControllerType controllerType)
	{
		for (int i = 0; i < controllerTypeByHardwareGuid.Length; i++)
		{
			ControllerTypeGuids controllerTypeGuids = controllerTypeByHardwareGuid[i];
			string[] guids = controllerTypeGuids.Guids;
			for (int j = 0; j < guids.Length; j++)
			{
				if (guids[j] == controllerGuid.ToString())
				{
					controllerType = controllerTypeGuids.ControllerType;
					return true;
				}
			}
		}
		controllerType = ControllerType.Default;
		return false;
	}
}
