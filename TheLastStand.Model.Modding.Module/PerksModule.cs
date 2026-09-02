using System.IO;
using System.Xml.Linq;
using TheLastStand.Controller.Modding.Module;
using TheLastStand.Definition.Modding.ModuleConfig.Perks;

namespace TheLastStand.Model.Modding.Module;

public class PerksModule : ModdingModule
{
	public new static class Constants
	{
		public const string FolderName = "Perks";
	}

	public override bool IsSaveBlocking => true;

	public PerksModuleController PerksModuleController => base.ModuleController as PerksModuleController;

	public PerksModuleConfigDefinition PerksModuleConfigDefinition => ModuleConfigDefinition as PerksModuleConfigDefinition;

	public PerksModule(ModuleController moduleController, DirectoryInfo directory)
		: base(moduleController, directory)
	{
	}

	public override void ReadConfig(XElement element)
	{
		ModuleConfigDefinition = new PerksModuleConfigDefinition(element, directory);
	}
}
