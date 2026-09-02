using System.IO;
using System.Xml.Linq;
using TheLastStand.Controller.Modding.Module;
using TheLastStand.Definition.Modding.ModuleConfig;

namespace TheLastStand.Model.Modding.Module;

public class LocalizationModule : ModdingModule
{
	public static class LocalizationModuleConstants
	{
		public const string FolderName = "Localization";
	}

	public override bool IsSaveBlocking => false;

	public override bool IsValid => LocalizationConfigDefinition?.LanguageLinkedToFontPack != null;

	public LocalizationConfigDefinition LocalizationConfigDefinition => ModuleConfigDefinition as LocalizationConfigDefinition;

	public LocalizationModuleController LocalizationModuleController => base.ModuleController as LocalizationModuleController;

	public LocalizationModule(LocalizationModuleController localizationModuleController, DirectoryInfo directory)
		: base(localizationModuleController, directory)
	{
	}

	public override void ReadConfig(XElement element)
	{
		ModuleConfigDefinition = new LocalizationConfigDefinition(element);
	}
}
