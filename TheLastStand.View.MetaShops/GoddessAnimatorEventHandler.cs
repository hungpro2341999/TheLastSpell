using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.MetaShops;

public class GoddessAnimatorEventHandler : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onGoddessAppear;

	public bool AppearFrame { get; set; }

	public void OnGoddessAppearFrame()
	{
		AppearFrame = true;
		onGoddessAppear.Invoke();
	}
}
