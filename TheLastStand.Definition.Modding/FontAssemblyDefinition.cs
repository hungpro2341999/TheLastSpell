using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding;

public class FontAssemblyDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class Constants
	{
		public const string IdAttributeName = "id";

		public const string FontAssetElementName = "FontAsset";
	}

	private string id = string.Empty;

	private List<FontAssetsDefinition> fontAssetsDefinitions = new List<FontAssetsDefinition>();

	public string Id => id;

	public List<FontAssetsDefinition> FontAssetsDefinition => fontAssetsDefinitions;

	public FontAssemblyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("id");
		if (xAttribute != null)
		{
			id = xAttribute.Value;
		}
		foreach (XElement item in obj.Elements("FontAsset"))
		{
			FontAssetsDefinition fontAssetsDefinition = new FontAssetsDefinition(item);
			if (fontAssetsDefinition.IsDefinitionValid())
			{
				fontAssetsDefinitions.Add(fontAssetsDefinition);
			}
			else
			{
				TPSingleton<FontManager>.Instance.LogError("A FontAsset definition isn't valid, the FontAssembly id is : " + id);
			}
		}
	}

	public bool IsValidDefinition()
	{
		return fontAssetsDefinitions.Count > 0;
	}
}
