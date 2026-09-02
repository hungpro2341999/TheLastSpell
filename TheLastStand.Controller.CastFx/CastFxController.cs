using TheLastStand.Definition.CastFx;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.CastFx;
using TheLastStand.View.CastFX;
using UnityEngine;

namespace TheLastStand.Controller.CastFx;

public class CastFxController
{
	public TheLastStand.Model.CastFx.CastFx CastFx { get; }

	public CastFxController(CastFxDefinition definition)
	{
		CastFx = new TheLastStand.Model.CastFx.CastFx(definition, this);
	}

	public void PlayCastFxs(TileObjectSelectionManager.E_Orientation specificOrientation = TileObjectSelectionManager.E_Orientation.NONE, Vector2 offset = default(Vector2), ITileObject source = null, bool isMirroredOnYAxis = false)
	{
		CastFxView.PlayCastFxs(CastFx, specificOrientation, offset, source, isMirroredOnYAxis);
	}
}
