using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Dev;

[RequireComponent(typeof(InputField))]
public class EnterSubmitInputField : MonoBehaviour
{
	[Serializable]
	public class InteractionEvent : UnityEvent<string>
	{
	}

	private InputField inputField;

	[SerializeField]
	private InteractionEvent onInteraction = new InteractionEvent();

	private bool wasFocused;

	private void Awake()
	{
		inputField = GetComponent<InputField>();
	}

	private void Update()
	{
		if (wasFocused && inputField.text != string.Empty && (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
		{
			onInteraction.Invoke(inputField.text);
			inputField.text = string.Empty;
			inputField.Select();
			inputField.ActivateInputField();
		}
		wasFocused = inputField.isFocused;
	}
}
