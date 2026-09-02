using DG.Tweening;
using TPLib;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ChooseRewardShelf : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 5f)]
	private float appearTweenDuration = 1f;

	[SerializeField]
	private Ease appearTweenEase = Ease.OutBack;

	[SerializeField]
	[Range(150f, 250f)]
	private float heightSelected = 200f;

	[SerializeField]
	[Range(0f, 3f)]
	private float selectTweenDuration = 1f;

	[SerializeField]
	private Ease selectTweenEase = Ease.OutCubic;

	[SerializeField]
	private RectTransform shelfRectTransform;

	[SerializeField]
	private Image rarityCloth;

	[SerializeField]
	private DataSpriteTable rarityClothSprites;

	[SerializeField]
	private Image cycleIcon;

	[SerializeField]
	private DataSpriteTable cycleIconSprites;

	[SerializeField]
	private RewardItemSlotView rewardItemSlotView;

	private float heightInit;

	private Tween moveTween;

	public float AppearTweenDuration => appearTweenDuration;

	public int ItemIndex { get; set; }

	public RewardItemSlotView RewardItemSlotView => rewardItemSlotView;

	public void Appear(float delay, AudioClip appearClip)
	{
		moveTween?.Kill();
		shelfRectTransform.sizeDelta = new Vector2(shelfRectTransform.sizeDelta.x, -50f);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		moveTween = shelfRectTransform.DOSizeDelta(new Vector2(shelfRectTransform.sizeDelta.x, heightInit), appearTweenDuration).SetEase(appearTweenEase).SetDelay(delay)
			.SetFullId("RewardShelfAppear", this)
			.OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			})
			.OnStart(delegate
			{
				SoundManager.PlayAudioClip(appearClip);
			});
	}

	public void Disappear(float delay, AudioClip disappearClip)
	{
		moveTween?.Kill();
		shelfRectTransform.sizeDelta = new Vector2(shelfRectTransform.sizeDelta.x, heightInit);
		moveTween = shelfRectTransform.DOSizeDelta(new Vector2(shelfRectTransform.sizeDelta.x, -50f), appearTweenDuration).SetEase(appearTweenEase).SetDelay(delay)
			.SetFullId("RewardShelfDisappear", this)
			.OnStart(delegate
			{
				SoundManager.PlayAudioClip(disappearClip);
			});
	}

	public void DisplaySelection()
	{
		rewardItemSlotView.DisplaySelectionBG();
		moveTween?.Kill();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		moveTween = shelfRectTransform.DOSizeDelta(new Vector2(shelfRectTransform.sizeDelta.x, (TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem == TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.Items[ItemIndex]) ? heightSelected : heightInit), selectTweenDuration).SetEase(selectTweenEase).SetFullId("RewardShelfSelection", this)
			.OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			});
	}

	public void Refresh()
	{
		rarityCloth.sprite = rarityClothSprites.GetSpriteAt((int)(TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.Items[ItemIndex].Rarity - 1));
		cycleIcon.sprite = cycleIconSprites.GetSpriteAt(TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.IsNightProduction ? 1 : 0);
		rewardItemSlotView.RewardItemSlot.Item = TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.Items[ItemIndex];
		rewardItemSlotView.Refresh();
	}

	public void Reload()
	{
		RewardItemSlot rewardItemSlot = new RewardItemSlotController(ItemDatabase.ItemSlotDefinitions[ItemSlotDefinition.E_ItemSlotId.RewardItem], rewardItemSlotView).RewardItemSlot;
		rewardItemSlotView.ItemSlot = rewardItemSlot;
	}

	private void Awake()
	{
		Reload();
		heightInit = shelfRectTransform.sizeDelta.y;
	}
}
