using System;
using DG.Tweening;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Cursor;

public class JoystickCursorView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private Tween hideTween;

	private Tween scaleTween;

	public bool Enabled => base.gameObject.activeSelf;

	public void Enable(bool isOn)
	{
		if (isOn != Enabled)
		{
			base.gameObject.SetActive(isOn);
		}
	}

	public void Show(bool show)
	{
		if ((!show || !(spriteRenderer.color.a >= 1f)) && (show || spriteRenderer.color.a != 0f))
		{
			if (show)
			{
				hideTween?.Kill();
				hideTween = null;
				spriteRenderer.color = spriteRenderer.color.WithA(1f);
			}
			else if (hideTween == null && spriteRenderer.color.a > 0f)
			{
				hideTween = spriteRenderer.DOFade(0f, InputManager.JoystickConfig.Cursor.HideDuration);
			}
		}
	}

	public void SetPosition(Vector3 position)
	{
		base.transform.position = position;
	}

	private void Awake()
	{
		ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Combine(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoomHasChanged));
	}

	private void OnDestroy()
	{
		ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Remove(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoomHasChanged));
	}

	private void OnZoomHasChanged(bool zoomed)
	{
		scaleTween?.Kill();
		scaleTween = spriteRenderer.transform.DOScale(Vector3.one * (zoomed ? 0.5f : 1f), InputManager.JoystickConfig.Cursor.ScaleDuration);
	}
}
