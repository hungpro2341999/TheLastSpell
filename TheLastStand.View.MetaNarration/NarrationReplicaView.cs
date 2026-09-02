using TPLib.Localization;
using TheLastStand.Framework.UI;
using TheLastStand.Model.Meta;
using TheLastStand.View.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaNarration;

public class NarrationReplicaView : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Animator selectorAnimator;

	[SerializeField]
	private Image replicaImage;

	[SerializeField]
	private BetterButton replicaButton;

	[SerializeField]
	private AudioClip onClickSoundDark;

	[SerializeField]
	private AudioClip onClickSoundLight;

	[SerializeField]
	private PlayUISound soundPlayer;

	public BetterButton Button => replicaButton;

	public MetaReplica Replica { get; private set; }

	public NarrationView NarrationView { get; set; }

	public void AllowInteraction(bool allowed)
	{
		replicaButton.Interactable = allowed;
	}

	public void Display(bool show)
	{
		base.gameObject.SetActive(show);
	}

	public void HideWithoutDisabling()
	{
		selectorAnimator.SetTrigger("Hide");
		replicaButton.Interactable = false;
	}

	public void OnClick()
	{
		NarrationView.OnReplicaSelected(this);
		selectorAnimator.SetTrigger(NarrationView.MetaNarration.IsDarkOne ? "Click_Dark" : "Click_Light");
		selectorAnimator.SetBool("Hover", value: false);
		soundPlayer.PlayAudioClip(NarrationView.MetaNarration.IsDarkOne ? onClickSoundDark : onClickSoundLight);
	}

	public void OnHover(bool hover)
	{
		selectorAnimator.SetBool("Hover", hover && replicaButton.Interactable);
	}

	public void SetReplica(MetaReplica replica)
	{
		Display(show: true);
		replicaButton.Interactable = true;
		replicaImage.enabled = true;
		selectorAnimator.SetTrigger("BackToIdle");
		Replica = replica;
		replicaButton.ChangeText(Localizer.Get(replica.LocalizationKey));
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnHover(hover: true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnHover(hover: false);
	}
}
