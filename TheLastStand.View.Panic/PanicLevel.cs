using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Panic;

public class PanicLevel : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private Sprite levelOnSprite;

	[SerializeField]
	private Sprite levelOffSprite;

	[SerializeField]
	private Material panicThresholdPulseMaterial;

	[SerializeField]
	private Transform indicatorsTransform;

	[SerializeField]
	private PanicLevelIndicator panicLevelIndicatorPrefab;

	private Material initialMaterial;

	private PanicLevelIndicator[] panicLevelIndicators;

	public void InstantiateIndicators(int indicatorsCount)
	{
		panicLevelIndicators = new PanicLevelIndicator[indicatorsCount];
		for (int i = 0; i < indicatorsCount; i++)
		{
			panicLevelIndicators[i] = Object.Instantiate(panicLevelIndicatorPrefab, indicatorsTransform);
		}
	}

	public void Activate()
	{
		image.sprite = levelOnSprite;
		for (int num = panicLevelIndicators.Length - 1; num >= 0; num--)
		{
			panicLevelIndicators[num].Activate();
		}
	}

	public void ActivateThresholdMaterial()
	{
		image.sprite = levelOnSprite;
		image.material = panicThresholdPulseMaterial;
		for (int num = panicLevelIndicators.Length - 1; num >= 0; num--)
		{
			panicLevelIndicators[num].ActivateThresholdMaterial();
		}
	}

	public void Disactivate()
	{
		image.sprite = levelOffSprite;
		for (int num = panicLevelIndicators.Length - 1; num >= 0; num--)
		{
			panicLevelIndicators[num].Disactivate();
		}
	}

	public void DisactivateThresholdMaterial()
	{
		image.material = initialMaterial;
		for (int num = panicLevelIndicators.Length - 1; num >= 0; num--)
		{
			panicLevelIndicators[num].DisactivateThresholdMaterial();
		}
	}

	private void Awake()
	{
		initialMaterial = image.material;
	}
}
