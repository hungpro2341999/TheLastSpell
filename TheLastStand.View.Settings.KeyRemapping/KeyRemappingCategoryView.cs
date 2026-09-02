using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingCategoryView : MonoBehaviour
{
	private static class Constants
	{
		public const string CategoryIconResourcesPathPrefix = "View/Sprites/UI/KeyRemapping/KeyRemapping_Category_";

		public const string GameObjectNamePrefix = "Category Panel - ";
	}

	[SerializeField]
	private KeyRemappingBindingLineView bindingLineView;

	[SerializeField]
	private TextMeshProUGUI categoryNameText;

	[SerializeField]
	private LocalizedFont categoryNameLocalizedFont;

	[SerializeField]
	private Image categoryIcon;

	[SerializeField]
	private RectTransform bindingLinesContainer;

	private string categoryName;

	private readonly List<KeyRemappingBindingLineView> linesViews = new List<KeyRemappingBindingLineView>();

	public RectTransform BindingLinesContainer => bindingLinesContainer;

	public void Initialize(InputCategory category)
	{
		categoryName = category.name;
		base.transform.name = "Category Panel - " + categoryName;
		categoryIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/KeyRemapping/KeyRemapping_Category_" + categoryName);
		categoryIcon.SetNativeSize();
		RefreshTexts();
	}

	public KeyRemappingBindingLineView InstantiateInputBindingLine()
	{
		KeyRemappingBindingLineView keyRemappingBindingLineView = Object.Instantiate(bindingLineView, BindingLinesContainer);
		linesViews.Add(keyRemappingBindingLineView);
		return keyRemappingBindingLineView;
	}

	public void Refresh()
	{
		foreach (KeyRemappingBindingLineView linesView in linesViews)
		{
			linesView.Refresh();
		}
	}

	public void RefreshTexts()
	{
		categoryNameText.text = Localizer.Get("KeyRemapping_CategoryName_" + categoryName);
		if (categoryNameLocalizedFont != null)
		{
			categoryNameLocalizedFont.RefreshFont();
		}
		foreach (KeyRemappingBindingLineView linesView in linesViews)
		{
			linesView.RefreshTexts();
		}
	}
}
