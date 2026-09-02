using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseModifierStepPointDisplay : MonoBehaviour
{
	[SerializeField]
	private GameObject completionFeedbackContainer;

	[SerializeField]
	private Image modifierSelectedBackgroundImage;

	[SerializeField]
	private Image pointImage;

	[SerializeField]
	private Sprite modifierNotSelectedBackgroundSprite;

	[SerializeField]
	private Sprite modifierSelectedBackgroundSprite;

	[SerializeField]
	private Sprite pointOnSprite;

	[SerializeField]
	private Sprite pointOffSprite;

	[SerializeField]
	private ApocalypseModifierDisplay modifierDisplay;

	private int currentStepIndex;

	public void ChangeDisplay(bool isOn)
	{
		pointImage.sprite = (isOn ? pointOnSprite : pointOffSprite);
		SetSelectedModifierBackground(modifierDisplay.IsSelected);
	}

	public void Init(int stepIndex)
	{
		currentStepIndex = stepIndex;
	}

	public void OnClick()
	{
		modifierDisplay.ChangeSelectedStep(currentStepIndex);
	}

	public void OnHover(bool isHovered)
	{
		modifierDisplay.ChangeModifierTooltip(isHovered ? currentStepIndex : (-1));
	}

	public void SetCompletionFeedbackVisible(bool isVisible)
	{
		completionFeedbackContainer.SetActive(isVisible);
	}

	public void SetSelectedModifierBackground(bool isModifierSelected)
	{
		modifierSelectedBackgroundImage.sprite = (isModifierSelected ? modifierSelectedBackgroundSprite : modifierNotSelectedBackgroundSprite);
	}
}
