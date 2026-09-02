using TMPro;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Resource;

public class MaterialsPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI materialsText;

	protected virtual void Awake()
	{
		materialsText.text = $"{TPSingleton<ResourceManager>.Instance.Materials}";
		TPSingleton<ResourceManager>.Instance.OnMaterialsChange += OnMaterialsChange;
	}

	private void OnDestroy()
	{
		if (TPSingleton<ResourceManager>.Exist())
		{
			TPSingleton<ResourceManager>.Instance.OnMaterialsChange -= OnMaterialsChange;
		}
	}

	private void OnMaterialsChange(int materials)
	{
		materialsText.text = materials.ToString();
	}
}
