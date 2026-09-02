using TMPro;
using TPLib.Localization;
using TheLastStand.View.WorldMap.Apocalypse;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class ApocalypseLevelView : MonoBehaviour
{
	[SerializeField]
	private string activateState = string.Empty;

	[SerializeField]
	private string activateIdleState = string.Empty;

	[SerializeField]
	private TextMeshProUGUI apocalypseName;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string disabledState = string.Empty;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Image arkBackgroundImage;

	[SerializeField]
	private Sprite arkIdleSprite;

	[SerializeField]
	private Sprite arkLitSprite;

	[SerializeField]
	private bool hasKindleFlameFeedback;

	[SerializeField]
	private ApocalypseFlameKindleFeedback flameKindleFeedback;

	private float animSpeed;

	private bool kindleFlameAnimationPlayed;

	public ApocalypseFlameKindleFeedback FlameKindleFeedback => flameKindleFeedback;

	public void Init(int level, bool playKindleFlameFeedback = false)
	{
		if (levelText != null)
		{
			levelText.enabled = level > 0;
			if (level > 0)
			{
				levelText.text = $"<style=Bad>{level}</style>";
			}
		}
		if (apocalypseName != null)
		{
			if (level == -1)
			{
				apocalypseName.text = string.Empty;
			}
			else
			{
				apocalypseName.text = Localizer.Get((level == 0) ? "WorldMap_ApocalypseDifficulty_Normal" : "WorldMap_ApocalypseDifficulty_Apocalypse");
			}
		}
		if (arkBackgroundImage != null)
		{
			arkBackgroundImage.sprite = ((level > 0) ? arkLitSprite : arkIdleSprite);
		}
		string text = disabledState;
		if (level >= 0)
		{
			text = ((level != 0 || string.IsNullOrEmpty(activateIdleState)) ? activateState : activateIdleState);
		}
		if (level == 0)
		{
			kindleFlameAnimationPlayed = false;
		}
		if (level > 0 && hasKindleFlameFeedback && flameKindleFeedback != null)
		{
			if (playKindleFlameFeedback && !kindleFlameAnimationPlayed)
			{
				flameKindleFeedback.PlayFeedback();
			}
			kindleFlameAnimationPlayed = true;
		}
		if (!playKindleFlameFeedback || text != activateState)
		{
			animator.Play(text, 0, UnityEngine.Random.value);
		}
	}

	public void PlayActivateState()
	{
		animator.Play(activateState, 0, UnityEngine.Random.value);
	}

	public void StopAnimation(bool stopAnimations)
	{
		animator.speed = (stopAnimations ? 0f : animSpeed);
		if (stopAnimations)
		{
			if (flameKindleFeedback != null)
			{
				flameKindleFeedback.PauseAnimations();
			}
		}
		else if (flameKindleFeedback != null)
		{
			flameKindleFeedback.ContinueAnimations();
		}
	}

	private void Awake()
	{
		RetrieveAnimatorSpeed();
		if (flameKindleFeedback != null)
		{
			flameKindleFeedback.OnFlameKindled += PlayActivateState;
		}
	}

	private void OnDestroy()
	{
		if (flameKindleFeedback != null)
		{
			flameKindleFeedback.OnFlameKindled -= PlayActivateState;
		}
	}

	private void RetrieveAnimatorSpeed()
	{
		animSpeed = animator.speed;
	}
}
