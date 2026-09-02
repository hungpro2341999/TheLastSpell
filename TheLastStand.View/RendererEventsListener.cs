using System;
using UnityEngine;

namespace TheLastStand.View;

public class RendererEventsListener : MonoBehaviour
{
	public event Action<bool> OnSpriteVisibilityToggle;

	private void OnBecameInvisible()
	{
		this.OnSpriteVisibilityToggle?.Invoke(obj: false);
	}

	private void OnBecameVisible()
	{
		this.OnSpriteVisibilityToggle?.Invoke(obj: true);
	}
}
