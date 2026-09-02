using Sirenix.OdinInspector;
using TheLastStand.Framework.Animation;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

[RequireComponent(typeof(SingleAnimPlayer))]
public class SpriteSheetFx : SerializedMonoBehaviour, IDisplayableEffect
{
	[SerializeField]
	private SingleAnimPlayer animPlayer;

	public Coroutine Display()
	{
		base.gameObject.SetActive(value: true);
		return animPlayer.Play();
	}

	public void Init(Tile targetTile)
	{
		base.transform.position = TileMapView.GetWorldPosition(targetTile);
	}

	public void Init(Vector3 worldPos)
	{
		base.transform.position = worldPos;
	}

	protected void Awake()
	{
		if (animPlayer == null)
		{
			animPlayer = GetComponent<SingleAnimPlayer>();
		}
		base.gameObject.SetActive(value: false);
	}
}
