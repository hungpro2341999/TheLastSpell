using TMPro;
using TPLib;
using TheLastStand.Manager.LevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class ColorPicker : MonoBehaviour
{
	public enum PickType
	{
		Hover,
		Click,
		Drag
	}

	[SerializeField]
	private RectTransform colorPickerRect;

	[SerializeField]
	private Image colorPickerImage;

	[SerializeField]
	private Image previewImage;

	[SerializeField]
	private TextMeshProUGUI colorHexText;

	[SerializeField]
	private Button hexToClipboardButton;

	[Tooltip("Defines how the color should be updated by user input.")]
	[SerializeField]
	private PickType pickType = PickType.Drag;

	[Tooltip("Does not take fully transparent pixels into account.")]
	[SerializeField]
	private bool ignoreTransparent = true;

	[Tooltip("Only pick the tint and always set the color as opaque.")]
	[SerializeField]
	private bool ignoreAlpha = true;

	[SerializeField]
	private ColorEvent onColorHovered = new ColorEvent();

	[SerializeField]
	private ColorEvent onColorClicked = new ColorEvent();

	[SerializeField]
	private Color hoveredColorPreview = Color.white;

	[SerializeField]
	private Color clickedColorPreview = Color.white;

	private bool disablePreview = true;

	private Texture2D colorPickerTexture;

	private Color lastPickedColor;

	private bool previousMousePressed;

	private bool mousePressed;

	public ColorEvent OnColorHovered => onColorHovered;

	public ColorEvent OnColorClicked => onColorClicked;

	public string LastPickedColorToHtmlString
	{
		get
		{
			if (!ignoreAlpha)
			{
				return ColorUtility.ToHtmlStringRGBA(lastPickedColor);
			}
			return ColorUtility.ToHtmlStringRGB(lastPickedColor);
		}
	}

	private void PickColor(Color color)
	{
		onColorClicked?.Invoke(color);
		clickedColorPreview = color;
		lastPickedColor = color;
		if (previewImage != null)
		{
			previewImage.color = color;
		}
		if (colorHexText != null)
		{
			colorHexText.text = LastPickedColorToHtmlString;
		}
	}

	private void UpdateColor()
	{
		if (!RectTransformUtility.RectangleContainsScreenPoint(colorPickerRect, Input.mousePosition))
		{
			return;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(colorPickerRect, Input.mousePosition, null, out var localPoint);
		float width = colorPickerRect.rect.width;
		float height = colorPickerRect.rect.height;
		localPoint += colorPickerRect.rect.size * 0.5f;
		float num = localPoint.x / width;
		float num2 = localPoint.y / height;
		Color color = colorPickerTexture.GetPixel(Mathf.RoundToInt(num * (float)colorPickerTexture.width), Mathf.RoundToInt(num2 * (float)colorPickerTexture.height));
		if (ignoreTransparent && color.a == 0f)
		{
			return;
		}
		if (ignoreAlpha && color.a < 1f)
		{
			color = new Color(color.r, color.g, color.b, 1f);
		}
		onColorHovered?.Invoke(color);
		hoveredColorPreview = color;
		bool flag = lastPickedColor != color;
		if (!flag)
		{
			return;
		}
		switch (pickType)
		{
		case PickType.Click:
			if (mousePressed && !previousMousePressed)
			{
				PickColor(color);
			}
			break;
		case PickType.Hover:
			if (flag)
			{
				PickColor(color);
			}
			break;
		case PickType.Drag:
			if (flag && mousePressed)
			{
				PickColor(color);
			}
			break;
		default:
			TPSingleton<LevelEditorManager>.Instance.LogError($"Unhandled color pick type {pickType}.");
			break;
		}
	}

	private void CopyPickedColorHexToClipboard()
	{
		GUIUtility.systemCopyBuffer = LastPickedColorToHtmlString;
	}

	private void Start()
	{
		colorPickerTexture = colorPickerImage.sprite.texture;
		if (hexToClipboardButton != null)
		{
			hexToClipboardButton.onClick.AddListener(CopyPickedColorHexToClipboard);
		}
	}

	private void Update()
	{
		previousMousePressed = mousePressed;
		mousePressed = Input.GetMouseButton(0);
		UpdateColor();
	}

	private void OnDestroy()
	{
		if (hexToClipboardButton != null)
		{
			hexToClipboardButton.onClick.RemoveListener(CopyPickedColorHexToClipboard);
		}
	}
}
