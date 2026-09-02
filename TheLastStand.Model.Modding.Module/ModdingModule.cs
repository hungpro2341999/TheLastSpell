using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Controller.Modding.Module;
using TheLastStand.Definition.Modding.ModuleConfig;
using TheLastStand.Manager.Modding;

namespace TheLastStand.Model.Modding.Module;

public abstract class ModdingModule
{
	public static class Constants
	{
		public const string ConfigElementName = "Config";
	}

	protected FileInfo configFile;

	protected DirectoryInfo directory;

	protected ModuleConfigDefinition ModuleConfigDefinition;

	public abstract bool IsSaveBlocking { get; }

	public virtual bool IsValid => true;

	public ModuleController ModuleController { get; }

	public ModdingModule(ModuleController moduleController, DirectoryInfo directory)
	{
		ModuleController = moduleController;
		this.directory = directory;
		RetrieveConfigDefinition();
		if (configFile == null)
		{
			return;
		}
		string text = string.Empty;
		using (StreamReader streamReader = configFile.OpenText())
		{
			text = streamReader.ReadToEnd();
		}
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				XElement element = XDocument.Parse(text, LoadOptions.SetBaseUri).Element("Config");
				ReadConfig(element);
			}
			catch (Exception arg)
			{
				TPSingleton<ModManager>.Instance.LogError($"Can't Read this config file : {configFile.FullName} ! (Exception : {arg})");
			}
		}
	}

	public abstract void ReadConfig(XElement element);

	protected virtual void RetrieveConfigDefinition()
	{
		FileInfo[] files = directory.GetFiles();
		if (files != null && files.Length != 0)
		{
			configFile = files.FirstOrDefault((FileInfo x) => x.Name == "config.xml");
		}
	}
}
