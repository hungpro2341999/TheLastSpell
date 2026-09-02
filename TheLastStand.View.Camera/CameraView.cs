using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using TPLib;
using TPLib.UI;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TheLastStand.View.Camera;

public class CameraView : ACameraView
{
	[SerializeField]
	[Tooltip("Time taken in seconds to execute a camera movement. Only used in animation, like EnemyUnit hitting something.")]
	private float animationMoveSpeed = 0.2f;

	[SerializeField]
	private PostProcessVolume blurPostProcessVolume;

	[SerializeField]
	private float blurWeightStartValue = 0.5f;

	[SerializeField]
	private float blurWeightEndValue = 1f;

	[SerializeField]
	private float blurTransitionDuration = 1f;

	[SerializeField]
	private Ease blurInEasing = Ease.OutQuart;

	[SerializeField]
	private Ease blurOutEasing = Ease.InQuart;

	[SerializeField]
	[Range(0f, 7f)]
	private float dayNightTransitionDuration = 2f;

	[SerializeField]
	private Ease dayNightTransitionEasing = Ease.InOutSine;

	[SerializeField]
	private PostProcessVolume vignetteNightPostProcessVolume;

	[SerializeField]
	private PostProcessVolume bloomNightPostProcessVolume;

	[SerializeField]
	private CameraLUTView lutView;

	[SerializeField]
	private CameraVisualEffects visualEffects;

	[SerializeField]
	private RippleEffect rippleEffect;

	[SerializeField]
	private CameraPOI cameraPOI;

	[SerializeField]
	private CameraVisionArea cameraVision;

	[SerializeField]
	private CameraUIMasksHandler cameraUIMasksHandler;

	private ProCamera2DNumericBoundaries camBoundaries;

	private Tweener uiBlurTweener;

	private Tweener dayNightPostProcessTweener;

	public static float AnimationMoveSpeed => ((CameraView)TPSingleton<ACameraView>.Instance).animationMoveSpeed;

	public static CameraVisualEffects CameraVisualEffects => ((CameraView)TPSingleton<ACameraView>.Instance).visualEffects;

	public static CameraPOI CameraPOI => ((CameraView)TPSingleton<ACameraView>.Instance).cameraPOI;

	public static CameraLUTView CameraLutView => ((CameraView)TPSingleton<ACameraView>.Instance).lutView;

	public static CameraVisionArea CameraVision => ((CameraView)TPSingleton<ACameraView>.Instance).cameraVision;

	public static CameraUIMasksHandler CameraUIMasksHandler
	{
		get
		{
			CameraView cameraView = TPSingleton<ACameraView>.Instance as CameraView;
			if (cameraView == null)
			{
				return null;
			}
			if (cameraView.cameraUIMasksHandler == null)
			{
				cameraView.cameraUIMasksHandler = Object.FindObjectOfType<CameraUIMasksHandler>();
			}
			return cameraView.cameraUIMasksHandler;
		}
	}

	public static ProCamera2DNumericBoundaries ProCamera2DNumericBoundaries => ((CameraView)TPSingleton<ACameraView>.Instance).camBoundaries;

	public static RippleEffect RippleEffect => ((CameraView)TPSingleton<ACameraView>.Instance).rippleEffect;

	public static void OnGameStateChange(Game.E_State state)
	{
		switch (state)
		{
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
		case Game.E_State.HowToPlay:
		case Game.E_State.GameOver:
		case Game.E_State.MetaShops:
		case Game.E_State.ApocalypseMoreInfo:
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		case Game.E_State.CutscenePlaying:
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.BuildingExecutingSkill:
			ACameraView.AllowUserPan = TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.EnemyUnits;
			ACameraView.AllowUserZoom = ACameraView.AllowUserPan;
			break;
		default:
			ACameraView.AllowUserPan = !FogManager.MovingCameraToFog;
			ACameraView.AllowUserZoom = ACameraView.AllowUserPan;
			break;
		case Game.E_State.Wait:
			break;
		}
	}

	public static void AttenuateWorldForPopupFocus(IOverlayUser overlayUser)
	{
		if (TPSingleton<ACameraView>.Exist())
		{
			if (overlayUser != null)
			{
				TPSingleton<TPOverlay>.Instance.Display(overlayUser);
				((CameraView)TPSingleton<ACameraView>.Instance).BlurWorld(doBlur: true);
			}
			else
			{
				TPSingleton<TPOverlay>.Instance.Hide();
				((CameraView)TPSingleton<ACameraView>.Instance).BlurWorld(doBlur: false);
			}
		}
	}

	public static void RefreshDayTimeEffects(bool instant = false, bool onLoad = false)
	{
		if (!onLoad)
		{
			CameraLutView.RefreshLut(instant);
		}
		((CameraView)TPSingleton<ACameraView>.Instance).RefreshDayNightPostProcess(instant);
	}

	public void BlurWorld(bool doBlur, bool instant = false)
	{
		float num = (doBlur ? blurWeightEndValue : blurWeightStartValue);
		if (instant)
		{
			blurPostProcessVolume.weight = num;
			blurPostProcessVolume.enabled = doBlur;
			return;
		}
		if (uiBlurTweener != null)
		{
			uiBlurTweener.Kill();
		}
		uiBlurTweener = DOTween.To(() => blurPostProcessVolume.weight, delegate(float v)
		{
			blurPostProcessVolume.weight = v;
		}, num, blurTransitionDuration).SetFullId("CameraBlur", this).SetEase(doBlur ? blurInEasing : blurOutEasing)
			.OnKill(delegate
			{
				uiBlurTweener = null;
			});
		if (doBlur)
		{
			uiBlurTweener.OnStart(delegate
			{
				blurPostProcessVolume.enabled = true;
			});
		}
		else
		{
			uiBlurTweener.OnComplete(delegate
			{
				blurPostProcessVolume.enabled = false;
			});
		}
	}

	public void ChangeBoundaries(Vector4 boundaries)
	{
		camBoundaries.TopBoundary = boundaries.x;
		camBoundaries.BottomBoundary = boundaries.y;
		camBoundaries.LeftBoundary = boundaries.z;
		camBoundaries.RightBoundary = boundaries.w;
	}

	public override void Init()
	{
		base.Init();
		if (TPSingleton<GameManager>.Exist())
		{
			ChangeBoundaries(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.CameraBoundaries);
			CameraLutView.UpdateLutTextures();
		}
	}

	protected override void Awake()
	{
		camBoundaries = ProCamera2D.Instance?.GetComponent<ProCamera2DNumericBoundaries>();
		base.Awake();
	}

	protected override int ComputeTargetPixelsPerUnit(bool zoomedIn)
	{
		int num = base.ComputeTargetPixelsPerUnit(zoomedIn);
		if (Screen.height >= 1440)
		{
			num /= 2;
		}
		return num;
	}

	protected override void Update()
	{
		if (TPSingleton<GameManager>.Exist())
		{
			switch (TPSingleton<GameManager>.Instance.Game.State)
			{
			case Game.E_State.CharacterSheet:
			case Game.E_State.Recruitment:
			case Game.E_State.Shopping:
			case Game.E_State.BuildingUpgrade:
			case Game.E_State.NightReport:
			case Game.E_State.ProductionReport:
			case Game.E_State.Settings:
			case Game.E_State.HowToPlay:
			case Game.E_State.MetaShops:
			case Game.E_State.ApocalypseMoreInfo:
				return;
			}
		}
		base.Update();
	}

	private void RefreshDayNightPostProcess(bool instant = false)
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
		float num = (flag ? 1 : 0);
		if (instant)
		{
			vignetteNightPostProcessVolume.weight = num;
			vignetteNightPostProcessVolume.enabled = flag;
			bloomNightPostProcessVolume.weight = num;
			bloomNightPostProcessVolume.enabled = flag;
			return;
		}
		dayNightPostProcessTweener?.Kill();
		dayNightPostProcessTweener = DOTween.To(() => vignetteNightPostProcessVolume.weight, delegate(float v)
		{
			vignetteNightPostProcessVolume.weight = v;
			bloomNightPostProcessVolume.weight = v;
		}, num, dayNightTransitionDuration).SetFullId("NightPostProcess", this).SetEase(dayNightTransitionEasing)
			.OnKill(delegate
			{
				dayNightPostProcessTweener = null;
			});
		if (flag)
		{
			dayNightPostProcessTweener.OnStart(delegate
			{
				vignetteNightPostProcessVolume.enabled = true;
				bloomNightPostProcessVolume.enabled = true;
			});
		}
		else
		{
			dayNightPostProcessTweener.OnComplete(delegate
			{
				vignetteNightPostProcessVolume.enabled = false;
				bloomNightPostProcessVolume.enabled = false;
			});
		}
	}

	[ContextMenu("Blur OFF")]
	private void DebugDisableBlur()
	{
		BlurWorld(doBlur: false);
	}

	[ContextMenu("Blur ON")]
	private void DebugEnableBlur()
	{
		BlurWorld(doBlur: true);
	}
}
