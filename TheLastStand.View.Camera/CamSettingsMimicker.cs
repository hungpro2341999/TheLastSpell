using TPLib;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(UnityEngine.Camera))]
public class CamSettingsMimicker : MonoBehaviour
{
	[SerializeField]
	private UnityEngine.Camera target;

	[SerializeField]
	private bool mimickOrthoSize;

	[SerializeField]
	private bool mimickPixelPerfect;

	private UnityEngine.Camera cam;

	private PixelPerfectCamera pixelPerfectCam;

	private PixelPerfectCamera targetPixelPerfectCam;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (target == null)
		{
			if (TPSingleton<GameView>.Exist())
			{
				target = ACameraView.MainCam;
			}
			else
			{
				target = UnityEngine.Camera.main;
			}
		}
		if (cam == null)
		{
			cam = GetComponent<UnityEngine.Camera>();
		}
	}

	private void InitPixelPerfect()
	{
		if (target == null)
		{
			Init();
		}
		if (target == null)
		{
			TPDebug.LogError("Need to have a target! Disabling self...", this);
			base.enabled = false;
			return;
		}
		pixelPerfectCam = cam.GetComponent<PixelPerfectCamera>();
		targetPixelPerfectCam = target.GetComponent<PixelPerfectCamera>();
		if (pixelPerfectCam == null || targetPixelPerfectCam == null)
		{
			TPDebug.LogError("Either this or the target doesn't have a PixelPerfectCam! Disabling self...", this);
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (target == null)
		{
			Init();
			if (target == null)
			{
				TPDebug.LogError("Need to have a target! Disabling self...", this);
				base.enabled = false;
				return;
			}
		}
		if (mimickPixelPerfect)
		{
			if (pixelPerfectCam == null || targetPixelPerfectCam == null)
			{
				InitPixelPerfect();
				if (pixelPerfectCam == null || targetPixelPerfectCam == null)
				{
					return;
				}
			}
			if (pixelPerfectCam.assetsPPU != targetPixelPerfectCam.assetsPPU)
			{
				pixelPerfectCam.assetsPPU = targetPixelPerfectCam.assetsPPU;
			}
			if (pixelPerfectCam.refResolutionX != targetPixelPerfectCam.refResolutionX || pixelPerfectCam.refResolutionY != targetPixelPerfectCam.refResolutionY)
			{
				pixelPerfectCam.refResolutionX = targetPixelPerfectCam.refResolutionX;
				pixelPerfectCam.refResolutionY = targetPixelPerfectCam.refResolutionY;
			}
		}
		else if (mimickOrthoSize)
		{
			cam.orthographicSize = target.orthographicSize;
		}
	}
}
