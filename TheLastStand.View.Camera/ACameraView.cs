using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.View.Camera.ProCam2D;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

public abstract class ACameraView : Manager<ACameraView>
{
	public delegate void DelZoom(bool zoomed);

	public static DelZoom OnZoomHasChanged;

	private static Tweener moveTweener;

	[SerializeField]
	private Vector3 startPos = new Vector3(0f, 39f, -10f);

	[SerializeField]
	private bool allowUserZoom = true;

	[SerializeField]
	private bool startZoomedIn;

	[SerializeField]
	private float scrollWheelSensitivity = 2f;

	[SerializeField]
	private float scrollWheelTimeframe = 1f;

	[SerializeField]
	private Ease zoomEasing = Ease.InOutQuart;

	[SerializeField]
	private float zoomDuration = 0.3f;

	[SerializeField]
	protected UnityEngine.Camera mainCam;

	[SerializeField]
	private PixelPerfectCamera pixelPerfectCam;

	private ProCamera2DShake camShake;

	private bool isZoomedIn;

	private bool lastZoom = true;

	private float lastZoomInputTime;

	protected int pixelPerfectCamOriginalPpu;

	private Coroutine zoomCoroutine;

	private Tweener zoomTweener;

	private float zoomInputCumulativeValue;

	[SerializeField]
	private bool debugStopShakesBeforeStartingNewOne;

	public ProCamera2DExtensionPan CamPanner { get; private set; }

	public static bool AllowUserPan
	{
		get
		{
			if (TPSingleton<ACameraView>.Instance.CamPanner != null && TPSingleton<ACameraView>.Instance.CamPanner.enabled)
			{
				return TPSingleton<ACameraView>.Instance.CamPanner.AllowPan;
			}
			return false;
		}
		set
		{
			if (TPSingleton<ACameraView>.Instance.CamPanner != null)
			{
				TPSingleton<ACameraView>.Instance.CamPanner.AllowPan = value;
			}
		}
	}

	public static bool AllowUserZoom
	{
		get
		{
			return TPSingleton<ACameraView>.Instance.allowUserZoom;
		}
		set
		{
			TPSingleton<ACameraView>.Instance.allowUserZoom = value;
		}
	}

	public static bool IsZoomedIn => TPSingleton<ACameraView>.Instance.isZoomedIn;

	public static bool IsZooming => TPSingleton<ACameraView>.Instance.zoomCoroutine != null;

	public static UnityEngine.Camera MainCam => TPSingleton<ACameraView>.Instance.mainCam;

	public static void MoveTo(Vector3 targetPosition, float time = 0f, Ease ease = Ease.Unset)
	{
		Vector3 previousTargetPosition = ProCamera2D.Instance.CameraTargets[0].TargetTransform.position;
		if (moveTweener != null)
		{
			moveTweener.Kill();
		}
		moveTweener = DOTween.To(() => previousTargetPosition, delegate(Vector3 x)
		{
			previousTargetPosition = x;
			ProCamera2D.Instance.CameraTargets[0].TargetTransform.position = x;
		}, targetPosition, time).SetEase(ease);
	}

	public static void MoveTo(Transform targetTransform)
	{
		MoveTo(targetTransform.position);
	}

	public static void Shake(string shakeId = "CamShake - Attack", float delay = 0f)
	{
		if (!TPSingleton<SettingsManager>.Exist() || TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue != 0f)
		{
			TPSingleton<ACameraView>.Instance.StartCoroutine(TPSingleton<ACameraView>.Instance.ShakeCoroutine(shakeId, delay));
		}
	}

	public static void Shake(float duration, Vector2 strength, int vibrato, float randomness, float initialAngle, Vector3 rotation, float smoothness, float delay = 0f)
	{
		if (!TPSingleton<SettingsManager>.Exist() || TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue != 0f)
		{
			TPSingleton<ACameraView>.Instance.StartCoroutine(TPSingleton<ACameraView>.Instance.ShakeCoroutine(duration, strength, vibrato, randomness, initialAngle, rotation, smoothness, delay));
		}
	}

	public static void StopShaking()
	{
		if (TPSingleton<ACameraView>.Instance.camShake == null)
		{
			TPSingleton<ACameraView>.Instance.InitShake();
		}
		TPSingleton<ACameraView>.Instance.camShake.StopShaking();
	}

	public static Coroutine Zoom(bool zoomIn, bool instant = false)
	{
		if (TPSingleton<ACameraView>.Instance.zoomCoroutine != null)
		{
			return TPSingleton<ACameraView>.Instance.zoomCoroutine;
		}
		if (zoomIn == IsZoomedIn)
		{
			return null;
		}
		OnZoomHasChanged?.Invoke(zoomIn);
		TPSingleton<ACameraView>.Instance.zoomCoroutine = TPSingleton<ACameraView>.Instance.StartCoroutine(TPSingleton<ACameraView>.Instance.ZoomCoroutine(zoomIn, instant));
		return TPSingleton<ACameraView>.Instance.zoomCoroutine;
	}

	public virtual void Init()
	{
		MainCam.transform.position = startPos;
		if (ProCamera2D.Exists)
		{
			ProCamera2D.Instance.MoveCameraInstantlyToPosition(startPos);
		}
		InitShake();
	}

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChanged;
		pixelPerfectCamOriginalPpu = pixelPerfectCam.assetsPPU;
		zoomTweener = null;
		lastZoomInputTime = -1f;
		zoomInputCumulativeValue = -1f;
		isZoomedIn = !startZoomedIn;
		CamPanner = ProCamera2D.Instance?.GetComponent<ProCamera2DExtensionPan>();
		Init();
	}

	protected virtual int ComputeTargetPixelsPerUnit(bool zoomedIn)
	{
		int num = pixelPerfectCamOriginalPpu;
		if (zoomedIn)
		{
			num *= 2;
		}
		return num;
	}

	private void OnResolutionChanged(Resolution resolution)
	{
		pixelPerfectCam.assetsPPU = ComputeTargetPixelsPerUnit(isZoomedIn);
	}

	private void InitShake()
	{
		if (ProCamera2D.Exists)
		{
			camShake = ProCamera2D.Instance.GetComponent<ProCamera2DShake>();
			ProCamera2DShake proCamera2DShake = camShake;
			proCamera2DShake.OnShakeCompleted = (Action)Delegate.Combine(proCamera2DShake.OnShakeCompleted, (Action)delegate
			{
				Log($"[{Time.time}] Shake completed", this, CLogLevel.DETAILED);
			});
		}
	}

	protected void Start()
	{
		if (CamPanner != null)
		{
			CamPanner.UsePanByMoveToEdges = TPSingleton<SettingsManager>.Instance.Settings.EdgePan;
			CamPanner.IgnoreUIOnEdges = TPSingleton<SettingsManager>.Instance.Settings.EdgePanOverUI;
		}
		Zoom(startZoomedIn, instant: true);
	}

	protected virtual void Update()
	{
		if (!allowUserZoom)
		{
			return;
		}
		float axis = InputManager.GetAxis(14);
		bool buttonDown = InputManager.GetButtonDown(14);
		if (Mathf.Abs(axis) > 0f)
		{
			if (zoomInputCumulativeValue != 0f && Mathf.Sign(axis) != Mathf.Sign(zoomInputCumulativeValue))
			{
				ResetZoomTimeFrame();
				Log("Zoom Reset: input direction changed", this, CLogLevel.DETAILED);
			}
			if (lastZoomInputTime < 0f)
			{
				lastZoomInputTime = Time.time;
			}
			zoomInputCumulativeValue += axis;
			float num = Mathf.Abs(zoomInputCumulativeValue);
			if (num > 0f)
			{
				Log($"Cumulative zoom value: {zoomInputCumulativeValue}", this, CLogLevel.DETAILED);
			}
			if (num >= scrollWheelSensitivity)
			{
				lastZoom = axis > 0f;
				Zoom(axis > 0f);
				ResetZoomTimeFrame();
				Log("Zoom Triggered", this, CLogLevel.DETAILED);
			}
		}
		else if (buttonDown)
		{
			if (lastZoom)
			{
				lastZoom = false;
				Zoom(zoomIn: false);
			}
			else
			{
				lastZoom = true;
				Zoom(zoomIn: true);
			}
		}
		if (lastZoomInputTime > 0f && lastZoomInputTime + scrollWheelTimeframe <= Time.time)
		{
			ResetZoomTimeFrame();
			Log("Zoom Timeout", this, CLogLevel.DETAILED);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChanged;
		}
	}

	private void ResetZoomTimeFrame()
	{
		lastZoomInputTime = -1f;
		zoomInputCumulativeValue = 0f;
	}

	private IEnumerator ShakeCoroutine(string shakeId, float delay)
	{
		if (camShake == null)
		{
			InitShake();
		}
		Log($"[{Time.time}] Starting shake with id '{shakeId}'", this, CLogLevel.DETAILED);
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (debugStopShakesBeforeStartingNewOne)
		{
			camShake.StopShaking();
		}
		Log($"[{Time.time}] Playing shake with id '{shakeId}'", this, CLogLevel.DETAILED);
		ShakePreset shakePreset = null;
		foreach (ShakePreset shakePreset2 in camShake.ShakePresets)
		{
			if (shakePreset2.name == shakeId)
			{
				shakePreset = ScriptableObject.CreateInstance<ShakePreset>();
				shakePreset.Duration = shakePreset2.Duration;
				shakePreset.Strength = shakePreset2.Strength;
				shakePreset.Vibrato = shakePreset2.Vibrato;
				shakePreset.Randomness = shakePreset2.Randomness;
				shakePreset.UseRandomInitialAngle = shakePreset2.UseRandomInitialAngle;
				shakePreset.Rotation = shakePreset2.Rotation;
				shakePreset.Smoothness = shakePreset2.Smoothness;
				shakePreset.IgnoreTimeScale = shakePreset2.IgnoreTimeScale;
				break;
			}
		}
		if (shakePreset != null)
		{
			if (TPSingleton<SettingsManager>.Exist())
			{
				shakePreset.Strength *= TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue;
			}
			camShake.Shake(shakePreset);
		}
		else
		{
			LogWarning("Shake preset with id " + shakeId + " hasn't been found. Skipping shake.");
		}
	}

	private IEnumerator ShakeCoroutine(float duration, Vector2 strength, int vibrato, float randomness, float initialAngle, Vector3 rotation, float smoothness, float delay = 0f)
	{
		if (camShake == null)
		{
			InitShake();
		}
		Log($"[{Time.time}] Starting custom shake'", this, CLogLevel.DETAILED);
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (debugStopShakesBeforeStartingNewOne)
		{
			camShake.StopShaking();
		}
		Log($"[{Time.time}] Playing custom shake", this, CLogLevel.DETAILED);
		if (TPSingleton<SettingsManager>.Exist())
		{
			strength *= TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue;
		}
		camShake.Shake(duration, strength, vibrato, randomness, initialAngle, rotation, smoothness);
	}

	private IEnumerator ZoomCoroutine(bool zoomIn, bool instant = false)
	{
		if (zoomTweener != null && zoomTweener.IsPlaying())
		{
			yield return zoomTweener.WaitForCompletion();
			yield break;
		}
		int num = ComputeTargetPixelsPerUnit(zoomIn);
		if (instant)
		{
			pixelPerfectCam.assetsPPU = num;
			isZoomedIn = zoomIn;
			yield break;
		}
		zoomTweener = DOTween.To(() => pixelPerfectCam.assetsPPU, delegate(int v)
		{
			pixelPerfectCam.assetsPPU = v;
		}, num, zoomDuration * (TPSingleton<SettingsManager>.Instance.IsTimeScaleAccelerated ? TPSingleton<SettingsManager>.Instance.Settings.SpeedScale : 1f)).SetId("CameraZoom").SetEase(zoomEasing)
			.OnKill(delegate
			{
				zoomTweener = null;
			});
		yield return zoomTweener.WaitForCompletion();
		isZoomedIn = zoomIn;
		zoomCoroutine = null;
	}

	[ContextMenu("Test Zoom Sequence")]
	private void DebugTestZoomSequence()
	{
		StartCoroutine(DebugTestZoomSequenceCoroutine());
	}

	private IEnumerator DebugTestZoomSequenceCoroutine()
	{
		yield return Zoom(zoomIn: false);
		yield return SharedYields.WaitForSeconds(0.5f);
		yield return Zoom(zoomIn: true, instant: true);
		yield return SharedYields.WaitForSeconds(0.5f);
		yield return Zoom(zoomIn: false);
	}

	[DevConsoleCommand(Name = "CameraMoveTo")]
	public static void Debug_CameraMoveTo(float x, float y)
	{
		MoveTo(new Vector3(x, y));
	}
}
