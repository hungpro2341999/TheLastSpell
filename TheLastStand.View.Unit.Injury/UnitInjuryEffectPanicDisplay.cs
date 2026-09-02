using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryEffectPanicDisplay : MonoBehaviour
{
	[SerializeField]
	private Image injuryIcon;

	[SerializeField]
	private DataSpriteTable injuryIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private float panicMalus;

	public virtual void Init(float newPanicMalus, int injuryStage)
	{
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		panicMalus = newPanicMalus;
		RefreshTitle();
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		titleText.text = string.Format("{0}{1} {2} {3}", (panicMalus >= 0f) ? "+" : string.Empty, panicMalus, AtlasIcons.PanicIcon, Localizer.Get("Panic_Name"));
	}
}
