using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseGameOverUnlockView : MonoBehaviour
{
	[SerializeField]
	private string activateState = string.Empty;

	[SerializeField]
	private TextMeshProUGUI apocalypseTitle;

	[SerializeField]
	private Animator systemUnlockAnimator;

	[SerializeField]
	private Animator rewardAnimator;

	[SerializeField]
	private string disabledState = string.Empty;

	private float animSpeed;

	public void Init(bool hasUnlock, bool isSystemUnlock)
	{
		if (!hasUnlock)
		{
			apocalypseTitle.text = string.Empty;
			rewardAnimator.gameObject.SetActive(value: false);
			systemUnlockAnimator.gameObject.SetActive(value: true);
			systemUnlockAnimator.Play(disabledState, 0, UnityEngine.Random.value);
			return;
		}
		apocalypseTitle.text = Localizer.Get("WorldMap_ApocalypseDifficulty_Apocalypse");
		if (isSystemUnlock)
		{
			rewardAnimator.gameObject.SetActive(value: false);
			systemUnlockAnimator.gameObject.SetActive(value: true);
		}
		else
		{
			rewardAnimator.gameObject.SetActive(value: true);
			systemUnlockAnimator.gameObject.SetActive(value: false);
		}
		(isSystemUnlock ? systemUnlockAnimator : rewardAnimator).Play(activateState, 0, UnityEngine.Random.value);
	}
}
