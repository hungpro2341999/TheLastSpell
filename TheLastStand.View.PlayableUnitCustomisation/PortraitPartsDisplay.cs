using System;
using System.Collections.Generic;
using PortraitAPI;
using PortraitAPI.Layers;
using Sirenix.OdinInspector;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PortraitPartsDisplay : SerializedMonoBehaviour
{
	[SerializeField]
	private Dictionary<Commons.E_LayerKind, Image> parts = new Dictionary<Commons.E_LayerKind, Image>();

	public void UpdateValues(Dictionary<Commons.E_LayerType, SimpleLayer> currentLayers, Commons.E_Gender gender, PlayableUnit playableUnit, Dictionary<Commons.E_LayerKind, Material> layerMaterials)
	{
		Dictionary<Commons.E_LayerKind, Texture2D> texturesByLayerKind = LayerManagement.GetTexturesByLayerKind(currentLayers);
		for (int i = 0; i < Enum.GetValues(typeof(Commons.E_LayerKind)).Length; i++)
		{
			Commons.E_LayerKind key = (Commons.E_LayerKind)i;
			Image image = parts[key];
			if (parts.ContainsKey(key) && texturesByLayerKind.ContainsKey(key))
			{
				if (texturesByLayerKind[key] == null)
				{
					image.enabled = false;
					continue;
				}
				image.enabled = true;
				image.sprite = Sprite.Create(texturesByLayerKind[key], new Rect(0f, 0f, PortraitAPIManager.GetTexturesData().Width, PortraitAPIManager.GetTexturesData().Height), PortraitAPIManager.GetTexturesData().PivotPoint, PortraitAPIManager.GetTexturesData().PixelPerUnit);
				image.material = layerMaterials[key];
			}
			else
			{
				image.enabled = false;
			}
		}
	}
}
