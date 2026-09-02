using TPLib;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Generic;

[RequireComponent(typeof(TMP_BetterDropdown))]
public class DropdownInputsListener : MonoBehaviour
{
	private TMP_BetterDropdown dropdown;

	public TMP_BetterDropdown Dropdown
	{
		get
		{
			if (dropdown == null)
			{
				dropdown = GetComponent<TMP_BetterDropdown>();
			}
			return dropdown;
		}
	}

	private void Hide()
	{
		Dropdown.Hide();
		if (TPSingleton<InputManager>.Exist())
		{
			InputManager.OnDropdownClose();
		}
	}

	private void OnDisable()
	{
		Dropdown.OnDropDownOpened.RemoveListener(OnDropdownOpen);
		Dropdown.OnDropDownClosed.RemoveListener(OnDropdownClose);
	}

	private void OnEnable()
	{
		Dropdown.OnDropDownOpened.AddListener(OnDropdownOpen);
		Dropdown.OnDropDownClosed.AddListener(OnDropdownClose);
	}

	private void OnDropdownOpen()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			InputManager.OnDropdownOpen();
		}
	}

	private void OnDropdownClose()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			InputManager.OnDropdownClose();
		}
	}

	private void Update()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			if (InputManager.GetButtonDown(63) && Dropdown.Displayed)
			{
				Hide();
			}
			if (InputManager.GetButtonDown(64) && Dropdown.Displayed)
			{
				Dropdown.SelectHoveredItem();
				Hide();
			}
		}
	}
}
