using Sirenix.OdinInspector;
using TPLib.Log;

namespace TheLastStand.Manager;

public abstract class Manager<T> : CLogger<T> where T : SerializedMonoBehaviour
{
	protected override void Awake()
	{
		base.Awake();
	}
}
