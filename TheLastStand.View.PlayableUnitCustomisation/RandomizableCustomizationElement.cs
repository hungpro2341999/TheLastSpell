using UnityEngine;

namespace TheLastStand.View.PlayableUnitCustomisation;

public abstract class RandomizableCustomizationElement : MonoBehaviour
{
	public bool IsLocked { get; set; }

	public abstract void RandomizeValue(bool useWeight);
}
