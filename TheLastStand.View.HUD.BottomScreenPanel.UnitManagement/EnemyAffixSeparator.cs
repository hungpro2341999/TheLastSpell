using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.UnitManagement;

public class EnemyAffixSeparator : MonoBehaviour
{
	[SerializeField]
	private Image separatorImage;

	public void Toggle(bool toggle)
	{
		separatorImage.enabled = toggle;
	}
}
