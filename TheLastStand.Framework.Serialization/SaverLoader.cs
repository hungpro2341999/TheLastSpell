using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.DLC;
using TheLastStand.Framework.Encryption;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Framework.Serialization;

public static class SaverLoader
{
	public enum E_SaveFormat
	{
		Xml,
		Raw
	}

	public static class Constants
	{
		public static class Debug
		{
			public const string LogBar = "===============================[ LOGBAR ]===============================";
		}
	}

	public class SaveLoadingFailedException : Exception
	{
		public string FilePath { get; private set; }

		public new string Message { get; private set; }

		public SaveLoadingFailedException(string filePath, string message, bool shouldMarkAsCorrupted)
			: this(filePath, shouldMarkAsCorrupted)
		{
			Message = message;
		}

		public SaveLoadingFailedException(string filePath, bool shouldMarkAsCorrupted)
		{
			if (File.Exists(filePath))
			{
				FilePath = (shouldMarkAsCorrupted ? MarkFileAsCorrupted(filePath) : filePath);
				return;
			}
			CLoggerManager.Log($"Threw a {GetType()} exception without supplying an existing save path (file {filePath} does NOT exist!)", LogType.Error);
			FilePath = filePath;
		}
	}

	public class FileDoesNotExistException : SaveLoadingFailedException
	{
		public FileDoesNotExistException(string filePath, string message, bool shouldMarkAsCorrupted)
			: base(filePath, message, shouldMarkAsCorrupted)
		{
		}

		public FileDoesNotExistException(string filePath, bool shouldMarkAsCorrupted)
			: base(filePath, shouldMarkAsCorrupted)
		{
		}
	}

	public class SerializedContainerLoadingInfo<T> where T : SerializedContainer
	{
		public T LoadedContainer;

		public (SaveManager.E_BrokenSaveReason? Reason, Exception Exception)[] FailedLoadsInfo;
	}

	public class WrongSaveVersionException : SaveLoadingFailedException
	{
		public WrongSaveVersionException(string filePath, bool shouldMarkAsCorrupted)
			: base(filePath, shouldMarkAsCorrupted)
		{
		}
	}

	public class MissingModException : SaveLoadingFailedException
	{
		public readonly HashSet<string> MissingModIds;

		public MissingModException(string filePath, bool shouldMarkAsCorrupted, HashSet<string> missingModIds)
			: base(filePath, shouldMarkAsCorrupted)
		{
			MissingModIds = missingModIds;
		}
	}

	public class MissingDLCException : SaveLoadingFailedException
	{
		public readonly HashSet<string> MissingDlcIds;

		public MissingDLCException(string filePath, bool shouldMarkAsCorrupted, HashSet<string> missingDlcIds)
			: base(filePath, shouldMarkAsCorrupted)
		{
			MissingDlcIds = missingDlcIds;
		}

		public List<string> GetLocalizedMissingDLCs()
		{
			List<string> list = new List<string>();
			foreach (string missingDlcId in MissingDlcIds)
			{
				DLCDefinition dLCFromId = TPSingleton<DLCManager>.Instance.GetDLCFromId(missingDlcId);
				if (dLCFromId != null)
				{
					list.Add(dLCFromId.LocalizedName);
				}
			}
			return list;
		}
	}

	private static readonly object SaveLock = new object();

	private static Dictionary<E_SaveType, Queue<SaveInfo>> saveQueues = new Dictionary<E_SaveType, Queue<SaveInfo>>(default(SaveTypeComparer))
	{
		{
			E_SaveType.App,
			new Queue<SaveInfo>()
		},
		{
			E_SaveType.Game,
			new Queue<SaveInfo>()
		},
		{
			E_SaveType.Settings,
			new Queue<SaveInfo>()
		}
	};

	public static Dictionary<string, XmlElementEventHandler> UnknownElementEvents = new Dictionary<string, XmlElementEventHandler>();

	public static event Action OnGameSavingStarts;

	public static event Action<bool> OnGameSavingEnds;

	public static bool AreSavesCompleted()
	{
		foreach (KeyValuePair<E_SaveType, Queue<SaveInfo>> saveQueue in saveQueues)
		{
			if (saveQueue.Value.Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static string MarkFileAsCorrupted(string filePath)
	{
		if (File.Exists(filePath))
		{
			string path = Path.GetFileNameWithoutExtension(filePath) + "__" + DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + Path.GetExtension(filePath);
			string text = Path.Combine(Path.GetDirectoryName(filePath), "Broken", path);
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			File.Move(filePath, text);
			return text;
		}
		return filePath;
	}

	public static string CopyDirectoryTo(string directoryPath, string destinationPath, bool recursive = true)
	{
		if (Directory.Exists(directoryPath))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
			FileInfo[] files = directoryInfo.GetFiles();
			string name = directoryInfo.Name;
			string text = Path.Combine(destinationPath, name);
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			for (int i = 0; i < files.Length; i++)
			{
				CopyFileTo(files[i].FullName, Path.Combine(destinationPath, name, files[i].Name));
			}
			if (recursive)
			{
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				for (int j = 0; j < directories.Length; j++)
				{
					CopyDirectoryTo(directories[j].FullName, text, recursive);
				}
			}
			return text;
		}
		return directoryPath;
	}

	public static string CopyFileTo(string filePath, string copyPath)
	{
		if (File.Exists(filePath))
		{
			if (File.Exists(copyPath))
			{
				File.Delete(copyPath);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(copyPath));
			File.Copy(filePath, copyPath);
			return copyPath;
		}
		return filePath;
	}

	public static bool SafeCopyFileTo(string oldPath, string newPath)
	{
		try
		{
			CopyFileTo(oldPath, newPath);
			CLoggerManager.Log("File(" + oldPath + ") succesfully copied to : " + newPath, LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return true;
		}
		catch (Exception arg)
		{
			CLoggerManager.Log($"An error occured during the copy of this file : {oldPath} ! Error : {arg}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return false;
		}
	}

	public static bool SafeCopyDirectoryTo(string oldPath, string newPath)
	{
		try
		{
			CopyDirectoryTo(oldPath, newPath);
			CLoggerManager.Log("Directory(" + oldPath + ") succesfully copied to : " + newPath, LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return true;
		}
		catch (Exception arg)
		{
			CLoggerManager.Log($"An error occured during the copy of this directory : {oldPath} ! Error : {arg}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return false;
		}
	}

	public static bool SafeDeleteDirectory(string path, bool recursive = true)
	{
		try
		{
			Directory.Delete(path, recursive);
			CLoggerManager.Log("Directory(" + path + ") has been deleted !", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return true;
		}
		catch (Exception arg)
		{
			CLoggerManager.Log($"An error occured during the deletion of this directory : {path} ! Error : {arg}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return false;
		}
	}

	public static bool SafeDeleteFile(string path)
	{
		try
		{
			File.Delete(path);
			CLoggerManager.Log("File(" + path + ") has been deleted !", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return true;
		}
		catch (Exception arg)
		{
			CLoggerManager.Log($"An error occured during the deletion of this file : {path} ! Error : {arg}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			return false;
		}
	}

	public static void Erase(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	public static T Load<T>(string filePath, bool useEncryption, bool shouldMarkAsCorrupted = true) where T : SerializedContainer
	{
		if (!File.Exists(filePath))
		{
			throw new FileDoesNotExistException(filePath, "Someone tried to load serialized data from " + filePath + " but there is NOTHING there!", shouldMarkAsCorrupted);
		}
		T val;
		using (FileStream fileStream = File.OpenRead(filePath))
		{
			if (useEncryption)
			{
				byte[] buffer;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					try
					{
						SaveEncoder.Decode(fileStream, memoryStream);
					}
					catch (SaveEncoder.HashMismatchException arg)
					{
						fileStream.Dispose();
						throw new SaveLoadingFailedException(filePath, $"The save file stored at {filePath} has been tampered with, and therefore will not be loaded.\n{arg}", shouldMarkAsCorrupted);
					}
					buffer = memoryStream.ToArray();
				}
				using MemoryStream stream = new MemoryStream(buffer);
				try
				{
					val = LoadInternal<T>(stream);
				}
				catch (InvalidOperationException arg2)
				{
					fileStream.Dispose();
					throw new SaveLoadingFailedException(filePath, $"Someone tried to load serialized data from {filePath} but the serialized file doesn't have the right structure\n{arg2}", shouldMarkAsCorrupted);
				}
				catch (Exception ex)
				{
					fileStream.Dispose();
					if (ex is SaveLoadingFailedException ex2)
					{
						throw ex2;
					}
					throw new SaveLoadingFailedException(filePath, $"Someone tried to load serialized data from {filePath} but an error occured.\n{ex}", shouldMarkAsCorrupted);
				}
			}
			else
			{
				try
				{
					val = LoadInternal<T>(fileStream);
				}
				catch (InvalidOperationException arg3)
				{
					fileStream.Dispose();
					throw new SaveLoadingFailedException(filePath, $"Someone tried to load serialized data from {filePath} but the serialized file doesn't have the right structure\n{arg3}", shouldMarkAsCorrupted);
				}
				catch (Exception ex3)
				{
					fileStream.Dispose();
					if (ex3 is SaveLoadingFailedException ex4)
					{
						throw ex4;
					}
					throw new SaveLoadingFailedException(filePath, $"Someone tried to load serialized data from {filePath} but an error occured.\n{ex3}", shouldMarkAsCorrupted);
				}
			}
		}
		if (val == null)
		{
			throw new SaveLoadingFailedException(filePath, "The deserialization of " + filePath + " lead to a game state FAILED!", shouldMarkAsCorrupted);
		}
		return val;
	}

	public static T SafeLoad<T>(string filePath, bool useEncryption, out Exception caughtException, int minVersion = -1) where T : SerializedContainer
	{
		try
		{
			caughtException = null;
			T val = Load<T>(filePath, useEncryption, shouldMarkAsCorrupted: false);
			if (val is SerializedGameState serializedGameState)
			{
				HashSet<string> hashSet = new HashSet<string>();
				foreach (string modId in serializedGameState.ModsInUse)
				{
					if (!ModManager.SubscribedMods.Any((Mod x) => x.IsUsed && x.Id == modId))
					{
						hashSet.Add(modId);
					}
				}
				if (hashSet.Count > 0)
				{
					throw new MissingModException(filePath, shouldMarkAsCorrupted: false, hashSet);
				}
				if (serializedGameState.DLCsInUse != null)
				{
					HashSet<string> hashSet2 = new HashSet<string>();
					foreach (string dlcId in serializedGameState.DLCsInUse)
					{
						if (!TPSingleton<DLCManager>.Instance.OwnedDLCIds.Any((string ownedDLCId) => ownedDLCId == dlcId))
						{
							hashSet2.Add(dlcId);
						}
					}
					if (hashSet2.Count > 0)
					{
						throw new MissingDLCException(filePath, shouldMarkAsCorrupted: false, hashSet2);
					}
				}
			}
			if (val.SaveVersion < minVersion)
			{
				throw new WrongSaveVersionException(filePath, shouldMarkAsCorrupted: false);
			}
			return val;
		}
		catch (Exception ex)
		{
			CLoggerManager.Log("Tried to SafeLoad " + filePath + ", caught an exception: " + ex.Message + ". Safely returning null.");
			caughtException = ex;
			return null;
		}
	}

	public static void RegisterUnkownXMLElementHandler<T>(XmlElementEventHandler eventHandler) where T : ISerializedData
	{
		Type typeFromHandle = typeof(T);
		if (UnknownElementEvents.ContainsKey(typeFromHandle.FullName))
		{
			Dictionary<string, XmlElementEventHandler> unknownElementEvents = UnknownElementEvents;
			string fullName = typeFromHandle.FullName;
			unknownElementEvents[fullName] = (XmlElementEventHandler)Delegate.Combine(unknownElementEvents[fullName], eventHandler);
		}
		else
		{
			UnknownElementEvents.Add(typeFromHandle.FullName, eventHandler);
		}
	}

	private static T LoadInternal<T>(Stream stream) where T : SerializedContainer
	{
		switch (SaveManager.SaveFormat)
		{
		case E_SaveFormat.Xml:
		{
			Type typeFromHandle = typeof(T);
			XmlSerializer xmlSerializer = new XmlSerializer(typeFromHandle);
			if (UnknownElementEvents.ContainsKey(typeFromHandle.FullName))
			{
				xmlSerializer.UnknownElement += UnknownElementEvents[typeFromHandle.FullName];
			}
			using XmlReader xmlReader = XmlReader.Create(stream, new XmlReaderSettings());
			return (T)xmlSerializer.Deserialize(xmlReader);
		}
		case E_SaveFormat.Raw:
			return (T)new BinaryFormatter().Deserialize(stream);
		default:
			throw new SaveLoadingFailedException(string.Empty, $"Unrecognized save format selected: {SaveManager.SaveFormat}", shouldMarkAsCorrupted: false);
		}
	}

	public static XDocument LoadXml(string filePath, bool useEncryption = false)
	{
		try
		{
			return LoadXMLInternal(filePath, useEncryption);
		}
		catch (SaveEncoder.CorruptSaveGameException)
		{
			CLoggerManager.Log("The savefile " + filePath + " is corrupt and cannot be loaded (damaged GZIP).", LogType.Error, CLogLevel.MAJOR);
			CLoggerManager.Log("===============================[ LOGBAR ]===============================", LogType.Error, CLogLevel.MAJOR);
			CLoggerManager.Log("Please ignore all subsequent NULLRefs. We meet again at the next LOGBAR.", LogType.Error, CLogLevel.MAJOR);
			throw new SaveLoadingFailedException(filePath, shouldMarkAsCorrupted: true);
		}
		catch (SaveEncoder.HashMismatchException)
		{
			CLoggerManager.Log("The savefile " + filePath + " has been modified (checksum mismatch) and cannot be loaded.", LogType.Error, CLogLevel.MAJOR);
			CLoggerManager.Log("===============================[ LOGBAR ]===============================", LogType.Error, CLogLevel.MAJOR);
			CLoggerManager.Log("Please ignore all subsequent NULLRefs. We meet again at the next LOGBAR.", LogType.Error, CLogLevel.MAJOR);
			throw new SaveLoadingFailedException(filePath, shouldMarkAsCorrupted: true);
		}
	}

	public static void EnqueueSave(E_SaveType saveType)
	{
		lock (SaveLock)
		{
			saveQueues[saveType].Enqueue(saveType.GetSaveInfo());
			if (saveQueues[saveType].Count == 1)
			{
				System.Threading.Tasks.Task.Run(delegate
				{
					Save(saveType);
				});
			}
		}
	}

	public static void Save(E_SaveType saveType)
	{
		while (true)
		{
			CLoggerManager.Log(saveType.ToString() + " : save starts :", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			SaveInfo saveInfo = saveQueues[saveType].Peek();
			if (saveInfo.BackupFilePath != string.Empty && saveInfo.TemporaryBackupFilePath != string.Empty)
			{
				CLoggerManager.Log(saveType.ToString() + " : copying temporary backup...", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
				CopyFileTo(saveInfo.FilePath, saveInfo.TemporaryBackupFilePath);
				CLoggerManager.Log(saveType.ToString() + " : temporary backup copied.", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
				CLoggerManager.Log(saveType.ToString() + " : copying backup...", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
				CopyFileTo(saveInfo.TemporaryBackupFilePath, saveInfo.BackupFilePath);
				CLoggerManager.Log(saveType.ToString() + " : backup copied.", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			}
			CLoggerManager.Log(saveType.ToString() + " : saving...", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			Save(saveInfo.Container, saveInfo.FilePath, saveInfo.UseEncryption);
			CLoggerManager.Log(saveType.ToString() + " : saved.", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			lock (SaveLock)
			{
				saveQueues[saveType].Dequeue();
				if (saveQueues[saveType].Count == 0)
				{
					break;
				}
			}
		}
	}

	public static void SaveXmlSync(XContainer container, string filePath, bool useEncryption = false)
	{
		SaveXml(container, filePath, useEncryption);
	}

	private static XDocument LoadXMLInternal(string filePath, bool useEncryption = false)
	{
		if (!File.Exists(filePath))
		{
			return null;
		}
		using FileStream fileStream = File.OpenRead(filePath);
		if (useEncryption)
		{
			byte[] buffer;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				SaveEncoder.Decode(fileStream, memoryStream);
				buffer = memoryStream.ToArray();
			}
			using MemoryStream stream = new MemoryStream(buffer);
			return XDocument.Load(stream);
		}
		return XDocument.Load(fileStream);
	}

	private static void OnGameSavingEnd(bool success)
	{
		CLoggerManager.Log($"Game save end. Success: {success}.", LogType.Log, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
		SaverLoader.OnGameSavingEnds?.Invoke(success);
	}

	private static void Save<T>(T container, string filePath, bool useEncryption = false) where T : SerializedContainer
	{
		if (container is SerializedGameState)
		{
			ThreadDispatcher.DispatchToMain(delegate
			{
				SaverLoader.OnGameSavingStarts?.Invoke();
			});
		}
		container.UpdateHeader();
		try
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Directory.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
			}
			byte[] buffer;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				switch (SaveManager.SaveFormat)
				{
				case E_SaveFormat.Xml:
				{
					XmlSerializer xmlSerializer = new XmlSerializer(container.GetType());
					using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
					{
						Indent = true,
						Encoding = Encoding.UTF8
					}))
					{
						xmlSerializer.Serialize(xmlWriter, container);
					}
					break;
				}
				case E_SaveFormat.Raw:
					new BinaryFormatter().Serialize(memoryStream, container);
					break;
				}
				buffer = memoryStream.ToArray();
			}
			using MemoryStream memoryStream2 = new MemoryStream(buffer);
			using FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
			if (useEncryption)
			{
				SaveEncoder.Encode(memoryStream2, fileStream);
			}
			else
			{
				memoryStream2.CopyTo(fileStream);
			}
		}
		catch (Exception arg)
		{
			CLoggerManager.Log($"Could not save {typeof(T)}! Triggered error:\n{arg}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SaverLoader");
			if (container is SerializedGameState)
			{
				ThreadDispatcher.DispatchToMain(delegate
				{
					OnGameSavingEnd(success: false);
				});
			}
			return;
		}
		if (container is SerializedGameState)
		{
			ThreadDispatcher.DispatchToMain(delegate
			{
				OnGameSavingEnd(success: true);
			});
		}
	}

	private static void SaveXml(XContainer container, string filePath, bool useEncryption = false)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Directory.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
			}
			byte[] buffer;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (XmlWriter writer = XmlWriter.Create(memoryStream, new XmlWriterSettings
				{
					Indent = true,
					Encoding = Encoding.UTF8
				}))
				{
					XDocument xDocument = new XDocument();
					xDocument.Add(container);
					xDocument.Save(writer);
				}
				buffer = memoryStream.ToArray();
			}
			using MemoryStream memoryStream2 = new MemoryStream(buffer);
			using FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
			if (useEncryption)
			{
				SaveEncoder.Encode(memoryStream2, fileStream);
			}
			else
			{
				memoryStream2.CopyTo(fileStream);
			}
		}
		catch (Exception arg)
		{
			CLoggerManager.Log($"Could not save XContainer! Triggered error:\n{arg}", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
