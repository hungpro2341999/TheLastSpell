using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using PortraitAPI;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class AppearanceToggle : SerializedMonoBehaviour
{
	[Serializable]
	public class ToggleState
	{
		[SerializeField]
		private BetterButton button;

		[SerializeField]
		private Vector2 selectorPosition = Vector2.zero;

		public BetterButton Button => button;

		public Vector2 SelectorPosition => selectorPosition;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct GenderComparer : IEqualityComparer<Commons.E_Gender>
	{
		public bool Equals(Commons.E_Gender x, Commons.E_Gender y)
		{
			return x == y;
		}

		public int GetHashCode(Commons.E_Gender obj)
		{
			return (int)obj;
		}
	}

	[SerializeField]
	private Dictionary<Commons.E_Gender, ToggleState> statesByGender = new Dictionary<Commons.E_Gender, ToggleState>(default(GenderComparer));

	[SerializeField]
	private RectTransform selectorRectTransform;

	[SerializeField]
	private Color highlightedColor = Color.black;

	[SerializeField]
	private Color normalColor = Color.black;

	public GenderEvent OnAppearanceChanged = new GenderEvent();

	public Commons.E_Gender CurrentGender { get; private set; }

	public void SelectState(ToggleState value)
	{
		if (CurrentGender == statesByGender.First((KeyValuePair<Commons.E_Gender, ToggleState> x) => x.Value == value).Key)
		{
			return;
		}
		selectorRectTransform.DOAnchorPos(value.SelectorPosition, 0.2f);
		CurrentGender = statesByGender.First((KeyValuePair<Commons.E_Gender, ToggleState> x) => x.Value == value).Key;
		OnAppearanceChanged?.Invoke(CurrentGender);
		foreach (KeyValuePair<Commons.E_Gender, ToggleState> item in statesByGender)
		{
			Unhighlight(item.Value);
		}
	}

	public void SelectStateWithoutNotify(ToggleState value)
	{
		if (CurrentGender == statesByGender.First((KeyValuePair<Commons.E_Gender, ToggleState> x) => x.Value == value).Key)
		{
			return;
		}
		selectorRectTransform.DOAnchorPos(value.SelectorPosition, 0.2f);
		CurrentGender = statesByGender.First((KeyValuePair<Commons.E_Gender, ToggleState> x) => x.Value == value).Key;
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender = CurrentGender;
		foreach (KeyValuePair<Commons.E_Gender, ToggleState> item in statesByGender)
		{
			Unhighlight(item.Value);
		}
	}

	public void SelectState(Commons.E_Gender e_Gender)
	{
		if (statesByGender.ContainsKey(e_Gender) && e_Gender != CurrentGender)
		{
			SelectState(statesByGender[e_Gender]);
		}
	}

	public void SelectStateWithoutNotify(Commons.E_Gender gender)
	{
		if (statesByGender.ContainsKey(gender) && gender != CurrentGender)
		{
			SelectStateWithoutNotify(statesByGender[gender]);
		}
	}

	private void Start()
	{
		foreach (KeyValuePair<Commons.E_Gender, ToggleState> item in statesByGender)
		{
			item.Value.Button.OnPointerEnterEvent.AddListener(delegate
			{
				Highlight(item.Value);
			});
			item.Value.Button.OnPointerExitEvent.AddListener(delegate
			{
				Unhighlight(item.Value);
			});
			item.Value.Button.onClick.AddListener(delegate
			{
				SelectState(item.Value);
			});
		}
		CurrentGender = Commons.E_Gender.Woman;
		foreach (KeyValuePair<Commons.E_Gender, ToggleState> item2 in statesByGender)
		{
			if (item2.Key != CurrentGender)
			{
				Unhighlight(item2.Value);
			}
		}
	}

	private void Unhighlight(ToggleState value)
	{
		TextMeshProUGUI component = value.Button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		KeyValuePair<Commons.E_Gender, ToggleState> keyValuePair = statesByGender.First((KeyValuePair<Commons.E_Gender, ToggleState> x) => x.Value == value);
		component.color = ((CurrentGender == keyValuePair.Key) ? normalColor : new Color(normalColor.r, normalColor.g, normalColor.b, 0.35f));
	}

	private void Highlight(ToggleState value)
	{
		TextMeshProUGUI component = value.Button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		KeyValuePair<Commons.E_Gender, ToggleState> keyValuePair = statesByGender.First((KeyValuePair<Commons.E_Gender, ToggleState> x) => x.Value == value);
		if (CurrentGender != keyValuePair.Key)
		{
			component.color = highlightedColor;
		}
	}
}
