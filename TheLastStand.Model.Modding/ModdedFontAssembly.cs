using System.IO;
using TPLib.Localization.Fonts;
using TheLastStand.Definition.Modding;

namespace TheLastStand.Model.Modding;

public class ModdedFontAssembly : FontAssembly
{
	public ModdedFontAssembly(FontAssemblyDefinition fontAssemblyDefinition, DirectoryInfo directory)
		: base(null)
	{
		base.Id = fontAssemblyDefinition.Id;
		for (int i = 0; i < fontAssemblyDefinition.FontAssetsDefinition.Count; i++)
		{
			FontAssets fontAssets = new FontAssets(fontAssemblyDefinition.FontAssetsDefinition[i].Importance, base.Id, directory, fontAssemblyDefinition.FontAssetsDefinition[i].FontPath);
			if (fontAssets.IsValid())
			{
				base.FontAssets.Add(fontAssets);
			}
		}
	}
}
