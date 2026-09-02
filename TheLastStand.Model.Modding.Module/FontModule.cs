using System.IO;
using System.Xml.Linq;
using TheLastStand.Controller.Modding.Module;
using TheLastStand.Definition.Modding.ModuleConfig;

namespace TheLastStand.Model.Modding.Module;

public class FontModule : ModdingModule
{
	public static class FontModuleConstants
	{
		public const string FolderName = "Fonts";
	}

	public override bool IsSaveBlocking => false;

	public FontConfigDefinition FontConfigDefinition => ModuleConfigDefinition as FontConfigDefinition;

	public FontModuleController FontModuleController => base.ModuleController as FontModuleController;

	public FontModule(FontModuleController fontModuleController, DirectoryInfo directory)
		: base(fontModuleController, directory)
	{
	}

	public override void ReadConfig(XElement element)
	{
		ModuleConfigDefinition = new FontConfigDefinition(element);
	}
}
