using TPLib;
using TheLastStand.Manager.Sound;
using TheLastStand.View.WorldMap;
using UnityEngine;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapRefsManager : TPSingleton<WorldMapRefsManager>
{
	[SerializeField]
	public Transform CitiesParent;

	[SerializeField]
	public WorldMapAmbientSound[] AmbientSounds;

	protected override void Awake()
	{
		base.Awake();
		ApplicationManager.Application.ApplicationController.SetState("WorldMap");
		TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.WorldMapMusic);
	}
}
