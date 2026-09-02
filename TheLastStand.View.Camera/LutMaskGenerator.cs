using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(UnityEngine.Camera))]
public class LutMaskGenerator : TPSingleton<LutMaskGenerator>
{
	[SerializeField]
	private AmplifyColorEffect amplifyColor;

	[SerializeField]
	private bool useScreenResolution = true;

	[SerializeField]
	private bool useCustomResolution;

	[SerializeField]
	protected int maskWidth = 1920;

	[SerializeField]
	protected int maskHeigth = 1080;

	private int width;

	private int height;

	private RenderTexture maskTexture;

	private UnityEngine.Camera cam;

	private List<MaskableBehaviour> maskablesObjects;

	private RenderTexture activeRTBackup;

	public List<MaskableBehaviour> MaskableObjects
	{
		get
		{
			if (maskablesObjects == null)
			{
				maskablesObjects = new List<MaskableBehaviour>();
			}
			return maskablesObjects;
		}
	}

	protected void OnEnable()
	{
		Init();
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChange;
		UpdateRenderTexture();
		UpdateCameraProperties();
	}

	private void OnResolutionChange(Resolution resolution)
	{
		UpdateRenderTexture();
		UpdateCameraProperties();
	}

	protected void OnDisable()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChange;
		}
		Cleanup();
	}

	public void Init()
	{
		if (cam == null)
		{
			cam = GetComponent<UnityEngine.Camera>();
		}
		maskablesObjects = new List<MaskableBehaviour>();
	}

	private void UpdateRenderTexture()
	{
		if (!(cam == null) && !(amplifyColor == null))
		{
			int num;
			int num2;
			if (useCustomResolution)
			{
				num = maskWidth;
				num2 = maskHeigth;
			}
			else if (useScreenResolution)
			{
				num = Screen.width;
				num2 = Screen.height;
			}
			else
			{
				num = (int)((float)cam.pixelWidth + 0.5f);
				num2 = (int)((float)cam.pixelHeight + 0.5f);
			}
			if (maskTexture == null || width != num || height != num2)
			{
				width = num;
				height = num2;
				Cleanup();
				maskTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear)
				{
					hideFlags = HideFlags.HideAndDontSave,
					name = "MaskTexture"
				};
				maskTexture.antiAliasing = ((QualitySettings.antiAliasing <= 0) ? 1 : QualitySettings.antiAliasing);
				maskTexture.autoGenerateMips = false;
				maskTexture.Create();
			}
			if (amplifyColor != null && amplifyColor.MaskTexture != maskTexture)
			{
				amplifyColor.MaskTexture = maskTexture;
			}
		}
	}

	private void UpdateCameraProperties()
	{
		if (!(cam == null))
		{
			cam.targetTexture = maskTexture;
		}
	}

	private void Cleanup()
	{
		if (maskTexture != null)
		{
			if (cam != null && cam.targetTexture == maskTexture)
			{
				cam.targetTexture = null;
			}
			Object.DestroyImmediate(maskTexture);
		}
	}

	private void EnableMaskableMaterials(bool enableMasking)
	{
		Shader.SetGlobalFloat("_UseMaskingColor", enableMasking ? 1 : 0);
		foreach (MaskableBehaviour maskableObject in MaskableObjects)
		{
			maskableObject.SwitchMaterial(enableMasking);
		}
	}

	private void OnPreRender()
	{
		if (!(maskTexture == null))
		{
			activeRTBackup = RenderTexture.active;
			RenderTexture.active = maskTexture;
			EnableMaskableMaterials(enableMasking: true);
		}
	}

	private void OnPostRender()
	{
		if (!(maskTexture == null))
		{
			EnableMaskableMaterials(enableMasking: false);
			RenderTexture.active = activeRTBackup;
		}
	}
}
