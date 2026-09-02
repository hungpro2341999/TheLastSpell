using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class BossUnitTemplateDefinition : EnemyUnitTemplateDefinition
{
	public string DeathCutsceneId { get; private set; }

	public bool AlwaysPlayDeathCutscene { get; private set; }

	public BossUnitTemplateDefinition(XContainer container)
		: base(container)
	{
		base.UnitType = DamageableType.Boss;
	}

	protected override bool CanSpawnOnSingleTile(Tile tile, bool isPhaseActor = false, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (base.MoveMethod == E_MoveMethod.Walking && !tile.IsCrossable)
		{
			return false;
		}
		if ((ignoreBuildings || tile.Building == null) && (ignoreUnits || tile.Unit == null) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
		{
			return false;
		}
		if (!ignoreUnits && tile.Unit != null && (!isPhaseActor || tile.Unit is EnemyUnit { IsBossPhaseActor: not false }))
		{
			return false;
		}
		return true;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		BossUnitTemplateDefinition value = null;
		XAttribute xAttribute = xElement.Attribute("TemplateId");
		if (xAttribute != null && !BossUnitDatabase.BossUnitTemplateDefinitions.TryGetValue(xAttribute.Value, out value))
		{
			CLoggerManager.Log("BossUnit " + base.Id + " could not find template with Id " + xAttribute.Value + "!", LogType.Error);
		}
		XElement xElement2 = xElement.Element("DeathCutscene");
		if (xElement2 != null)
		{
			XAttribute xAttribute2 = xElement2.Attribute("Id");
			XAttribute xAttribute3 = xElement2.Attribute("AlwaysPlay");
			DeathCutsceneId = xAttribute2.Value;
			if (xAttribute3 != null)
			{
				if (bool.TryParse(xAttribute3.Value, out var result))
				{
					AlwaysPlayDeathCutscene = result;
					return;
				}
				CLoggerManager.Log("AlwaysPlayCutscene " + base.Id + ": Could not parse " + xAttribute3.Value + " as a bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else if (value != null)
		{
			DeathCutsceneId = value.DeathCutsceneId;
			AlwaysPlayDeathCutscene = value.AlwaysPlayDeathCutscene;
		}
	}
}
