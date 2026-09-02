using Newtonsoft.Json;

namespace TheLastStand.Model.AnalyticsEventsData;

public class GameStartData
{
	[JsonProperty("gpu")]
	public string GPUName;

	[JsonProperty("cpu")]
	public string CPUName;

	[JsonProperty("controller")]
	public string InputController;

	[JsonProperty("game_language")]
	public string GameLanguage;

	[JsonProperty("os_language")]
	public string OSLanguage;

	[JsonProperty("dlc")]
	public string[] OwnedDLCs;

	public GameStartData(string gpuName, string cpuName, string inputController, string gameLanguage, string osLanguage, string[] ownedDLCs)
	{
		GPUName = gpuName;
		CPUName = cpuName;
		InputController = inputController;
		GameLanguage = gameLanguage;
		OSLanguage = osLanguage;
		OwnedDLCs = ownedDLCs;
	}
}
