using System;
using System.Text;
using TPLib;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.Dev;

public class ColorSwapHandlerUI : MonoBehaviour
{
	private static class Constants
	{
		public static class Xml
		{
			public const string ColorSwapPaletteTag = "ColorSwapPalette";

			public const string ColorSwapTag = "ColorSwap";

			public const string IndexTag = "Index";

			public const string OutputColorTag = "OutputColor";
		}
	}

	[Serializable]
	private class ColorSwap
	{
		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private int index;

		public Color Color => color;

		public int Index => index;
	}

	[SerializeField]
	[FormerlySerializedAs("imageRenderer")]
	private Image imageRenderer;

	private Texture2D colorSwapTex;

	private Color[] spriteColors;

	[SerializeField]
	[FormerlySerializedAs("DBG_colorSwaps")]
	private ColorSwap[] debugColorSwaps;

	[ContextMenu("Init Swap Texture")]
	public void InitColorSwapTex()
	{
		Texture2D texture2D = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
		{
			filterMode = FilterMode.Point
		};
		for (int i = 0; i < texture2D.width; i++)
		{
			texture2D.SetPixel(i, 0, new Color(0f, 0f, 0f, 0f));
		}
		texture2D.Apply();
		imageRenderer.material.SetTexture("_SwapTex", texture2D);
		colorSwapTex = texture2D;
	}

	public void SwapColor(int index, Color color)
	{
		colorSwapTex.SetPixel(index, 0, color);
	}

	private void Awake()
	{
		if (imageRenderer == null)
		{
			imageRenderer = GetComponent<Image>();
		}
	}

	private void Start()
	{
		InitColorSwapTex();
		SwapColorsInstant();
	}

	[ContextMenu("Swap Colors")]
	public void SwapColorsInstant()
	{
		if (debugColorSwaps != null)
		{
			for (int i = 0; i < debugColorSwaps.Length; i++)
			{
				SwapColor(debugColorSwaps[i].Index, debugColorSwaps[i].Color);
			}
			colorSwapTex.Apply();
		}
	}

	[ContextMenu("Print XML Swap Palette")]
	public void PrintPalette()
	{
		if (debugColorSwaps != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("<ColorSwapPalette Id=\"ToBeDefined\">");
			for (int i = 0; i < debugColorSwaps.Length; i++)
			{
				AddPrintableXmlColorSwap(debugColorSwaps[i], ref stringBuilder);
			}
			stringBuilder.AppendLine("</ColorSwapPalette>");
			TPDebug.Log("\n" + stringBuilder.ToString(), this);
		}
	}

	private void AddPrintableXmlColorSwap(ColorSwap colorSwap, ref StringBuilder stringBuilder)
	{
		if (colorSwap != null)
		{
			stringBuilder.AppendLine("\t<ColorSwap>");
			stringBuilder.AppendLine(string.Format("\t\t<{0}>{1}</{2}>", "Index", colorSwap.Index, "Index"));
			stringBuilder.AppendLine("\t\t<OutputColor>#" + ColorUtility.ToHtmlStringRGBA(colorSwap.Color) + "</OutputColor>");
			stringBuilder.AppendLine("\t</ColorSwap>");
		}
	}
}
