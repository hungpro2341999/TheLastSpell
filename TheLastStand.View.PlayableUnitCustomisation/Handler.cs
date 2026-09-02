using TPLib;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public abstract class Handler : RandomizableCustomizationElement
{
	[SerializeField]
	protected Image dropdownBackground;

	[SerializeField]
	protected BetterButton prevTextureButton;

	[SerializeField]
	protected BetterButton nextTextureButton;

	[SerializeField]
	protected BetterToggle lockToggle;

	[SerializeField]
	protected BetterButton randomizeButton;

	[SerializeField]
	protected Color interactableColor;

	[SerializeField]
	protected Color uninteractableColor;

	[SerializeField]
	private GameObject nextOptionInputDisplay;

	[SerializeField]
	private GameObject previousOptionInputDisplay;

	protected UnityAction<int> onValueChanged;

	public BetterToggle LockToggle => lockToggle;

	public abstract bool IsDropdownOpen { get; }

	public abstract void ChangeCurrentValue();

	public abstract void DecreaseCurrentValue();

	public abstract void IncreaseCurrentValue();

	public void MarkAsJoystickSelectedHandler()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.SetHandlerAsJoystickTarget(this);
		if (nextOptionInputDisplay != null)
		{
			nextOptionInputDisplay.gameObject.SetActive(value: true);
		}
		if (previousOptionInputDisplay != null)
		{
			previousOptionInputDisplay.gameObject.SetActive(value: true);
		}
	}

	public void UnmarkAsJoystickSelectedHandler()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.SetHandlerAsJoystickTarget(null);
		if (nextOptionInputDisplay != null)
		{
			nextOptionInputDisplay.gameObject.SetActive(value: false);
		}
		if (previousOptionInputDisplay != null)
		{
			previousOptionInputDisplay.gameObject.SetActive(value: false);
		}
	}

	public override void RandomizeValue(bool useWeights)
	{
	}

	public virtual void SwitchHandlerLockState(bool state)
	{
		nextTextureButton.Interactable = state;
		nextTextureButton.image.CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, ignoreTimeScale: false, useAlpha: true);
		prevTextureButton.Interactable = state;
		prevTextureButton.image.CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, ignoreTimeScale: false, useAlpha: true);
		randomizeButton.Interactable = state;
		randomizeButton.image.CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, ignoreTimeScale: false, useAlpha: true);
		lockToggle.interactable = state;
		lockToggle.image.CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, ignoreTimeScale: false, useAlpha: true);
		dropdownBackground.CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, ignoreTimeScale: false, useAlpha: true);
	}

	private void Awake()
	{
		UnmarkAsJoystickSelectedHandler();
	}
}
