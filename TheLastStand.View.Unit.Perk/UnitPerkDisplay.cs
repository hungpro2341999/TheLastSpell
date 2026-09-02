using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class UnitPerkDisplay : MonoBehaviour
{
	public static class Constants
	{
		public const string DefaultPerkIcon = "Default";

		public const string PerkIconPath = "View/Sprites/UI/Perks/";

		public const string PerkUnlockAnimationName = "PerkUnlock_Misc";

		public const string PerkUnlockAnimationTrigger = "unlock";

		public const string PerkRerollAnimationTrigger = "reroll";
	}

	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private TextMeshProUGUI perkName;

	[SerializeField]
	private TextMeshProUGUI perkDescription;

	[SerializeField]
	private TextMeshProUGUI descriptionAdditionalValues;

	[SerializeField]
	private GameObject separator;

	[SerializeField]
	private bool neverDisplayAdditionalValues;

	[SerializeField]
	private Image perkBorder;

	[SerializeField]
	private RectTransform perkSelectorPos;

	[SerializeField]
	private Image perkUnlockImage;

	[SerializeField]
	private Animator perkUnlockAnimator;

	[SerializeField]
	private Image perkRerollSuccessImage;

	[SerializeField]
	private Animator perkRerollSuccessAnimator;

	[SerializeField]
	private AnimationClip perkRerollSuccessClip;

	[SerializeField]
	private Image perkRerollHoverImage;

	[SerializeField]
	private Image perkRerollSelectionImage;

	[SerializeField]
	private Sprite perkRerollSelectionImagePayable;

	[SerializeField]
	private Sprite perkRerollSelectionImageNotPayable;

	[SerializeField]
	private Canvas bookmarkCanvas;

	[SerializeField]
	private Animator bookmarkAnimator;

	[SerializeField]
	private Image bookmarkImage;

	[SerializeField]
	private AudioClip[] bookmarkAudioClips;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private Selectable defaultSelectOnLeft;

	[SerializeField]
	private bool IsOnBottomLine;

	private Tween bookmarkFadeTween;

	private static readonly int RerollHash = Animator.StringToHash("reroll");

	private static readonly int UnlockHash = Animator.StringToHash("unlock");

	public bool CanDisplayAdditionalInfo
	{
		get
		{
			if (Perk != null)
			{
				return Perk.Owner != null;
			}
			return false;
		}
	}

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform PerkSelectorPos => perkSelectorPos;

	public TheLastStand.Model.Unit.Perk.Perk Perk { get; private set; }

	public PerkDefinition PerkDefinition { get; private set; }

	public Selectable DefaultSelectOnLeft => defaultSelectOnLeft;

	private bool HasPotentialReroll
	{
		get
		{
			if (Perk != null && !Perk.UnlockedInPerkTree)
			{
				return Perk.HasPotentialReroll();
			}
			return false;
		}
	}

	public void DisplayBookmark(bool triggerAnimation = true)
	{
		bookmarkFadeTween?.Kill();
		Color color = bookmarkImage.color;
		color.a = 1f;
		bookmarkImage.color = color;
		bookmarkCanvas.enabled = true;
		if (triggerAnimation)
		{
			bookmarkAnimator.SetTrigger("Appear");
			SoundManager.PlayAudioClip(bookmarkAudioClips[UnityEngine.Random.Range(0, bookmarkAudioClips.Length)]);
		}
	}

	public void HideBookmark()
	{
		bookmarkFadeTween = bookmarkImage.DOFade(0f, 0.3f).OnComplete(delegate
		{
			bookmarkCanvas.enabled = false;
		});
	}

	public void Init()
	{
		if (PerkDefinition == null)
		{
			if (perkIcon != null)
			{
				perkIcon.enabled = false;
			}
			return;
		}
		if (perkIcon != null)
		{
			perkIcon.enabled = true;
			perkIcon.sprite = PerkDefinition.PerkSprite;
		}
		if (perkName != null)
		{
			perkName.text = PerkDefinition.Name;
		}
		if (perkDescription != null)
		{
			perkDescription.text = PerkDefinition.GetDescription(Perk);
		}
		if (descriptionAdditionalValues != null)
		{
			if (Perk != null && PerkDefinition.PerkEffectsInformationsExist && ((IsPerkUnlockedForCurrentDisplay() && !neverDisplayAdditionalValues) || PerkDefinition.DisplayBonusBeforePurchase) && CanDisplayAdditionalInfo)
			{
				descriptionAdditionalValues.gameObject.SetActive(value: true);
				separator.SetActive(value: true);
				descriptionAdditionalValues.text = PerkDefinition.GetAdditionDescription(Perk);
			}
			else if (descriptionAdditionalValues.gameObject.activeInHierarchy || separator.activeInHierarchy)
			{
				descriptionAdditionalValues.gameObject.SetActive(value: false);
				separator.SetActive(value: false);
			}
		}
		if (bookmarkCanvas != null && Perk.Bookmarked)
		{
			bookmarkCanvas.enabled = true;
			DisplayBookmark(triggerAnimation: false);
		}
	}

	public bool IsPerkUnlockedForCurrentDisplay()
	{
		if (Perk != null)
		{
			if (Perk.PerkTier != null)
			{
				return Perk.UnlockedInPerkTree;
			}
			return Perk.Unlocked;
		}
		return false;
	}

	public void OnPerkButtonClick()
	{
		if (!TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.IsInRerollMode)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.SelectPerk(this);
		}
	}

	public void OnJoystickSelect()
	{
		if (TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree == null)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Refresh();
		}
		OnPerkButtonClick();
	}

	public void OnJoystickDeselect()
	{
		TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.SelectPerk(null);
		JoystickSelectable.TooltipDisplayer.HideTooltip();
	}

	public void Refresh()
	{
		if (Perk == null)
		{
			perkBorder.sprite = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>(IsOnBottomLine ? "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_Off" : "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_Off", "Misc");
			return;
		}
		perkBorder.sprite = (IsPerkUnlockedForCurrentDisplay() ? UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>(IsOnBottomLine ? "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_On" : "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_On", Perk.CollectionId) : UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>(IsOnBottomLine ? "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_Off" : "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_Off", Perk.CollectionId));
		if (InputManager.IsLastControllerJoystick && EventSystem.current.currentSelectedGameObject == base.gameObject && TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			JoystickSelectable.TooltipDisplayer.HideTooltip();
			JoystickSelectable.TooltipDisplayer.DisplayTooltip();
		}
		if (bookmarkCanvas != null)
		{
			bookmarkCanvas.enabled = Perk.Bookmarked;
		}
	}

	public void OnRerollPointerEnter(bool forceHighlight = false)
	{
		perkRerollHoverImage.enabled = forceHighlight || HasPotentialReroll;
	}

	public void OnRerollPointerExit()
	{
		perkRerollHoverImage.enabled = false;
	}

	public void OnRerollSelection(bool isSelected, bool forceHighlight, bool canReroll)
	{
		bool flag = isSelected && (forceHighlight || HasPotentialReroll);
		perkRerollSelectionImage.enabled = flag;
		if (flag)
		{
			perkRerollSelectionImage.sprite = (canReroll ? perkRerollSelectionImagePayable : perkRerollSelectionImageNotPayable);
		}
	}

	public void SetAvailabilityMaterial(Material perkMaterial, Material backgroundMaterial)
	{
		perkBorder.material = backgroundMaterial;
		perkIcon.material = perkMaterial;
	}

	public void SetContent(TheLastStand.Model.Unit.Perk.Perk perk, PerkDefinition perkDefinition = null)
	{
		Perk = perk;
		PerkDefinition = perk?.PerkDefinition ?? perkDefinition;
	}

	public void Unlock()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsPerksPanelOpened)
		{
			AnimationClip collectionAssetOrDefault = UnitPerkTreeView.GetCollectionAssetOrDefault<AnimationClip>("Animation/PerkUnlock/PerkUnlock_{0}", Perk.CollectionId);
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(perkUnlockAnimator.runtimeAnimatorController);
			animatorOverrideController["PerkUnlock_Misc"] = collectionAssetOrDefault;
			perkUnlockAnimator.runtimeAnimatorController = animatorOverrideController;
			StartCoroutine(UnlockCoroutine(collectionAssetOrDefault.length));
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.PlayPerkSelectionSound();
		}
	}

	public void PlayRerollSuccess()
	{
		StartCoroutine(RerollSuccessCoroutine(perkRerollSuccessClip.length));
	}

	private void OnDisable()
	{
		if (perkUnlockImage != null)
		{
			perkUnlockImage.enabled = false;
		}
		if (perkRerollSuccessImage != null)
		{
			perkRerollSuccessImage.enabled = false;
		}
	}

	private IEnumerator UnlockCoroutine(float unlockDuration)
	{
		perkUnlockAnimator.enabled = true;
		perkUnlockAnimator.SetTrigger(UnlockHash);
		yield return SharedYields.WaitForSeconds(unlockDuration);
		perkUnlockAnimator.enabled = false;
		perkUnlockImage.enabled = false;
	}

	private IEnumerator RerollSuccessCoroutine(float rerollDuration)
	{
		perkRerollSuccessAnimator.enabled = true;
		perkRerollSuccessImage.enabled = true;
		perkRerollSuccessAnimator.SetTrigger(RerollHash);
		if (TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.CanPlayRerollSound)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.PlayPerkRerollSound();
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.CanPlayRerollSound = false;
		}
		yield return SharedYields.WaitForSeconds(rerollDuration);
		perkRerollSuccessAnimator.enabled = false;
		perkRerollSuccessImage.enabled = false;
	}
}
