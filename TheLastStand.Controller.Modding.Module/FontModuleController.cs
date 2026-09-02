using System.Collections.Generic;
using System.IO;
using TPLib;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using TheLastStand.Model.Modding.Module;

namespace TheLastStand.Controller.Modding.Module;

public class FontModuleController : ModuleController
{
	public FontModule FontModule => module as FontModule;

	public List<ModdedFontAssembly> FontAssemblies { get; } = new List<ModdedFontAssembly>();

	public FontModuleController(DirectoryInfo directory)
		: base(directory)
	{
		module = new FontModule(this, directory);
		if (FontModule.FontConfigDefinition == null)
		{
			return;
		}
		for (int i = 0; i < FontModule.FontConfigDefinition.FontPackDefinitions.Count; i++)
		{
			ModdedFontAssembly moddedFontAssembly = new ModdedFontAssembly(FontModule.FontConfigDefinition.FontPackDefinitions[i], directory);
			if (moddedFontAssembly.FontAssets.Count > 0)
			{
				FontAssemblies.Add(moddedFontAssembly);
			}
			else
			{
				TPSingleton<ModManager>.Instance.LogError("This FontPack " + FontModule.FontConfigDefinition.FontPackDefinitions[i].Id + " can't be loaded due to inexistant font files !");
			}
		}
	}

	public override string ToString()
	{
		string text = "<b>Fonts Module</b> : \r\n   - Loaded FontPacks : \r\n";
		for (int i = 0; i < FontAssemblies.Count; i++)
		{
			text = text + "      " + $"* {FontAssemblies[i]}\r\n";
		}
		return text;
	}
}
