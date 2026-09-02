using TMPro;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class SkillPointsFeedback : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	public void Refresh(int skillPoints, int skillPointsTotal)
	{
		text.text = $"{skillPoints}/{skillPointsTotal}";
	}
}
