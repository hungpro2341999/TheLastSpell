using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Rewired;
using Rewired.Data;
using Rewired.Utils.Libraries.TinyJson;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Serialization;

public class KeyRemappingSaverLoader : UserDataStore
{
	public class Constants
	{
		public static readonly XNamespace XmlNamespace = "http://guavaman.com/rewired";

		public const string SaveElementLocalName = "InputMappingSave";

		public const string CategoryIdElementName = "categoryId";

		public const string LayoutIdElementName = "layoutId";

		public const string InputBehaviorElementName = "InputBehavior";

		public const string IdElementName = "id";

		public const string EditorLoadedMessage = "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.";

		public const int ControllerMapPPKeyVersionOriginal = 0;

		public const int ControllerMapPPKeyVersionIncludeDuplicateJoystickIndex = 1;

		public const int ControllerMapPPKeyVersionSupportDisconnectedControllers = 2;

		public const int ControllerMapPPKeyVersionIncludeFormatVersion = 2;

		public const int ControllerMapPPKeyVersion = 2;
	}

	private class ControllerAssignmentSaveInfo
	{
		public class PlayerInfo
		{
			public bool HasKeyboard { get; set; }

			public bool HasMouse { get; set; }

			public int Id { get; set; }

			public int JoystickCount
			{
				get
				{
					if (Joysticks == null)
					{
						return 0;
					}
					return Joysticks.Length;
				}
			}

			public JoystickInfo[] Joysticks { get; set; }

			public int IndexOfJoystick(int joystickId)
			{
				for (int i = 0; i < JoystickCount; i++)
				{
					if (Joysticks[i] != null && Joysticks[i].Id == joystickId)
					{
						return i;
					}
				}
				return -1;
			}

			public bool ContainsJoystick(int joystickId)
			{
				return IndexOfJoystick(joystickId) >= 0;
			}
		}

		public class JoystickInfo
		{
			public string HardwareIdentifier { get; set; }

			public int Id { get; set; }

			public Guid InstanceGuid { get; set; }
		}

		public int PlayerCount
		{
			get
			{
				if (Players == null)
				{
					return 0;
				}
				return Players.Length;
			}
		}

		public PlayerInfo[] Players { get; set; }

		public ControllerAssignmentSaveInfo()
		{
		}

		public ControllerAssignmentSaveInfo(int playerCount)
		{
			Players = new PlayerInfo[playerCount];
			for (int i = 0; i < playerCount; i++)
			{
				Players[i] = new PlayerInfo();
			}
		}

		public int IndexOfPlayer(int playerId)
		{
			for (int i = 0; i < PlayerCount; i++)
			{
				if (Players[i] != null && Players[i].Id == playerId)
				{
					return i;
				}
			}
			return -1;
		}

		public bool ContainsPlayer(int playerId)
		{
			return IndexOfPlayer(playerId) >= 0;
		}
	}

	private class JoystickAssignmentHistoryInfo
	{
		public readonly Joystick Joystick;

		public readonly int OldJoystickId;

		public JoystickAssignmentHistoryInfo(Joystick joystick, int oldJoystickId)
		{
			if (joystick == null)
			{
				throw new ArgumentNullException("joystick");
			}
			Joystick = joystick;
			OldJoystickId = oldJoystickId;
		}
	}

	[SerializeField]
	[Tooltip("Should this script be used? If disabled, nothing will be saved or loaded.")]
	private bool isEnabled = true;

	[SerializeField]
	[Tooltip("Should saved data be loaded on start?")]
	private bool loadDataOnStart = true;

	[SerializeField]
	[Tooltip("Should Player Joystick assignments be saved and loaded? This is not totally reliable for all Joysticks on all platforms. Some platforms/input sources do not provide enough information to reliably save assignments from session to session and reboot to reboot.")]
	private bool loadJoystickAssignments = true;

	[SerializeField]
	[Tooltip("Should Player Keyboard assignments be saved and loaded?")]
	private bool loadKeyboardAssignments = true;

	[SerializeField]
	[Tooltip("Should Player Mouse assignments be saved and loaded?")]
	private bool loadMouseAssignments = true;

	[HideInInspector]
	[SerializeField]
	[Tooltip("The PlayerPrefs key prefix. Change this to change how keys are stored in PlayerPrefs. Changing this will make saved data already stored with the old key no longer accessible.")]
	private string playerPrefsKeyPrefix = "RewiredSaveData";

	[SerializeField]
	private UserDataStore_PlayerPrefs playerPrefsSaverLoader;

	private XElement saveElement;

	private XElement loadElement;

	[NonSerialized]
	private bool allowImpreciseJoystickAssignmentMatching = true;

	[NonSerialized]
	private bool deferredJoystickAssignmentLoadPending;

	[NonSerialized]
	private bool wasJoystickEverDetected;

	[NonSerialized]
	private List<int> allActionIds;

	[NonSerialized]
	private string allActionIdsString;

	public static string KeyRemappingSaveFilePath => SaveManager.PersistentDataPath + "/Save/" + SaveManager.GetSaveSubFolderPath() + "/InputMappingSave.xml";

	public static bool KeyRemappingSaveExists => File.Exists(KeyRemappingSaveFilePath);

	public bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			isEnabled = value;
		}
	}

	public bool LoadDataOnStart
	{
		get
		{
			return loadDataOnStart;
		}
		set
		{
			loadDataOnStart = value;
		}
	}

	public bool LoadJoystickAssignments
	{
		get
		{
			return loadJoystickAssignments;
		}
		set
		{
			loadJoystickAssignments = value;
		}
	}

	public bool LoadKeyboardAssignments
	{
		get
		{
			return loadKeyboardAssignments;
		}
		set
		{
			loadKeyboardAssignments = value;
		}
	}

	public bool LoadMouseAssignments
	{
		get
		{
			return loadMouseAssignments;
		}
		set
		{
			loadMouseAssignments = value;
		}
	}

	public string PlayerPrefsKeyPrefix
	{
		get
		{
			return playerPrefsKeyPrefix;
		}
		set
		{
			playerPrefsKeyPrefix = value;
		}
	}

	private string PlayerPrefsKeyControllerAssignments => string.Format("{0}_{1}", playerPrefsKeyPrefix, "ControllerAssignments");

	private bool LoadControllerAssignments
	{
		get
		{
			if (!loadKeyboardAssignments && !loadMouseAssignments)
			{
				return loadJoystickAssignments;
			}
			return true;
		}
	}

	private List<int> AllActionIds
	{
		get
		{
			if (allActionIds != null)
			{
				return allActionIds;
			}
			List<int> list = new List<int>();
			IList<InputAction> actions = ReInput.mapping.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				list.Add(actions[i].id);
			}
			allActionIds = list;
			return list;
		}
	}

	public override void Save()
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not save any data.", base.gameObject);
			return;
		}
		saveElement = new XElement("InputMappingSave");
		try
		{
			SaveAll();
		}
		catch (Exception ex)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader could not save input mapping. Exception message : " + ex.Message, base.gameObject);
			return;
		}
		TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log("KeyRemappingSaverLoader saved all user data to XML.", CLogLevel.DETAILED);
	}

	public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not save any data.", this);
			return;
		}
		SaveControllerDataNow(playerId, controllerType, controllerId);
		TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log(string.Format("{0} saved {1} {2} data for Player {3} to XML.", "KeyRemappingSaverLoader", controllerType, controllerId, playerId));
	}

	public override void SaveControllerData(ControllerType controllerType, int controllerId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not save any data.", this);
			return;
		}
		SaveControllerDataNow(controllerType, controllerId);
		TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log(string.Format("{0} saved {1} {2} data to XML.", "KeyRemappingSaverLoader", controllerType, controllerId));
	}

	public override void SavePlayerData(int playerId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not save any data.", this);
			return;
		}
		SavePlayerDataNow(playerId);
		TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log(string.Format("{0} saved all user data for Player {1} to XML.", "KeyRemappingSaverLoader", playerId));
	}

	public override void SaveInputBehavior(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not save any data.", this);
			return;
		}
		SaveInputBehaviorNow(playerId, behaviorId);
		TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log(string.Format("{0} saved Input Behavior data for Player {1} to XML.", "KeyRemappingSaverLoader", playerId));
	}

	public override void Load()
	{
		TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log("KeyRemappingSaverLoader load.", this);
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not load any data.", this);
			return;
		}
		if (SaveManager.SettingsSaveVersion >= 9 && !KeyRemappingSaveExists)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log("KeyRemappingSaverLoader could not find xml file, trying to load from PlayerPrefs (this should be due to backward compatibility or if a new player loads for the first time).", base.gameObject, CLogLevel.DETAILED);
			playerPrefsSaverLoader.Load();
			playerPrefsSaverLoader.IsEnabled = false;
			return;
		}
		try
		{
			XDocument xDocument = SaverLoader.LoadXml(KeyRemappingSaveFilePath);
			if (xDocument != null)
			{
				loadElement = xDocument.Element("InputMappingSave");
				if (loadElement == null)
				{
					TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader has found a save file but no KeyRemapping element has been found. \n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.");
				}
				else if (LoadAll() > 0)
				{
					TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log("KeyRemappingSaverLoader loaded all user data from XML. \n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.", CLogLevel.NORMAL, forcePrintInUnity: true);
				}
			}
		}
		catch (Exception arg)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogError($"Error caught while loading InputMappingSave, resetting remapping. Message: {arg}", CLogLevel.MAJOR);
			TPSingleton<KeyRemappingManager>.Instance.ResetAll();
		}
	}

	public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not load any data.", this);
		}
		else if (LoadControllerDataNow(playerId, controllerType, controllerId) > 0)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning(string.Format("{0} loaded user data for {1} {2} for Player {3} from XML. ", "KeyRemappingSaverLoader", controllerType, controllerId, playerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.");
		}
	}

	public override void LoadControllerData(ControllerType controllerType, int controllerId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not load any data.", this);
		}
		else if (LoadControllerDataNow(controllerType, controllerId) > 0)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning(string.Format("{0} loaded user data for {1} {2} from XML. ", "KeyRemappingSaverLoader", controllerType, controllerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.");
		}
	}

	public override void LoadPlayerData(int playerId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not load any data.", this);
		}
		else if (LoadPlayerDataNow(playerId) > 0)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning(string.Format("{0} loaded Player {1} user data from XML. ", "KeyRemappingSaverLoader", playerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.");
		}
	}

	public override void LoadInputBehavior(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader is disabled and will not load any data.", this);
		}
		else if (LoadInputBehaviorNow(playerId, behaviorId) > 0)
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning(string.Format("{0} loaded Player {1} InputBehavior data from XML. ", "KeyRemappingSaverLoader", playerId) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.");
		}
	}

	protected override void OnInitialize()
	{
		if (LoadDataOnStart)
		{
			Load();
			if (LoadControllerAssignments && ReInput.controllers.joystickCount > 0)
			{
				SaveControllerAssignments();
			}
		}
	}

	protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		if (IsEnabled && args.controllerType == ControllerType.Joystick)
		{
			if (LoadJoystickData(args.controllerId) > 0)
			{
				TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning(string.Format("{0} loaded Joystick {1} ({2}) data from XML. ", "KeyRemappingSaverLoader", args.controllerId, ReInput.controllers.GetJoystick(args.controllerId).hardwareName) + "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.");
			}
			if (LoadDataOnStart && LoadJoystickAssignments && !wasJoystickEverDetected)
			{
				StartCoroutine(LoadJoystickAssignmentsDeferred());
			}
			if (LoadJoystickAssignments && !deferredJoystickAssignmentLoadPending)
			{
				SaveControllerAssignments();
			}
			wasJoystickEverDetected = true;
		}
	}

	protected override void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		if (IsEnabled && args.controllerType == ControllerType.Joystick)
		{
			SaveJoystickData(args.controllerId);
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log(string.Format("{0} saved Joystick {1} ({2}) data to XML.", "KeyRemappingSaverLoader", args.controllerId, ReInput.controllers.GetJoystick(args.controllerId).hardwareName));
		}
	}

	protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (IsEnabled && LoadControllerAssignments)
		{
			SaveControllerAssignments();
		}
	}

	public override void SaveControllerMap(int playerId, ControllerMap controllerMap)
	{
		if (controllerMap != null)
		{
			SaveControllerMap(controllerMap);
		}
	}

	public override ControllerMap LoadControllerMap(int playerId, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return null;
		}
		return LoadControllerMap(player, controllerIdentifier, categoryId, layoutId);
	}

	private int LoadAll()
	{
		int num = 0;
		if (LoadControllerAssignments && LoadControllerAssignmentsNow())
		{
			num++;
		}
		return num + LoadPlayerDataNow(TPSingleton<TheLastStand.Manager.InputManager>.Instance.Player);
	}

	private int LoadPlayerDataNow(int playerId)
	{
		return LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	private int LoadPlayerDataNow(Player player)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		num += LoadInputBehaviors(player.id);
		num += LoadControllerMaps(player.id, ControllerType.Keyboard, 0);
		num += LoadControllerMaps(player.id, ControllerType.Mouse, 0);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			num += LoadControllerMaps(player.id, ControllerType.Joystick, joystick.id);
		}
		RefreshLayoutManager(player.id);
		return num;
	}

	private int LoadAllJoystickCalibrationData()
	{
		int num = 0;
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			num += LoadJoystickCalibrationData(joysticks[i]);
		}
		return num;
	}

	private int LoadJoystickCalibrationData(Joystick joystick)
	{
		if (joystick == null)
		{
			return 0;
		}
		if (!joystick.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)))
		{
			return 0;
		}
		return 1;
	}

	private int LoadJoystickCalibrationData(int joystickId)
	{
		return LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private int LoadJoystickData(int joystickId)
	{
		int num = 0;
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				num += LoadControllerMaps(player.id, ControllerType.Joystick, joystickId);
				RefreshLayoutManager(player.id);
			}
		}
		return num + LoadJoystickCalibrationData(joystickId);
	}

	private int LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		int num = 0 + LoadControllerMaps(playerId, controllerType, controllerId);
		RefreshLayoutManager(playerId);
		return num + LoadControllerDataNow(controllerType, controllerId);
	}

	private int LoadControllerDataNow(ControllerType controllerType, int controllerId)
	{
		int num = 0;
		if (controllerType == ControllerType.Joystick)
		{
			num += LoadJoystickCalibrationData(controllerId);
		}
		return num;
	}

	private int LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		int num = 0;
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return num;
		}
		Rewired.Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
		if (controller == null)
		{
			return num;
		}
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory inputMapCategory = mapCategories[i];
			if (!inputMapCategory.userAssignable)
			{
				continue;
			}
			IList<InputLayout> list = ReInput.mapping.MapLayouts(controller.type);
			for (int j = 0; j < list.Count; j++)
			{
				InputLayout inputLayout = list[j];
				ControllerMap controllerMap = LoadControllerMap(player, controller.identifier, inputMapCategory.id, inputLayout.id);
				if (controllerMap != null)
				{
					player.controllers.maps.AddMap(controller, controllerMap);
					num++;
				}
			}
		}
		return num;
	}

	private ControllerMap LoadControllerMap(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		if (player == null)
		{
			return null;
		}
		string controllerMapXml = GetControllerMapXml(controllerIdentifier, categoryId, layoutId);
		if (string.IsNullOrEmpty(controllerMapXml))
		{
			return null;
		}
		ControllerMap controllerMap = ControllerMap.CreateFromXml(controllerIdentifier.controllerType, controllerMapXml);
		if (controllerMap == null)
		{
			return null;
		}
		List<int> controllerMapKnownActionIds = GetControllerMapKnownActionIds(player, controllerIdentifier, categoryId, layoutId);
		AddDefaultMappingsForNewActions(controllerIdentifier, controllerMap, controllerMapKnownActionIds);
		return controllerMap;
	}

	private int LoadInputBehaviors(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
		for (int i = 0; i < inputBehaviors.Count; i++)
		{
			num += LoadInputBehaviorNow(inputBehaviors[i]);
		}
		return num;
	}

	private int LoadInputBehaviorNow(int playerId, int behaviorId)
	{
		if (ReInput.players.GetPlayer(playerId) == null)
		{
			return 0;
		}
		InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
		if (inputBehavior == null)
		{
			return 0;
		}
		return LoadInputBehaviorNow(inputBehavior);
	}

	private int LoadInputBehaviorNow(InputBehavior inputBehavior)
	{
		if (inputBehavior == null)
		{
			return 0;
		}
		string inputBehaviorXml = GetInputBehaviorXml(inputBehavior.id);
		if (string.IsNullOrEmpty(inputBehaviorXml))
		{
			return 0;
		}
		if (!inputBehavior.ImportXmlString(inputBehaviorXml))
		{
			return 0;
		}
		return 1;
	}

	private bool LoadControllerAssignmentsNow()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = LoadControllerAssignmentData();
			if (controllerAssignmentSaveInfo == null)
			{
				return false;
			}
			if (loadKeyboardAssignments || loadMouseAssignments)
			{
				LoadKeyboardAndMouseAssignmentsNow(controllerAssignmentSaveInfo);
			}
			if (loadJoystickAssignments)
			{
				LoadJoystickAssignmentsNow(controllerAssignmentSaveInfo);
			}
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader loaded controller assignments from PlayerPrefs.");
		}
		catch
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogError("KeyRemappingSaverLoader encountered an error loading controller assignments from PlayerPrefs.");
		}
		return true;
	}

	private bool LoadKeyboardAndMouseAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (data.ContainsPlayer(allPlayer.id))
				{
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.Players[data.IndexOfPlayer(allPlayer.id)];
					if (loadKeyboardAssignments)
					{
						allPlayer.controllers.hasKeyboard = playerInfo.HasKeyboard;
					}
					if (loadMouseAssignments)
					{
						allPlayer.controllers.hasMouse = playerInfo.HasMouse;
					}
				}
			}
		}
		catch
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogError("KeyRemappingSaverLoader encountered an error loading keyboard and/or mouse assignments from PlayerPrefs.");
		}
		return true;
	}

	private bool LoadJoystickAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (ReInput.controllers.joystickCount == 0)
			{
				return false;
			}
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				allPlayer.controllers.ClearControllersOfType(ControllerType.Joystick);
			}
			List<JoystickAssignmentHistoryInfo> list = (loadJoystickAssignments ? new List<JoystickAssignmentHistoryInfo>() : null);
			foreach (Player allPlayer2 in ReInput.players.AllPlayers)
			{
				if (!data.ContainsPlayer(allPlayer2.id))
				{
					continue;
				}
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.Players[data.IndexOfPlayer(allPlayer2.id)];
				for (int i = 0; i < playerInfo.JoystickCount; i++)
				{
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = playerInfo.Joysticks[i];
					if (joystickInfo == null)
					{
						continue;
					}
					Joystick joystick = FindJoystickPrecise(joystickInfo);
					if (joystick != null)
					{
						if (list.Find((JoystickAssignmentHistoryInfo x) => x.Joystick == joystick) == null)
						{
							list.Add(new JoystickAssignmentHistoryInfo(joystick, joystickInfo.Id));
						}
						allPlayer2.controllers.AddController(joystick, removeFromOtherPlayers: false);
					}
				}
			}
			if (allowImpreciseJoystickAssignmentMatching)
			{
				foreach (Player allPlayer3 in ReInput.players.AllPlayers)
				{
					if (!data.ContainsPlayer(allPlayer3.id))
					{
						continue;
					}
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo2 = data.Players[data.IndexOfPlayer(allPlayer3.id)];
					for (int num = 0; num < playerInfo2.JoystickCount; num++)
					{
						ControllerAssignmentSaveInfo.JoystickInfo joystickInfo2 = playerInfo2.Joysticks[num];
						if (joystickInfo2 == null)
						{
							continue;
						}
						Joystick joystick2 = null;
						int num2 = list.FindIndex((JoystickAssignmentHistoryInfo x) => x.OldJoystickId == joystickInfo2.Id);
						if (num2 >= 0)
						{
							joystick2 = list[num2].Joystick;
						}
						else
						{
							if (!TryFindJoysticksImprecise(joystickInfo2, out var matches))
							{
								continue;
							}
							foreach (Joystick match in matches)
							{
								if (list.Find((JoystickAssignmentHistoryInfo x) => x.Joystick == match) == null)
								{
									joystick2 = match;
									break;
								}
							}
							if (joystick2 == null)
							{
								continue;
							}
							list.Add(new JoystickAssignmentHistoryInfo(joystick2, joystickInfo2.Id));
						}
						allPlayer3.controllers.AddController(joystick2, removeFromOtherPlayers: false);
					}
				}
			}
		}
		catch
		{
		}
		if (ReInput.configuration.autoAssignJoysticks)
		{
			ReInput.controllers.AutoAssignJoysticks();
		}
		return true;
	}

	private ControllerAssignmentSaveInfo LoadControllerAssignmentData()
	{
		try
		{
			if (!PlayerPrefs.HasKey(PlayerPrefsKeyControllerAssignments))
			{
				return null;
			}
			string text = PlayerPrefs.GetString(PlayerPrefsKeyControllerAssignments);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = JsonParser.FromJson<ControllerAssignmentSaveInfo>(text);
			if (controllerAssignmentSaveInfo == null || controllerAssignmentSaveInfo.PlayerCount == 0)
			{
				return null;
			}
			return controllerAssignmentSaveInfo;
		}
		catch
		{
			return null;
		}
	}

	private IEnumerator LoadJoystickAssignmentsDeferred()
	{
		deferredJoystickAssignmentLoadPending = true;
		yield return new WaitForEndOfFrame();
		if (ReInput.isReady)
		{
			if (LoadJoystickAssignmentsNow(null))
			{
				TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning("KeyRemappingSaverLoader loaded joystick assignments from PlayerPrefs.");
			}
			SaveControllerAssignments();
			deferredJoystickAssignmentLoadPending = false;
		}
	}

	private void SaveAll()
	{
		SavePlayerDataNow(TPSingleton<TheLastStand.Manager.InputManager>.Instance.Player);
		SaverLoader.SaveXmlSync(saveElement, KeyRemappingSaveFilePath);
	}

	private void SavePlayerDataNow(int playerId)
	{
		SavePlayerDataNow(ReInput.players.GetPlayer(playerId));
		PlayerPrefs.Save();
	}

	private void SavePlayerDataNow(Player player)
	{
		if (player != null)
		{
			PlayerSaveData saveData = player.GetSaveData(userAssignableMapsOnly: true);
			SaveInputBehaviors(player, saveData);
			SaveControllerMaps(player, saveData);
		}
	}

	private void SaveAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			SaveJoystickCalibrationData(joysticks[i]);
		}
	}

	private void SaveJoystickCalibrationData(int joystickId)
	{
		SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private void SaveJoystickCalibrationData(Joystick joystick)
	{
		if (joystick != null)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			PlayerPrefs.SetString(GetJoystickCalibrationMapPlayerPrefsKey(joystick), calibrationMapSaveData.map.ToXmlString());
		}
	}

	private void SaveJoystickData(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				SaveControllerMaps(player.id, ControllerType.Joystick, joystickId);
			}
		}
		SaveJoystickCalibrationData(joystickId);
	}

	private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		SaveControllerMaps(playerId, controllerType, controllerId);
		SaveControllerDataNow(controllerType, controllerId);
		PlayerPrefs.Save();
	}

	private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
	{
		if (controllerType == ControllerType.Joystick)
		{
			SaveJoystickCalibrationData(controllerId);
		}
		PlayerPrefs.Save();
	}

	private void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
	{
		foreach (ControllerMapSaveData allControllerMapSaveDatum in playerSaveData.AllControllerMapSaveData)
		{
			SaveControllerMap(allControllerMapSaveDatum.map);
		}
	}

	private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
		{
			return;
		}
		ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, userAssignableMapsOnly: true);
		if (mapSaveData != null)
		{
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				SaveControllerMap(mapSaveData[i].map);
			}
		}
	}

	private void SaveControllerMap(ControllerMap controllerMap)
	{
		XDocument xDocument = XDocument.Parse(controllerMap.ToXmlString());
		saveElement.Add(xDocument.Elements().First());
	}

	private void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
	{
		if (player != null)
		{
			InputBehavior[] inputBehaviors = playerSaveData.inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				SaveInputBehaviorNow(inputBehaviors[i]);
			}
		}
	}

	private void SaveInputBehaviorNow(int playerId, int behaviorId)
	{
		if (ReInput.players.GetPlayer(playerId) != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				SaveInputBehaviorNow(inputBehavior);
			}
		}
	}

	private void SaveInputBehaviorNow(InputBehavior inputBehavior)
	{
		if (inputBehavior != null)
		{
			XDocument xDocument = XDocument.Parse(inputBehavior.ToXmlString());
			saveElement.Add(xDocument.Elements().First());
		}
	}

	private bool SaveControllerAssignments()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = new ControllerAssignmentSaveInfo(ReInput.players.allPlayerCount);
			for (int i = 0; i < ReInput.players.allPlayerCount; i++)
			{
				Player player = ReInput.players.AllPlayers[i];
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = new ControllerAssignmentSaveInfo.PlayerInfo();
				controllerAssignmentSaveInfo.Players[i] = playerInfo;
				playerInfo.Id = player.id;
				playerInfo.HasKeyboard = player.controllers.hasKeyboard;
				playerInfo.HasMouse = player.controllers.hasMouse;
				ControllerAssignmentSaveInfo.JoystickInfo[] array = (playerInfo.Joysticks = new ControllerAssignmentSaveInfo.JoystickInfo[player.controllers.joystickCount]);
				for (int j = 0; j < player.controllers.joystickCount; j++)
				{
					Joystick joystick = player.controllers.Joysticks[j];
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = new ControllerAssignmentSaveInfo.JoystickInfo
					{
						InstanceGuid = joystick.deviceInstanceGuid,
						Id = joystick.id,
						HardwareIdentifier = joystick.hardwareIdentifier
					};
					array[j] = joystickInfo;
				}
			}
			PlayerPrefs.SetString(PlayerPrefsKeyControllerAssignments, JsonWriter.ToJson(controllerAssignmentSaveInfo));
			PlayerPrefs.Save();
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.Log("KeyRemappingSaverLoader saved controller assignments to PlayerPrefs.");
		}
		catch
		{
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogError("KeyRemappingSaverLoader encountered an error saving controller assignments to PlayerPrefs.");
		}
		return true;
	}

	private bool ControllerAssignmentSaveDataExists()
	{
		if (!PlayerPrefs.HasKey(PlayerPrefsKeyControllerAssignments))
		{
			return false;
		}
		if (string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsKeyControllerAssignments)))
		{
			return false;
		}
		return true;
	}

	private string GetBasePlayerPrefsKey(Player player)
	{
		return playerPrefsKeyPrefix + "|playerName=" + player.name;
	}

	private string GetControllerMapKnownActionIdsPlayerPrefsKey(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		return string.Concat(GetBasePlayerPrefsKey(player) + "|dataType=ControllerMap_KnownActionIds", GetControllerMapPlayerPrefsKeyCommonSuffix(player, controllerIdentifier, categoryId, layoutId, ppKeyVersion));
	}

	private static string GetControllerMapPlayerPrefsKeyCommonSuffix(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		string text = string.Empty;
		if (ppKeyVersion >= 2)
		{
			text = text + "|kv=" + ppKeyVersion;
		}
		text = text + "|controllerMapType=" + GetControllerMapType(controllerIdentifier.controllerType).Name;
		text = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		if (ppKeyVersion >= 2)
		{
			text = text + "|hardwareGuid=" + controllerIdentifier.hardwareTypeGuid.ToString();
			if (controllerIdentifier.hardwareTypeGuid == Guid.Empty)
			{
				text = text + "|hardwareIdentifier=" + controllerIdentifier.hardwareIdentifier;
			}
			if (controllerIdentifier.controllerType == ControllerType.Joystick)
			{
				text = text + "|duplicate=" + GetDuplicateIndex(player, controllerIdentifier);
			}
		}
		else
		{
			text = text + "|hardwareIdentifier=" + controllerIdentifier.hardwareIdentifier;
			if (controllerIdentifier.controllerType == ControllerType.Joystick)
			{
				text = text + "|hardwareGuid=" + controllerIdentifier.hardwareTypeGuid.ToString();
				if (ppKeyVersion >= 1)
				{
					text = text + "|duplicate=" + GetDuplicateIndex(player, controllerIdentifier);
				}
			}
		}
		return text;
	}

	private string GetJoystickCalibrationMapPlayerPrefsKey(Joystick joystick)
	{
		return string.Concat(string.Concat(string.Concat(playerPrefsKeyPrefix + "|dataType=CalibrationMap", "|controllerType=", joystick.type.ToString()), "|hardwareIdentifier=", joystick.hardwareIdentifier), "|hardwareGuid=", joystick.hardwareTypeGuid.ToString());
	}

	private string GetControllerMapXml(ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		foreach (XElement item in from o in loadElement.Elements()
			where o.Name.LocalName.Contains(controllerIdentifier.controllerType.ToString())
			select o)
		{
			XElement xElement = item.Element(Constants.XmlNamespace + "categoryId");
			XElement xElement2 = item.Element(Constants.XmlNamespace + "layoutId");
			if (xElement.Value == categoryId.ToString() && xElement2.Value == layoutId.ToString())
			{
				return item.ToString();
			}
		}
		return null;
	}

	private List<int> GetControllerMapKnownActionIds(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		List<int> list = new List<int>();
		string key = null;
		bool flag = false;
		for (int num = 2; num >= 0; num--)
		{
			key = GetControllerMapKnownActionIdsPlayerPrefsKey(player, controllerIdentifier, categoryId, layoutId, num);
			if (PlayerPrefs.HasKey(key))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return list;
		}
		string text = PlayerPrefs.GetString(key);
		if (string.IsNullOrEmpty(text))
		{
			return list;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && int.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}

	private string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(joystick);
		if (!PlayerPrefs.HasKey(joystickCalibrationMapPlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(joystickCalibrationMapPlayerPrefsKey);
	}

	private string GetInputBehaviorXml(int id)
	{
		return (from o in loadElement.Elements(Constants.XmlNamespace + "InputBehavior")
			where o.Element(Constants.XmlNamespace + "id").Value == id.ToString()
			select o).FirstOrDefault()?.ToString() ?? string.Empty;
	}

	private void AddDefaultMappingsForNewActions(ControllerIdentifier controllerIdentifier, ControllerMap controllerMap, List<int> knownActionIds)
	{
		if (controllerMap == null || knownActionIds == null || knownActionIds == null || knownActionIds.Count == 0)
		{
			return;
		}
		ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(controllerIdentifier, controllerMap.categoryId, controllerMap.layoutId);
		if (controllerMapInstance == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int allActionId in AllActionIds)
		{
			if (!knownActionIds.Contains(allActionId))
			{
				list.Add(allActionId);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (ActionElementMap allMap in controllerMapInstance.AllMaps)
		{
			if (list.Contains(allMap.actionId) && !controllerMap.DoesElementAssignmentConflict(allMap))
			{
				ElementAssignment elementAssignment = new ElementAssignment(controllerMap.controllerType, allMap.elementType, allMap.elementIdentifierId, allMap.axisRange, allMap.keyCode, allMap.modifierKeyFlags, allMap.actionId, allMap.axisContribution, allMap.invert);
				controllerMap.CreateElementMap(elementAssignment);
			}
		}
	}

	private Joystick FindJoystickPrecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo)
	{
		if (joystickInfo == null)
		{
			return null;
		}
		if (joystickInfo.InstanceGuid == Guid.Empty)
		{
			return null;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (joysticks[i].deviceInstanceGuid == joystickInfo.InstanceGuid)
			{
				return joysticks[i];
			}
		}
		return null;
	}

	private bool TryFindJoysticksImprecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo, out List<Joystick> matches)
	{
		matches = null;
		if (joystickInfo == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(joystickInfo.HardwareIdentifier))
		{
			return false;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (string.Equals(joysticks[i].hardwareIdentifier, joystickInfo.HardwareIdentifier, StringComparison.OrdinalIgnoreCase))
			{
				if (matches == null)
				{
					matches = new List<Joystick>();
				}
				matches.Add(joysticks[i]);
			}
		}
		return matches != null;
	}

	private static int GetDuplicateIndex(Player player, ControllerIdentifier controllerIdentifier)
	{
		Rewired.Controller controller = ReInput.controllers.GetController(controllerIdentifier);
		if (controller == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Rewired.Controller controller2 in player.controllers.Controllers)
		{
			if (controller2.type != controller.type)
			{
				continue;
			}
			bool flag = false;
			if (controller.type == ControllerType.Joystick)
			{
				if ((controller2 as Joystick).hardwareTypeGuid != controller.hardwareTypeGuid)
				{
					continue;
				}
				if (controller.hardwareTypeGuid != Guid.Empty)
				{
					flag = true;
				}
			}
			if (flag || !(controller2.hardwareIdentifier != controller.hardwareIdentifier))
			{
				if (controller2 == controller)
				{
					return num;
				}
				num++;
			}
		}
		return num;
	}

	private void RefreshLayoutManager(int playerId)
	{
		ReInput.players.GetPlayer(playerId)?.controllers.maps.layoutManager.Apply();
	}

	private static Type GetControllerMapType(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Custom:
			return typeof(CustomControllerMap);
		case ControllerType.Joystick:
			return typeof(JoystickMap);
		case ControllerType.Keyboard:
			return typeof(KeyboardMap);
		case ControllerType.Mouse:
			return typeof(MouseMap);
		default:
			TPSingleton<TheLastStand.Manager.InputManager>.Instance.LogWarning($" Unknown ControllerType {controllerType}");
			return null;
		}
	}
}
