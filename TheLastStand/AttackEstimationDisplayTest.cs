using TheLastStand.Framework.Animation;
using UnityEngine;

namespace TheLastStand;

public class AttackEstimationDisplayTest : MonoBehaviour
{
	public enum E_DeathProbability
	{
		None,
		Possible,
		Guaranteed
	}

	[SerializeField]
	private ImageAnimator mainImage;

	[SerializeField]
	private bool isSurrounding;

	[SerializeField]
	private bool isDamages;

	[SerializeField]
	private string effectId = string.Empty;

	[SerializeField]
	private E_DeathProbability deathProbability;

	public void Refresh()
	{
		Debug.Log("Updating view according to values.");
		string text = "View/Sprites/UI/Units/AttackEstimation/";
		if (isSurrounding)
		{
			text += "Surrounding/";
		}
		if (isDamages)
		{
			switch (deathProbability)
			{
			case E_DeathProbability.None:
				text += "NoDeath";
				break;
			case E_DeathProbability.Possible:
				text += "PossibleDeath";
				break;
			case E_DeathProbability.Guaranteed:
				text += "GuaranteedDeath";
				break;
			}
		}
		else
		{
			text = text + effectId + "/";
		}
		mainImage.SpritesPath = text;
		mainImage.Refresh();
	}
}
