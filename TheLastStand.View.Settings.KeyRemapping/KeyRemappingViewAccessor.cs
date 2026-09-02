using TPLib;
using UnityEngine;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingViewAccessor : TPSingleton<KeyRemappingViewAccessor>
{
	[SerializeField]
	private KeyRemappingView keyRemappingView;

	public static KeyRemappingView KeyRemappingView => TPSingleton<KeyRemappingViewAccessor>.Instance.keyRemappingView;
}
