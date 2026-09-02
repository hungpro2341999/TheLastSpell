using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Modding.ModuleConfig;

public class FontConfigDefinition : ModuleConfigDefinition
{
	private class FontConfigConstants
	{
		public const string FontAssembliesElementName = "FontAssemblies";

		public const string UseFontAssemblyInModListElementName = "UseFontAssemblyInModList";

		public const string FontAssemblyElementName = "FontAssembly";

		public const string IdAttributeName = "id";
	}

	public List<FontAssemblyDefinition> FontPackDefinitions { get; } = new List<FontAssemblyDefinition>();

	public bool UseFontAssemblyInModList { get; private set; }

	public string FontAssemblyToUseInModList { get; private set; }

	public FontConfigDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("FontAssemblies");
		XElement xElement2 = obj.Element("UseFontAssemblyInModList");
		foreach (XElement item in xElement.Elements("FontAssembly"))
		{
			FontAssemblyDefinition fontAssemblyDefinition = new FontAssemblyDefinition(item);
			if (fontAssemblyDefinition.IsValidDefinition())
			{
				FontPackDefinitions.Add(fontAssemblyDefinition);
			}
		}
		UseFontAssemblyInModList = xElement2 != null;
		if (UseFontAssemblyInModList)
		{
			XAttribute xAttribute = xElement2.Attribute("id");
			if (xAttribute != null)
			{
				FontAssemblyToUseInModList = xAttribute.Value;
			}
			else
			{
				FontAssemblyToUseInModList = FontPackDefinitions[0].Id;
			}
		}
	}
}
