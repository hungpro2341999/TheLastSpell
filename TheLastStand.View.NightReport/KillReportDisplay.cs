using TMPro;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.NightReport;

public class KillReportDisplay : MonoBehaviour
{
	public class Constants
	{
		public const string AnimatorParamaterAppear = "Appear";

		public const string AnimatorParamaterDisappear = "Disappear";

		public const string EnemyBackgroundSpritePathFormat = "View/Sprites/UI/NightReportPanel/KillReport_{0}";
	}

	[SerializeField]
	private Image enemyBackground;

	[SerializeField]
	private TextMeshProUGUI killsCountText;

	[SerializeField]
	private TextMeshProUGUI totalXPText;

	[SerializeField]
	private Animator killReportAnimator;

	public void Refresh(string enemyId, int kills, float experience)
	{
		enemyBackground.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/NightReportPanel/KillReport_{enemyId}");
		killsCountText.text = $"x{kills}";
		totalXPText.text = experience.ToString();
	}

	private void OnEnable()
	{
		killReportAnimator.SetTrigger("Appear");
	}

	private void OnDisable()
	{
		killReportAnimator.SetTrigger("Disappear");
	}
}
