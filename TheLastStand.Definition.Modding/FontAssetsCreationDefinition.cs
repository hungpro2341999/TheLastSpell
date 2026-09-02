using System;
using System.Xml.Linq;
using TMPro;
using TheLastStand.Framework.Serialization;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TheLastStand.Definition.Modding;

public class FontAssetsCreationDefinition : TheLastStand.Framework.Serialization.Definition
{
	private class Constants
	{
		public const string SamplingPointSizeElementName = "SamplingPointSize";

		public const string AtlasPaddingElementName = "AtlasPadding";

		public const string GlyphRenderModeElementName = "GlyphRenderMode";

		public const string AtlasSizeElementName = "AtlasSize";

		public const string AtlasPopulationModeElementName = "AtlasPopulationMode";

		public const string AtlasSizeXAttributeName = "x";

		public const string AtlasSizeYAttributeName = "y";
	}

	private int atlasPadding = 10;

	private AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.Dynamic;

	private Vector2Int atlasSize = new Vector2Int(1024, 1024);

	private GlyphRenderMode glyphRenderMode = GlyphRenderMode.SDFAA;

	private int samplingPointSize = 54;

	public int AtlasPadding => atlasPadding;

	public AtlasPopulationMode AtlasPopulationMode => atlasPopulationMode;

	public Vector2Int AtlasSize => atlasSize;

	public GlyphRenderMode GlyphRenderMode => glyphRenderMode;

	public int SamplingPointSize => samplingPointSize;

	public FontAssetsCreationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement obj = container as XElement;
		XElement xElement = obj.Element("GlyphRenderMode");
		XElement xElement2 = obj.Element("AtlasSize");
		XElement xElement3 = obj.Element("AtlasPopulationMode");
		if (xElement != null && Enum.TryParse<GlyphRenderMode>(xElement.Value, out var result))
		{
			glyphRenderMode = result;
		}
		if (xElement2 != null)
		{
			XAttribute xAttribute = xElement2.Attribute("x");
			XAttribute xAttribute2 = xElement2.Attribute("y");
			if (xAttribute != null && xAttribute2 != null && int.TryParse(xAttribute.Value, out var result2) && int.TryParse(xAttribute2.Value, out var result3))
			{
				atlasSize = new Vector2Int(result2, result3);
			}
		}
		if (xElement3 != null && Enum.TryParse<AtlasPopulationMode>(xElement3.Value, out var result4))
		{
			atlasPopulationMode = result4;
		}
	}
}
