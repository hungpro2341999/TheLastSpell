using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Controller;

/// <summary>
/// Bộ điều khiển cấp ứng dụng cao nhất (Root Application Controller):
/// Quản lý máy trạng thái hữu hạn (Finite State Machine / Automaton) của toàn bộ game,
/// điều phối chuyển đổi giữa các màn hình lớn: GameLobby, SplashScreen, WorldMap, MetaShops, Game, Settings...
/// </summary>
public class ApplicationController
{
	#region Delegates & Events

	/// <summary>
	/// Delegate thông báo sự kiện thay đổi trạng thái của ứng dụng.
	/// </summary>
	/// <param name="state">Trạng thái mới vừa được thiết lập.</param>
	public delegate void ApplicationStateChangeHandler(State state);

	/// <summary>
	/// Sự kiện được kích hoạt mỗi khi ứng dụng chuyển sang một State mới.
	/// </summary>
	public event ApplicationStateChangeHandler ApplicationStateChangeEvent;

	#endregion

	#region Properties & Constructor

	/// <summary>
	/// Model lưu trữ dữ liệu và ngăn xếp trạng thái (State Stack) của ứng dụng.
	/// </summary>
	public TheLastStand.Model.Application Application { get; private set; }

	/// <summary>
	/// Khởi tạo ApplicationController và model Application.
	/// </summary>
	public ApplicationController()
	{
		Application = new TheLastStand.Model.Application(this);
	}

	#endregion

	#region State Navigation & Transitions

	/// <summary>
	/// Quay trở lại trạng thái trước đó từ ngăn xếp PreviousStatesStack.
	/// </summary>
	public void BackToPreviousState()
	{
		if (Application.PreviousStatesStack.Count == 0)
		{
			TPSingleton<ApplicationManager>.Instance.LogError($"Trying to back to a previous state (currently {Application.State}) but no previous state found!", CLogLevel.MAJOR);
		}
		else
		{
			Application.SetState(Application.PreviousStatesStack.Pop());
		}
	}

	/// <summary>
	/// Chuyển trạng thái ứng dụng theo tên định danh chuỗi (stateName).
	/// Khởi tạo và lưu vào StatesPool nếu trạng thái chưa từng được tạo trước đó.
	/// </summary>
	/// <param name="stateName">Tên định danh của trạng thái cần chuyển tới.</param>
	public void SetState(string stateName)
	{
		// Kiểm tra trạng thái đã có trong pool chưa, nếu chưa thì khởi tạo mới
		if (!Application.StatesPool.TryGetValue(stateName, out var value))
		{
			switch (stateName)
			{
			case "ExitApp":
				value = new ExitAppState();
				break;
			case "Game":
				value = new GameState();
				break;
			case "GameLobby":
				value = new GameLobbyState();
				break;
			case "LoadGame":
				value = new LoadGameState();
				break;
			case "ReloadGame":
				value = new ReloadGameState();
				break;
			case "LoadWorldMap":
				value = new LoadWorldMapState();
				break;
			case "WorldMap":
				value = new WorldMapState();
				break;
			case "MetaShops":
				value = new MetaShopsState();
				break;
			case "NewGame":
				value = new NewGameState();
				break;
			case "Settings":
				value = new SettingsState();
				break;
			case "LevelEditor":
				value = new LevelEditorState();
				break;
			case "Credits":
				value = new CreditsState();
				break;
			case "AnimatedCutscene":
				value = new AnimatedCutsceneState();
				break;
			case "ModList":
				value = new ModListState();
				break;
			case "SplashScreen":
				value = new TheLastStand.Controller.ApplicationState.SplashScreen();
				break;
			case "SaveSlots":
				value = new SaveSlotsState();
				break;
			case "LoadMainMenuFromGameState":
				value = new LoadMainMenuFromGameState();
				break;
			default:
				TPSingleton<ApplicationManager>.Instance.LogError("Unknown state " + stateName, CLogLevel.MAJOR);
				return;
			}
			Application.StatesPool.Add(stateName, value);
		}
		
		// Xử lý hiệu ứng mờ dần (Fade In / Fade Out) cho các màn hình chuyển cảnh lớn
		switch (stateName)
		{
		case "GameLobby":
			if (ScenesManager.IsSceneActive(ScenesManager.SplashSceneName) || !(Application.State?.GetName() != "LoadMainMenuFromGameState"))
			{
				break;
			}
			goto case "Credits";
		case "Credits":
		case "Game":
		case "AnimatedCutscene":
		case "LoadGame":
		case "LoadWorldMap":
		case "MetaShops":
		case "NewGame":
		case "WorldMap":
		case "LoadMainMenuFromGameState":
			if (Application.State == null)
			{
				SetState(value);
			}
			else
			{
				TPSingleton<ApplicationManager>.Instance.StartCoroutine(SetStateCoroutine(value));
			}
			return;
		}
		SetState(value);
	}

	/// <summary>
	/// Thiết lập đối tượng State cụ thể cho ứng dụng, ghi log và kích hoạt event.
	/// </summary>
	/// <param name="newState">Đối tượng trạng thái mới.</param>
	/// <param name="args">Các tham số tùy chọn truyền kèm.</param>
	public void SetState(State newState, params object[] args)
	{
		TPSingleton<ApplicationManager>.Instance.Log($"State transition : {Application.State} -> {newState}", CLogLevel.DETAILED);
		Application.SetState(newState);
		ApplicationManager.CurrentStateName = Application.State.GetName();
		this.ApplicationStateChangeEvent?.Invoke(Application.State);
	}

	#endregion

	#region Transition Coroutines

	/// <summary>
	/// Coroutine chuyển cảnh: Làm tối màn hình (Fade In), chờ hiệu ứng tối hoàn tất rồi mới chuyển State.
	/// </summary>
	/// <param name="newState">Trạng thái mới cần chuyển tới.</param>
	public IEnumerator SetStateCoroutine(State newState)
	{
		CanvasFadeManager.FadeIn();
		yield return new WaitUntil(() => TPSingleton<CanvasFadeManager>.Instance.FadeIsOver);
		SetState(newState);
	}

	#endregion
}
