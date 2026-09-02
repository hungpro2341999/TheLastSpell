using TPLib;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Recruitment;

public class RerollButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private BetterButton button;

	public BetterButton Button => button;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (TPSingleton<ResourceManager>.Instance.Gold < RecruitmentController.ComputeRerollCost())
		{
			string localizationKey = "Recruitment_NotEnoughGold";
			RecruitmentView.WarningTooltip.SetContent(localizationKey);
			RecruitmentView.WarningTooltip.Display();
		}
		else
		{
			RecruitmentView.WarningTooltip.Hide();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		RecruitmentView.WarningTooltip.Hide();
	}
}
