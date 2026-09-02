using System;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Maths;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class GoddessSeal : MonoBehaviour
{
	[SerializeField]
	private RectTransform pivotRectTransform;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private UnityEngine.Camera mainCamera;

	[SerializeField]
	private bool isDarkShop = true;

	[SerializeField]
	private bool clickAllowed = true;

	[SerializeField]
	private bool mouseDetectionEnabled = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float clickMinPercentage = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float percentage = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float percentageMaxSnap = 0.95f;

	[SerializeField]
	private AnimationCurve positionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private Vector2 positionAmplitudeRange = new Vector2(0f, 50f);

	[SerializeField]
	private Vector2 positionFrequencyRange = new Vector2(0f, 20f);

	[SerializeField]
	private Image sealImage;

	[SerializeField]
	private Image glowImage;

	[SerializeField]
	private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private Vector2 alphaRange = new Vector2(0f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float clickableAlpha = 1f;

	[SerializeField]
	[Min(0.1f)]
	private float glowAlphaTweenSpeed = 2f;

	[SerializeField]
	private Image vignette;

	[SerializeField]
	[Range(0f, 1f)]
	private float vignetteMinPercentage = 0.75f;

	[SerializeField]
	[Range(0f, 1f)]
	private float vignetteMaxAlpha = 1f;

	[SerializeField]
	private AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private float vignetteSmoothTime = 0.3f;

	[SerializeField]
	private ParticleSystem circlesParticleSystem;

	[SerializeField]
	private AnimationCurve circlesRateOverTimeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float circlesMinPercentage = 0.75f;

	[SerializeField]
	private Vector2 circlesRateOverTimeRange = new Vector2(1f, 5f);

	[SerializeField]
	private ParticleSystem lightRaysParticleSystem;

	[SerializeField]
	private ParticleSystemRenderer lightRaysParticleSystemRenderer;

	[SerializeField]
	private Vector2 lightRaysLengthScaleRange = new Vector2(6f, 10f);

	[SerializeField]
	private Vector2 lightRaysRateOverTimeRange = new Vector2(25f, 50f);

	[SerializeField]
	private AnimationCurve lightRaysCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private ParticleSystem dustParticleSystem;

	[SerializeField]
	private Vector2 dustRateOverTimeRange = new Vector2(25f, 50f);

	[SerializeField]
	private Vector2 dustStartSpeedRange = new Vector2(5f, 15f);

	[SerializeField]
	private AnimationCurve dustCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private ParticleSystem smokeParticleSystem;

	[SerializeField]
	private Vector2 smokeSpeedRange = new Vector2(1f, 3f);

	[SerializeField]
	private AudioSource hoverAudioSource;

	[SerializeField]
	[Min(0.1f)]
	private float joystickDisabledVolumeDecreaseSpeed = 2f;

	private float currentPositionFrequency;

	private float positionPhase;

	private float refVignetteAlpha;

	public bool DisablePercentage => true;

	private void Awake()
	{
		currentPositionFrequency = positionFrequencyRange.Lerp(0f, positionCurve);
		glowImage.color = glowImage.color.WithA(0f);
	}

	private void Start()
	{
		if ((object)mainCamera == null)
		{
			mainCamera = ACameraView.MainCam;
		}
	}

	private void ComputeNewPositionFrequency()
	{
		float num = positionFrequencyRange.Lerp(percentage, positionCurve);
		float num2 = (Time.time * currentPositionFrequency + positionPhase) % ((float)Math.PI * 2f);
		float num3 = Time.time * num % ((float)Math.PI * 2f);
		positionPhase = num2 - num3;
		currentPositionFrequency = num;
	}

	private void Update()
	{
		if (mouseDetectionEnabled && (!InputManager.IsLastControllerJoystick || TPSingleton<OraculumView>.Instance.CursorView.Enabled))
		{
			Vector3 a = mainCamera.ScreenToWorldPoint(InputManager.IsLastControllerJoystick ? TPSingleton<OraculumView>.Instance.CursorView.transform.position : Input.mousePosition);
			Vector3 b = mainCamera.ScreenToWorldPoint(pivotRectTransform.position);
			float num = Vector3.Distance(a, b);
			float num2 = 1080f / (float)Screen.height;
			float num3 = mainCamera.orthographicSize * num2;
			float num4 = num * num2;
			percentage = Mathf.Clamp01(1f - num4 / num3);
			if (percentage > percentageMaxSnap)
			{
				percentage = 1f;
			}
		}
		else
		{
			percentage = (InputManager.IsLastControllerJoystick ? Mathf.Max(0f, percentage - Time.unscaledDeltaTime * joystickDisabledVolumeDecreaseSpeed) : 0f);
		}
		int num5;
		if (clickAllowed)
		{
			num5 = ((percentage > clickMinPercentage) ? 1 : 0);
			if (num5 != 0 && InputManager.GetSubmitButtonDown() && !TPSingleton<OraculumView>.Instance.IsInAnyShop && !TPSingleton<OraculumView>.Instance.OpeningOrClosing)
			{
				TPSingleton<OraculumView>.Instance.StartCoroutine(TPSingleton<OraculumView>.Instance.TransitionToShop(isDarkShop));
			}
		}
		else
		{
			num5 = 0;
		}
		if (Mathf.Abs(positionFrequencyRange.Lerp(percentage, positionCurve) - currentPositionFrequency) > 0.01f)
		{
			ComputeNewPositionFrequency();
		}
		Vector3 position = pivotRectTransform.position;
		position.y += Mathf.Sin(Time.time * currentPositionFrequency + positionPhase) * positionAmplitudeRange.Lerp(percentage, positionCurve);
		rectTransform.position = position;
		Color color = glowImage.color;
		float num6 = ((num5 != 0) ? 1f : 0f);
		if (color.a < num6)
		{
			float value = color.a + Time.deltaTime * glowAlphaTweenSpeed;
			color.a = Mathf.Clamp01(value);
		}
		else if (color.a > num6)
		{
			float value2 = color.a - Time.deltaTime * glowAlphaTweenSpeed;
			color.a = Mathf.Clamp01(value2);
		}
		glowImage.color = color;
		Color color2 = sealImage.color;
		float a2 = alphaRange.Lerp(percentage, alphaCurve);
		if (num5 != 0 && glowImage.color.a >= 1f)
		{
			a2 = clickableAlpha;
		}
		color2.a = a2;
		sealImage.color = color2;
		if (vignette.enabled)
		{
			float t = percentage.Normalize01(vignetteMinPercentage, 1f);
			Color color3 = vignette.color;
			float target = new Vector2(0f, vignetteMaxAlpha).Lerp(t, vignetteCurve);
			color3.a = Mathf.SmoothDamp(vignette.color.a, target, ref refVignetteAlpha, vignetteSmoothTime);
			vignette.color = color3;
		}
		if (percentage >= circlesMinPercentage)
		{
			if (!circlesParticleSystem.isPlaying)
			{
				circlesParticleSystem.Play();
			}
			ParticleSystem.EmissionModule emission = circlesParticleSystem.emission;
			float t2 = percentage.Normalize01(circlesMinPercentage, 1f);
			emission.rateOverTime = circlesRateOverTimeRange.Lerp(t2, circlesRateOverTimeCurve);
		}
		else if (circlesParticleSystem.isPlaying)
		{
			circlesParticleSystem.Stop();
		}
		lightRaysParticleSystemRenderer.lengthScale = lightRaysLengthScaleRange.Lerp(percentage);
		ParticleSystem.EmissionModule emission2 = lightRaysParticleSystem.emission;
		emission2.rateOverTimeMultiplier = lightRaysLengthScaleRange.Lerp(percentage);
		ParticleSystem.EmissionModule emission3 = dustParticleSystem.emission;
		ParticleSystem.MainModule main = dustParticleSystem.main;
		emission3.rateOverTimeMultiplier = dustRateOverTimeRange.Lerp(percentage, dustCurve);
		main.startSpeed = dustStartSpeedRange.Lerp(percentage, dustCurve);
		ParticleSystem.MainModule main2 = smokeParticleSystem.main;
		main2.simulationSpeed = smokeSpeedRange.Lerp(percentage);
		hoverAudioSource.volume = percentage;
	}
}
