using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Localization.ScriptableObjects;

namespace TheLastStand.Definition.Modding.ModuleConfig;

public class LocalizationConfigDefinition : ModuleConfigDefinition
{
	private class LocalizationConfigConstants
	{
		public const string FontAssembliesBindingsElementName = "FontAssembliesBindings";

		public const string LanguagesAndFontAssembliesElementName = "LanguagesAndFontAssemblies";

		public const string LanguageIdsElementName = "LanguageIds";

		public const string FontAssemblyIdsElementName = "FontAssemblyIds";

		public const string FontAssemblyIdElementName = "FontAssemblyId";

		public const string CustomLanguageFormat = "{0} [CUSTOM]";
	}

	private Dictionary<string, List<string>> languagesLinkedToFontAssemblies = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> LanguageLinkedToFontPack => languagesLinkedToFontAssemblies;

	public LocalizationConfigDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = (container as XElement).Element("FontAssembliesBindings");
		if (xElement == null)
		{
			return;
		}
		foreach (XElement item in xElement.Elements("LanguagesAndFontAssemblies"))
		{
			XElement xElement2 = item.Element("LanguageIds");
			XElement xElement3 = item.Element(FontSettings.LanguageCanSupportsMultipleFontAssembly ? "FontAssemblyIds" : "FontAssemblyId");
			string[] array = RetrieveStrings(xElement2.Value);
			string[] array2 = RetrieveStrings(xElement3.Value);
			for (int i = 0; i < array.Length; i++)
			{
				string language = array[i];
				if (Localizer.knownLanguages.FirstOrDefault((string x) => x == language) == null)
				{
					language = $"{language} [CUSTOM]";
					if (!languagesLinkedToFontAssemblies.ContainsKey(language))
					{
						languagesLinkedToFontAssemblies.Add(language, new List<string>());
					}
				}
				else if (!languagesLinkedToFontAssemblies.ContainsKey(language))
				{
					languagesLinkedToFontAssemblies.Add(language, new List<string>());
				}
				for (int num = 0; num < array2.Length; num++)
				{
					if (!languagesLinkedToFontAssemblies[language].Contains(array2[num]))
					{
						languagesLinkedToFontAssemblies[language].Add(array2[num]);
					}
				}
			}
		}
	}

	private string[] RetrieveStrings(string value)
	{
		string[] array = value.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Replace("\n", string.Empty);
			array[i] = array[i].Replace("\r", string.Empty);
			array[i] = array[i].Replace("\t", string.Empty);
			array[i] = array[i].Trim();
		}
		if (array.Length > 1 && FontSettings.LanguageCanSupportsMultipleFontAssembly)
		{
			TPSingleton<FontManager>.Instance.Log("LanguagesAndFontAssemblies can't supports multiple FontAssembblies !");
			Array.Resize(ref array, 1);
		}
		return array;
	}
}
