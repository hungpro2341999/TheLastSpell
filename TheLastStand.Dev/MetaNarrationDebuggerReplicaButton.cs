using TMPro;
using TPLib.Localization;
using TheLastStand.Framework.UI;
using TheLastStand.Model.Meta;
using UnityEngine;

namespace TheLastStand.Dev;

public class MetaNarrationDebuggerReplicaButton : MonoBehaviour
{
	[SerializeField]
	private Animator selectorAnimator;

	[SerializeField]
	private BetterButton replicaButton;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	public BetterButton Button => replicaButton;

	public MetaNarration Context { get; set; }

	public bool Displayed { get; private set; }

	public MetaReplica Replica { get; private set; }

	public void AllowInteraction(bool allowed)
	{
		replicaButton.Interactable = allowed;
	}

	public void Display(bool show)
	{
		Displayed = show;
		base.gameObject.SetActive(show);
	}

	public void HideWithoutDisabling()
	{
		selectorAnimator.SetTrigger("Hide");
		replicaButton.Interactable = false;
	}

	public void Init(MetaNarration narration)
	{
		Context = narration;
	}

	public void OnClick()
	{
		Context.MetaNarrationController.MarkReplicaAsUsed(Replica);
	}

	public void OnHover(bool hover)
	{
		selectorAnimator.SetBool("Hover", hover && replicaButton.Interactable);
	}

	public void SetReplica(MetaReplica replica)
	{
		Replica = replica;
		buttonText.text = Localizer.Get(replica.LocalizationKey);
		selectorAnimator.SetTrigger("BackToIdle");
	}
}
