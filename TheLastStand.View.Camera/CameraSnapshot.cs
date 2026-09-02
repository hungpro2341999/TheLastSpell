using TPLib;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(UnityEngine.Camera))]
public class CameraSnapshot : TPSingleton<CameraSnapshot>
{
	public static class Constants
	{
		public static readonly int SnaptshotLayer;

		public static readonly Vector3 OffsetFromTarget;

		private const string SnaptshotLayerName = "Unit Snapshot";

		static Constants()
		{
			OffsetFromTarget = new Vector3(0f, 0f, -1f);
			SnaptshotLayer = LayerMask.NameToLayer("Unit Snapshot");
		}
	}

	[SerializeField]
	private UnityEngine.Camera cam;

	[SerializeField]
	private Vector2Int renderTextureDimensions = new Vector2Int(128, 128);

	private Transform parentBackup;

	[SerializeField]
	[HideInInspector]
	private PixelPerfectCamera pixelPerfectCamera;

	private RenderTexture renderTexture;

	public Sprite TakeSnapshot(ISnapshotable target)
	{
		if (target == null)
		{
			TPDebug.LogError("TakeSnapshot() can't work with a null target. Aborting...", this);
			return null;
		}
		target.PrepareForSnapshot();
		parentBackup = base.transform.parent;
		base.transform.SetParent(target.SnapshotPosition, worldPositionStays: false);
		base.transform.localPosition = Constants.OffsetFromTarget;
		cam.enabled = true;
		cam.Render();
		cam.enabled = false;
		base.transform.SetParent(parentBackup, worldPositionStays: false);
		target.OnSnapshotFinished();
		Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, mipChain: false)
		{
			filterMode = FilterMode.Point,
			anisoLevel = 0
		};
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		Rect rect = new Rect(0f, 0f, texture2D.width, texture2D.height);
		texture2D.ReadPixels(rect, 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		return Sprite.Create(texture2D, rect, new Vector2(0.5f, 0.5f), 100f);
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void CreateRenderTexture()
	{
		int width = ((pixelPerfectCamera != null) ? pixelPerfectCamera.refResolutionX : renderTextureDimensions.x);
		int height = ((pixelPerfectCamera != null) ? pixelPerfectCamera.refResolutionY : renderTextureDimensions.y);
		renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
		{
			filterMode = FilterMode.Point,
			autoGenerateMips = false,
			useMipMap = false,
			anisoLevel = 0
		};
		cam.targetTexture = renderTexture;
	}

	private void Init()
	{
		if (cam == null)
		{
			cam = GetComponent<UnityEngine.Camera>();
		}
		CreateRenderTexture();
	}
}
