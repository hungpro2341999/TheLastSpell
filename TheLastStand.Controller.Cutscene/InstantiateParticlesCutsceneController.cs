using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class InstantiateParticlesCutsceneController : CutsceneController
{
	public InstantiateParticlesCutsceneDefinition InstantiateParticlesCutsceneDefinition => base.CutsceneDefinition as InstantiateParticlesCutsceneDefinition;

	public InstantiateParticlesCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Tile == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play a InstantiateParticlesCutsceneController with a null tile.");
			yield break;
		}
		GameObject pooledGameObject = ObjectPooler.GetPooledGameObject(InstantiateParticlesCutsceneDefinition.ParticlesId, ResourcePooler.LoadOnce<GameObject>(InstantiateParticlesCutsceneDefinition.ParticlesPath));
		if (pooledGameObject != null)
		{
			pooledGameObject.transform.position = TileMapView.GetTileCenter(cutsceneData.Tile);
		}
	}
}
