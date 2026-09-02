using System;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public class StandardVisualEffectDefinition : VisualEffectDefinition
{
	public class TargetData
	{
		public enum E_TargetType
		{
			None = -1,
			Caster,
			AoeOrigin,
			HitTiles,
			HitUnits,
			HitPlayableUnits,
			HitEnemyUnits,
			PropagationTiles,
			CastFxSourceTile,
			FollowCaster
		}

		public E_TargetType TargetType { get; set; }

		public Vector2Int TileOffset { get; set; }
	}

	public TargetData Target { get; private set; }

	public string SpawnedParticlesPath { get; set; } = string.Empty;

	public StandardVisualEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container.Element("Target");
		XElement xElement2 = container.Element("SpawnedParticles");
		if (xElement2 != null)
		{
			SpawnedParticlesPath = xElement2.Value;
		}
		if (xElement != null)
		{
			if (!Enum.TryParse<TargetData.E_TargetType>(xElement.Value, out var result))
			{
				Debug.LogError("VisualEffectDefinition (Path='" + paths[0] + "') defines an invalid Target (" + xElement.Value + ")!");
				return;
			}
			Vector2Int zero = Vector2Int.zero;
			XAttribute xAttribute = xElement.Attribute("XTileOffset");
			if (!xAttribute.IsNullOrEmpty())
			{
				zero.x = int.Parse(xAttribute.Value, CultureInfo.InvariantCulture);
			}
			XAttribute xAttribute2 = xElement.Attribute("YTileOffset");
			if (!xAttribute2.IsNullOrEmpty())
			{
				zero.y = int.Parse(xAttribute2.Value, CultureInfo.InvariantCulture);
			}
			Target = new TargetData
			{
				TargetType = result,
				TileOffset = zero
			};
		}
		else
		{
			Debug.LogError("VisualEffectDefinition (Path='" + paths[0] + "') must define a Target!");
		}
	}
}
