using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Steamworks;
using TPLib;
using TPLib.Localization.ScriptableObjects;
using TheLastStand.Controller.Modding;
using TheLastStand.Controller.Modding.Module;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding.Module;

namespace TheLastStand.Model.Modding;

public class Mod
{
	public enum E_ModLocation
	{
		LOCAL,
		STEAM
	}

	public class Constants
	{
		public const string Author = "Author";

		public const string ManifestName = "manifest.xml";

		public const string Description = "Description";

		public const string OverrideSteamDatas = "OverrideSteamDatas";

		public const string ThumbnailName = "thumbnail";

		public const string Title = "Title";

		public const string Version = "Version";

		public const long MaxThumbnailSize = 1000000L;

		public const string PngExtension = ".png";

		public const string JpgExtension = ".jpg";

		public const string JpegExtension = ".jpeg";

		public const string GifExtension = ".gif";
	}

	private FileInfo steamWorkshopFileInfo;

	public string Id => Title + " (" + (HasAWorkshopId ? $"Steam Workshop Id: {WorkshopId}" : "Local") + ")";

	public string Author { get; private set; }

	public string Description { get; private set; }

	public DirectoryInfo DirectoryInfo { get; }

	public bool HasAWorkshopId { get; private set; }

	public bool HasManifest => ManifestElement != null;

	public SteamUGCDetails_t? Item { get; }

	public bool IsIncompatible { get; set; }

	public bool IsUsed { get; set; } = true;

	public bool IsSaveBlocking { get; private set; }

	public E_ModLocation Location { get; }

	public XElement ManifestElement { get; private set; }

	public ModController ModController { get; }

	public bool MustOverrideSteamDatas { get; private set; }

	public List<ModdingModule> Modules { get; } = new List<ModdingModule>();

	public FileInfo Thumbnail { get; private set; }

	public string Title { get; private set; }

	public uint Version { get; private set; }

	public ulong WorkshopId { get; private set; }

	public Mod(ModController controller, DirectoryInfo directoryInfo, SteamUGCDetails_t? steamItem)
	{
		ModController = controller;
		Item = steamItem;
		Location = (Item.HasValue ? E_ModLocation.STEAM : E_ModLocation.LOCAL);
		if (directoryInfo != null)
		{
			DirectoryInfo = directoryInfo;
			FileInfo[] files = directoryInfo.GetFiles();
			if (files != null && files.Length != 0)
			{
				RetrieveManifestFile(files);
			}
			Init();
			if (Version >= ModManager.ModMinVersion)
			{
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				if (directories != null && directories.Length != 0)
				{
					RetrieveModules(directories);
				}
			}
		}
		DeserializeSteamWorkshopId();
	}

	public void Init()
	{
		DeserializeAuthor();
		DeserializeDescription();
		DeserializeTitle();
		DeserializeOverrideSteamDatas();
		DeserializeThumbnailFileInfo();
		DeserializeVersion();
	}

	public string ModulesToString()
	{
		string text = "\r\n";
		for (int num = Modules.Count - 1; num >= 0; num--)
		{
			text = text + Modules[num].ModuleController.ToString() + "\r\n";
		}
		return text;
	}

	public override string ToString()
	{
		return Title + (HasAWorkshopId ? $" (Steam Workshop Id: {WorkshopId})" : string.Empty);
	}

	private void RetrieveManifestFile(FileInfo[] fileInfos)
	{
		FileInfo fileInfo = fileInfos.FirstOrDefault((FileInfo x) => x.Name.ToLower() == "manifest.xml");
		if (fileInfo != null)
		{
			FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
			ManifestElement = XElement.Load(stream);
		}
	}

	private void RetrieveModules(DirectoryInfo[] modulesDirectories)
	{
		for (int i = 0; i < modulesDirectories.Length; i++)
		{
			ModdingModule moddingModule = null;
			switch (modulesDirectories[i].Name)
			{
			case "Perks":
				moddingModule = new PerksModuleController(modulesDirectories[i]).PerksModule;
				break;
			case "Localization":
				moddingModule = new LocalizationModuleController(modulesDirectories[i]).LocalizationModule;
				break;
			case "Fonts":
				if (FontSettings.IsActivated)
				{
					moddingModule = new FontModuleController(modulesDirectories[i]).FontModule;
					if (moddingModule is FontModule { FontConfigDefinition: null })
					{
						moddingModule = null;
					}
				}
				break;
			}
			if (moddingModule != null)
			{
				IsSaveBlocking |= moddingModule.IsSaveBlocking;
				Modules.Add(moddingModule);
			}
		}
	}

	private void DeserializeAuthor()
	{
		if (string.IsNullOrEmpty(Author) && ManifestElement != null)
		{
			Author = ManifestElement.Element("Author")?.Value ?? string.Empty;
		}
	}

	private void DeserializeDescription()
	{
		if (!string.IsNullOrEmpty(Description))
		{
			return;
		}
		switch (Location)
		{
		case E_ModLocation.LOCAL:
			if (ManifestElement != null)
			{
				Description = ManifestElement.Element("Description")?.Value ?? string.Empty;
			}
			break;
		case E_ModLocation.STEAM:
			if (Item.HasValue && Item.HasValue)
			{
				Description = Item.Value.m_rgchDescription ?? string.Empty;
			}
			else if (ManifestElement != null)
			{
				Description = ManifestElement.Element("Description")?.Value ?? string.Empty;
			}
			break;
		}
	}

	private void DeserializeOverrideSteamDatas()
	{
		if (ManifestElement != null)
		{
			MustOverrideSteamDatas = ManifestElement.Attribute("OverrideSteamDatas")?.Value == "true";
		}
	}

	private void DeserializeSteamWorkshopId()
	{
		if (HasAWorkshopId)
		{
			return;
		}
		if (Location == E_ModLocation.STEAM && Item.HasValue && Item.HasValue)
		{
			WorkshopId = Item.Value.m_nPublishedFileId.m_PublishedFileId;
			HasAWorkshopId = true;
			return;
		}
		FileInfo[] files = DirectoryInfo.GetFiles();
		steamWorkshopFileInfo = files.FirstOrDefault((FileInfo x) => x.Name.ToLower() == "/steam_workshop.txt".Replace("/", string.Empty));
		if (steamWorkshopFileInfo == null)
		{
			return;
		}
		using StreamReader streamReader = new StreamReader(steamWorkshopFileInfo.FullName);
		string value = streamReader.ReadLine();
		try
		{
			WorkshopId = Convert.ToUInt64(value);
			HasAWorkshopId = true;
		}
		catch (Exception arg)
		{
			TPSingleton<ModManager>.Instance.Log($"Can't read steamworkshop id of this local mod : {Title}. Exception : {arg}");
		}
		streamReader.Close();
	}

	private void DeserializeThumbnailFileInfo()
	{
		if (DirectoryInfo == null)
		{
			return;
		}
		FileInfo[] files = DirectoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string[] array = fileInfo.Name.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0 && array[0] == "thumbnail")
			{
				Thumbnail = fileInfo;
				break;
			}
		}
	}

	private void DeserializeTitle()
	{
		if (!string.IsNullOrEmpty(Title))
		{
			return;
		}
		switch (Location)
		{
		case E_ModLocation.LOCAL:
			if (ManifestElement != null)
			{
				Title = ManifestElement.Element("Title")?.Value ?? string.Empty;
			}
			break;
		case E_ModLocation.STEAM:
			if (Item.HasValue && Item.HasValue)
			{
				Title = Item.Value.m_rgchTitle ?? string.Empty;
			}
			else if (ManifestElement != null)
			{
				Title = ManifestElement.Element("Title")?.Value ?? string.Empty;
			}
			break;
		}
	}

	private void DeserializeVersion()
	{
		if (ManifestElement == null)
		{
			return;
		}
		if (ManifestElement.Element("Version") != null)
		{
			if (uint.TryParse(ManifestElement.Element("Version")?.Value, out var result))
			{
				Version = result;
			}
		}
		else
		{
			Version = 1u;
		}
	}
}
