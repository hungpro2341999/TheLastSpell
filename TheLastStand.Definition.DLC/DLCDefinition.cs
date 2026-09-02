using System;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.Definition.DLC;

[CreateAssetMenu(fileName = "New DLC Definition", menuName = "TLS/DLC/DLC Definition")]
public class DLCDefinition : ScriptableObject
{
	public static class Constants
	{
		public const string SteamDlcStoreUrl = "https://store.steampowered.com/app/{0}";
	}

	[Serializable]
	public struct DLCGoGSpecificData
	{
		[SerializeField]
		private ulong id;

		[SerializeField]
		private string storeUrl;

		public ulong Id => id;

		public string StoreUrl => storeUrl;
	}

	[Serializable]
	public struct DLCSteamSpecificData
	{
		[SerializeField]
		private uint id;

		public uint Id => id;

		public string StoreUrl => $"https://store.steampowered.com/app/{id}";
	}

	[SerializeField]
	private string id;

	[SerializeField]
	private Sprite iconSprite;

	[SerializeField]
	private string textIconIdForPortraits;

	[Tooltip("Should be a simple letter, to add in the version number")]
	[SerializeField]
	private string versionIdentifier;

	[SerializeField]
	private DLCSteamSpecificData steamSpecificData;

	[SerializeField]
	private DLCGoGSpecificData gogSpecificData;

	[SerializeField]
	private bool ownedInEditor;

	public DLCGoGSpecificData GogSpecificData => gogSpecificData;

	public string Id => id;

	public Sprite IconSprite => iconSprite;

	public string LocalizedName => Localizer.Get("DLC_Name_" + id);

	public bool OwnedInEditor => ownedInEditor;

	public DLCSteamSpecificData SteamSpecificData => steamSpecificData;

	public string TextIconForPortraits => "<style=" + textIconIdForPortraits + "></style>";

	public string VersionIdentifier => versionIdentifier;

	public string GetStoreURL()
	{
		return steamSpecificData.StoreUrl;
	}
}
