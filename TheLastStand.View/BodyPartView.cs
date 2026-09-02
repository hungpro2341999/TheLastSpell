using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View;

[RequireComponent(typeof(SpriteRenderer))]
public class BodyPartView : MonoBehaviour
{
	[SerializeField]
	private BodyPartDefinition.E_Orientation orientation = BodyPartDefinition.E_Orientation.Front;

	[SerializeField]
	private bool keepOwnMaterial;

	private SpriteRenderer bodyPartRenderer;

	public bool IsDirty { get; set; } = true;

	public BodyPart BodyPart { get; set; }

	public BodyPartDefinition.E_Orientation Orientation => orientation;

	public static Sprite GetSprite(BodyPart bodyPart, string faceId, string gender, BodyPartDefinition.E_Orientation orientation)
	{
		if (bodyPart == null)
		{
			TPDebug.LogError("BodyPartView.GetSprite() => bodyPart can't be null! Aborting...");
			return null;
		}
		if (bodyPart.AdditionalConstraints.Contains("Hide"))
		{
			return null;
		}
		string spritePath = bodyPart.GetSpritePath(faceId, gender, orientation);
		if (string.IsNullOrEmpty(spritePath))
		{
			return null;
		}
		return ResourcePooler.LoadOnce<Sprite>(spritePath);
	}

	public Sprite GetSprite()
	{
		return bodyPartRenderer.sprite;
	}

	public void Init()
	{
		InitRenderer();
	}

	public void Refresh(string faceId, string gender, bool forceRefresh = false)
	{
		if ((IsDirty || forceRefresh) && !(bodyPartRenderer == null))
		{
			Sprite sprite = GetSprite(faceId, gender);
			bodyPartRenderer.sprite = sprite;
			bodyPartRenderer.enabled = sprite != null;
			IsDirty = false;
		}
	}

	public void SetMaterial(Material material)
	{
		if (!keepOwnMaterial)
		{
			InitRenderer();
			bodyPartRenderer.material = material;
		}
	}

	public void Tint(Color bodyPartColor)
	{
		bodyPartRenderer.color = bodyPartColor;
	}

	private void Awake()
	{
		InitRenderer();
	}

	private Sprite GetSprite(string faceId, string gender)
	{
		return GetSprite(BodyPart, faceId, gender, orientation);
	}

	private void InitRenderer()
	{
		if (!(bodyPartRenderer != null))
		{
			bodyPartRenderer = GetComponent<SpriteRenderer>();
		}
	}
}
