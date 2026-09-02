using System;
using System.Collections;
using TMPro;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Trait;

public class UnitTraitDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI traitTitleText;

	[SerializeField]
	private TextMeshProUGUI traitDescriptionText;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image textImage;

	[SerializeField]
	private Sprite textHoveredSprite;

	private UnitTraitDefinition traitDefinition;

	private TheLastStand.Model.Unit.Unit targetUnit;

	private Sprite textInitSprite;

	public TheLastStand.Model.Unit.Unit TargetUnit
	{
		get
		{
			return targetUnit ?? TileObjectSelectionManager.SelectedUnit;
		}
		set
		{
			if (targetUnit != value)
			{
				targetUnit = value;
			}
		}
	}

	public UnitTraitDefinition UnitTraitDefinition
	{
		get
		{
			return traitDefinition;
		}
		set
		{
			if (traitDefinition != value)
			{
				traitDefinition = value;
			}
		}
	}

	public void OnJoystickSelect()
	{
		textImage.sprite = textHoveredSprite;
	}

	public void OnJoystickDeselect()
	{
		textImage.sprite = textInitSprite;
	}

	public void Refresh()
	{
		base.gameObject.SetActive(UnitTraitDefinition != null);
		if (UnitTraitDefinition == null)
		{
			return;
		}
		RefreshText();
		if (iconImage != null)
		{
			string text = ((!UnitTraitDefinition.IsBackgroundTrait) ? (PlayableUnitDatabase.UnitTraitTiersId.ContainsKey(UnitTraitDefinition.Cost) ? PlayableUnitDatabase.UnitTraitTiersId[UnitTraitDefinition.Cost] : "Default") : ("Background_" + (PlayableUnitDatabase.UnitBackgroundTraitTiersId.ContainsKey(UnitTraitDefinition.Cost) ? PlayableUnitDatabase.UnitBackgroundTraitTiersId[UnitTraitDefinition.Cost] : "Default")));
			iconImage.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Traits/Icons_Traits_" + UnitTraitDefinition.Id, failSilently: true);
			if (iconImage.sprite == null)
			{
				iconImage.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Traits/Icons_Traits_" + text);
			}
		}
	}

	private void Awake()
	{
		if (textImage != null)
		{
			textInitSprite = textImage.sprite;
		}
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
			RefreshText();
		}
	}

	private void RefreshText()
	{
		if (UnitTraitDefinition != null)
		{
			if (traitTitleText != null)
			{
				traitTitleText.text = UnitTraitDefinition.Name;
			}
			if (traitDescriptionText != null)
			{
				traitDescriptionText.text = UnitTraitDefinition.GetDescription();
			}
		}
	}

	private IEnumerator Start()
	{
		yield return GameManager.WaitForGameInit;
		Refresh();
	}
}
