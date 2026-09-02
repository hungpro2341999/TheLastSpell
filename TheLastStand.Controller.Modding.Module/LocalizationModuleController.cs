using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding.Module;

namespace TheLastStand.Controller.Modding.Module;

public class LocalizationModuleController : ModuleController
{
	public class Constants
	{
		public const string FolderPath = "/Localization/";

		public const string CustomLocaSuffix = "[CUSTOM]";

		public const string TextExtension = ".txt";
	}

	private List<string> loadedLanguages = new List<string>();

	public LocalizationModule LocalizationModule => module as LocalizationModule;

	public LocalizationModuleController(DirectoryInfo directory)
		: base(directory)
	{
		module = new LocalizationModule(this, directory);
		LoadLanguages(directory);
	}

	public bool HasLinkedFontPack(string language)
	{
		if (LocalizationModule.LocalizationConfigDefinition.LanguageLinkedToFontPack != null)
		{
			return LocalizationModule.LocalizationConfigDefinition.LanguageLinkedToFontPack.ContainsKey(language);
		}
		return false;
	}

	public override string ToString()
	{
		string text = "<b>Localizaton Module</b> : \r\n   - Loaded Languages : \r\n";
		for (int i = 0; i < loadedLanguages.Count; i++)
		{
			text = text + "      * " + loadedLanguages[i] + "\r\n";
		}
		return text;
	}

	private void LoadLanguages(DirectoryInfo directory)
	{
		FileInfo[] array = null;
		try
		{
			array = directory.GetFiles();
		}
		catch (Exception message)
		{
			TPSingleton<ModManager>.Instance.LogError(message);
		}
		if (array == null)
		{
			return;
		}
		FileInfo[] array2 = array;
		foreach (FileInfo fileInfo in array2)
		{
			if (fileInfo.Name == "config.xml")
			{
				continue;
			}
			if (fileInfo.Extension == ".txt")
			{
				try
				{
					Localizer.LoadCSV(FileToByteArray(fileInfo), merge: true);
				}
				catch (Exception arg)
				{
					TPSingleton<ModManager>.Instance.LogError($"An unknown error occurred during the loading of a language ! Exception : {arg}");
				}
			}
			else
			{
				TPSingleton<ModManager>.Instance.Log("A localization file should be of type .txt ! (Wrong file name : " + fileInfo.Name + ")", CLogLevel.MAJOR);
			}
		}
	}

	private byte[] FileToByteArray(FileInfo fileInfo)
	{
		byte[] array = null;
		FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		long length = fileInfo.Length;
		array = binaryReader.ReadBytes((int)length);
		binaryReader.Close();
		fileStream.Close();
		string text = Encoding.Default.GetString(array);
		text = text.Replace("\\n", "\n");
		text = text.Replace("\\r", "\r");
		string[] array2 = text.Replace("\r", string.Empty).Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
		string oldValue = array2[0];
		string[] array3 = array2[0].Split(new string[1] { "," }, StringSplitOptions.None);
		if (array3.Length >= 2)
		{
			string text2 = array3[0] + ",";
			for (int i = 1; i < array3.Length; i++)
			{
				if (!string.IsNullOrEmpty(array3[i]) && !string.IsNullOrWhiteSpace(array3[i]))
				{
					if (!Localizer.knownLanguages.Contains(array3[i]))
					{
						loadedLanguages.Add(array3[i]);
						array3[i] += " [CUSTOM]";
					}
					text2 = text2 + array3[i] + ",";
				}
			}
			text = text.Replace(oldValue, text2);
			array = Encoding.Default.GetBytes(text);
		}
		return array;
	}

	public bool TryGetLinkedLanguages(string fontAssemblyId, out string[] languages)
	{
		List<string> list = new List<string>();
		if (LocalizationModule.LocalizationConfigDefinition?.LanguageLinkedToFontPack != null)
		{
			foreach (KeyValuePair<string, List<string>> item in LocalizationModule.LocalizationConfigDefinition.LanguageLinkedToFontPack)
			{
				if (item.Value.Contains(fontAssemblyId))
				{
					list.Add(item.Key);
				}
			}
			languages = list.ToArray();
			return languages.Length != 0;
		}
		languages = null;
		return false;
	}
}
