using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public abstract class UnitTemplateDefinition : TheLastStand.Framework.Serialization.Definition, ITileObjectDefinition
{
	public enum E_MoveMethod
	{
		Undefined,
		Walking,
		Flying,
		AboveAll
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct UnitTypeComparer : IEqualityComparer<DamageableType>
	{
		public bool Equals(DamageableType x, DamageableType y)
		{
			return x == y;
		}

		public int GetHashCode(DamageableType obj)
		{
			return (int)obj;
		}
	}

	public static readonly UnitTypeComparer SharedUnitTypeComparer;

	public string DamagedParticlesId { get; protected set; } = string.Empty;

	public Vector2 HUDOffset { get; private set; }

	public List<InjuryDefinition> InjuryDefinitions { get; protected set; }

	public E_MoveMethod MoveMethod { get; protected set; } = E_MoveMethod.Walking;

	public int OriginX { get; protected set; }

	public int OriginY { get; protected set; }

	public bool OverrideDefaultHUDOffset { get; private set; }

	public Vector3 SelectedFeedbackSize { get; protected set; } = Vector3.one;

	public List<List<Tile.E_UnitAccess>> Tiles { get; protected set; }

	public abstract Tile.E_UnitAccess UnitAccessNeeded { get; }

	public DamageableType UnitType { get; protected set; }

	public virtual bool UpdateAnimatorOnOrientationChange { get; protected set; }

	public UnitTemplateDefinition(XContainer container)
		: base(container)
	{
	}

	public bool CanSpawnOn(Tile tile, bool isPhaseActor = false, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		foreach (Tile occupiedTile in tile.GetOccupiedTiles(this))
		{
			if (!CanSpawnOnSingleTile(occupiedTile, isPhaseActor, ignoreUnits, ignoreBuildings))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool CanSpawnOnSingleTile(Tile tile, bool isPhaseActor = false, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (!CanTravelThrough(tile, ignoreUnits, ignoreBuildings))
		{
			return false;
		}
		if (MoveMethod != E_MoveMethod.AboveAll && !ignoreBuildings && tile.Building != null)
		{
			DamageableModule damageableModule = tile.Building.DamageableModule;
			if ((damageableModule == null || !damageableModule.IsDead) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
		}
		if ((ignoreUnits || tile.Unit == null) && tile.Building == null && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
		{
			return false;
		}
		if (!ignoreUnits && tile.Unit != null && (!isPhaseActor || tile.Unit is EnemyUnit { IsBossPhaseActor: not false }))
		{
			return false;
		}
		return true;
	}

	public bool CanStopOn(Tile tile, TheLastStand.Model.Unit.Unit unit)
	{
		foreach (Tile occupiedTile in tile.GetOccupiedTiles(this))
		{
			if (!CanStopOnSingleTile(occupiedTile, unit))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool CanStopOnSingleTile(Tile tile, TheLastStand.Model.Unit.Unit unit)
	{
		if (!CanTravelThrough(tile))
		{
			return false;
		}
		if (MoveMethod != E_MoveMethod.AboveAll && tile.Building != null)
		{
			DamageableModule damageableModule = tile.Building.DamageableModule;
			if ((damageableModule == null || !damageableModule.IsDead) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
		}
		if (tile.Building == null && tile.Unit == null && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
		{
			return false;
		}
		if (tile.WillBeReached && tile.WillBeReachedBy != unit.RandomId)
		{
			return false;
		}
		return true;
	}

	public bool CanTravelThrough(Tile tile, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		return CanTravelThrough(tile, MoveMethod, ignoreUnits, ignoreBuildings);
	}

	public virtual bool CanTravelThrough(Tile tile, E_MoveMethod moveMethod, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (tile == null)
		{
			return false;
		}
		switch (moveMethod)
		{
		case E_MoveMethod.Walking:
			if (!tile.IsCrossable)
			{
				return false;
			}
			if (!ignoreBuildings && tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || tile.Building.ShouldWaitDeathLikeEffect || !tile.Building.DamageableModule.IsDead) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
			if ((ignoreUnits || tile.Unit == null) && tile.Building == null && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
			break;
		case E_MoveMethod.Flying:
			if (!ignoreBuildings && tile.Building != null && tile.Building.BuildingDefinition.BlueprintModuleDefinition.BlockFlying)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement obj = xContainer as XElement;
		XElement xElement = obj.Element("DamagedParticles");
		if (xElement != null)
		{
			XAttribute xAttribute = xElement.Attribute("Id");
			if (xAttribute != null)
			{
				DamagedParticlesId = xAttribute.Value;
			}
		}
		XElement xElement2 = obj.Element("HUDOffset");
		if (xElement2 != null)
		{
			XAttribute xAttribute2 = xElement2.Attribute("X");
			HUDOffset = new Vector2(y: int.Parse(xElement2.Attribute("Y")?.Value ?? "0"), x: int.Parse(xAttribute2?.Value ?? "0"));
			OverrideDefaultHUDOffset = true;
		}
		else
		{
			OverrideDefaultHUDOffset = false;
			HUDOffset = Vector2.zero;
		}
		XElement xElement3 = obj.Element("Tiles");
		Tiles = new List<List<Tile.E_UnitAccess>>();
		if (xElement3 != null)
		{
			string[] array = xElement3.Value.Split('\n');
			for (int num = array.Length - 1; num >= 0; num--)
			{
				array[num] = array[num].RemoveWhitespace();
				if (array[num] != string.Empty)
				{
					Tiles.Add(new List<Tile.E_UnitAccess>(array[num].Length));
					for (int i = 0; i < array[num].Length; i++)
					{
						char tileChar = array[num][i];
						Tiles[Tiles.Count - 1].Add(Tile.CharToUnitAccess(tileChar));
					}
				}
			}
			OriginX = ((xElement3.Attribute("OriginX") != null) ? int.Parse(xElement3.Attribute("OriginX").Value) : 0);
			OriginY = ((xElement3.Attribute("OriginY") != null) ? int.Parse(xElement3.Attribute("OriginY").Value) : 0);
		}
		else
		{
			Tiles.Add(new List<Tile.E_UnitAccess> { Tile.E_UnitAccess.Blocked });
			OriginX = 0;
			OriginY = 0;
		}
		SelectedFeedbackSize = new Vector3(Tiles.First().Count, Tiles.Count, 1f);
	}

	public abstract void DeserializeInjuries(XContainer container);
}
