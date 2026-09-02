using TPLib;
using TheLastStand.Manager.DLC;
using UnityEngine;

namespace TheLastStand.Definition.DLC;

public class DLCTextAssetDefinition
{
	[field: SerializeField]
	public DLCDefinition DLCDefinition { get; private set; }

	[field: SerializeField]
	public TextAsset TextAsset { get; private set; }

	public bool IsLinkedToDLC => DLCDefinition != null;

	public bool IsDLCOwned()
	{
		if (!TPSingleton<DLCManager>.Exist())
		{
			return false;
		}
		return TPSingleton<DLCManager>.Instance.IsDLCOwned(DLCDefinition.Id);
	}
}
