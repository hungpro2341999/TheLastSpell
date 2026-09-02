using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Panic;

public class PanicLevelIndicator : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private Sprite indicatorOnSprite;

	[SerializeField]
	private Sprite indicatorOffSprite;

	[SerializeField]
	private Material panicThresholdPulseMaterial;

	private Material initialMaterial;

	public void Activate()
	{
		image.sprite = indicatorOnSprite;
	}

	public void ActivateThresholdMaterial()
	{
		image.sprite = indicatorOnSprite;
		image.material = panicThresholdPulseMaterial;
	}

	public void Disactivate()
	{
		image.sprite = indicatorOffSprite;
	}

	public void DisactivateThresholdMaterial()
	{
		image.material = initialMaterial;
	}

	private void Awake()
	{
		initialMaterial = image.material;
	}
}
