using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Perk;

public class PerkTargeting
{
	public PerkTargetingDefinition PerkTargetingDefinition { get; }

	public PerkTargeting(PerkTargetingDefinition perkTargetingDefinition)
	{
		PerkTargetingDefinition = perkTargetingDefinition;
	}

	private List<ITileObject> GetTargetingReference(PerkDataContainer data, Perk perk)
	{
		List<ITileObject> list = new List<ITileObject>();
		switch (PerkTargetingDefinition.TargetingReference)
		{
		case PerkTargetingDefinition.E_TargetingReference.Owner:
			list.Add(perk.Owner);
			break;
		case PerkTargetingDefinition.E_TargetingReference.Caster:
			list.Add(data.Caster);
			break;
		case PerkTargetingDefinition.E_TargetingReference.Target:
			list.Add(data.TargetUnit);
			break;
		case PerkTargetingDefinition.E_TargetingReference.AllTargets:
			list.AddRange(data.AllAttackData.Select((AttackSkillActionExecutionTileData x) => x.TargetTile));
			break;
		}
		return list;
	}

	private HashSet<Tile> GetTargetFromBase(PerkDataContainer data, Perk perk, List<ITileObject> TargetingReferenceTileObjects, List<Tile> validTargets)
	{
		HashSet<Tile> targetTiles = new HashSet<Tile>();
		int num = PerkTargetingDefinition.AmountExpression?.EvalToInt(perk) ?? int.MaxValue;
		int num2 = PerkTargetingDefinition.RangeExpression?.EvalToInt(perk) ?? 1;
		foreach (ITileObject TargetingReferenceTileObject in TargetingReferenceTileObjects)
		{
			switch (PerkTargetingDefinition.TargetingMethod)
			{
			case PerkTargetingDefinition.E_TargetingMethod.AdjacentDamageables:
			{
				List<Tile> list = new List<Tile>();
				foreach (Tile adjacentTile in TargetingReferenceTileObject.TileObjectController.GetAdjacentTiles())
				{
					IDamageable damageable = adjacentTile.GetDamageable(isUnitPriority: true);
					if (!adjacentTile.HasFog && damageable != null && damageable.CanBeDamaged() && (damageable == null || damageable.IsTargetableByAI()) && damageable != perk.Owner)
					{
						List<DamageableType> validDamageableTypes = PerkTargetingDefinition.ValidDamageableTypes;
						if ((validDamageableTypes == null || validDamageableTypes.Contains(damageable.DamageableType)) && (PerkTargetingDefinition.TargetHasStatus == TheLastStand.Model.Status.Status.E_StatusType.None || (damageable is Unit unit2 && unit2.StatusOwned.HasFlag(PerkTargetingDefinition.TargetHasStatus) != PerkTargetingDefinition.HasStatusInverted)))
						{
							list.Add(adjacentTile);
						}
					}
				}
				targetTiles.AddRange(list);
				break;
			}
			case PerkTargetingDefinition.E_TargetingMethod.ClosestTarget:
				foreach (Tile item in validTargets.OrderBy((Tile t) => TileMapController.DistanceBetweenTiles(TargetingReferenceTileObject.OriginTile, t)).ToList())
				{
					if (targetTiles.Count >= num)
					{
						break;
					}
					IDamageable damageable2 = item.GetDamageable(isUnitPriority: true);
					if (damageable2 != null && damageable2.CanBeDamaged() && (damageable2 == null || damageable2.IsTargetableByAI()) && (PerkTargetingDefinition.ValidDamageableTypes == null || PerkTargetingDefinition.ValidDamageableTypes.Contains(damageable2.DamageableType)) && (PerkTargetingDefinition.TargetHasStatus == TheLastStand.Model.Status.Status.E_StatusType.None || !(damageable2 is Unit unit3) || unit3.StatusOwned.HasFlag(PerkTargetingDefinition.TargetHasStatus) != PerkTargetingDefinition.HasStatusInverted))
					{
						targetTiles.Add(item);
					}
				}
				break;
			case PerkTargetingDefinition.E_TargetingMethod.DamageablesInRange:
				GetDamageablesInRange(perk, TargetingReferenceTileObject, num2, num, shuffle: true, ref targetTiles);
				break;
			case PerkTargetingDefinition.E_TargetingMethod.ClosestDamageablesInRange:
				GetDamageablesInRange(perk, TargetingReferenceTileObject, num2, num, shuffle: false, ref targetTiles);
				break;
			case PerkTargetingDefinition.E_TargetingMethod.Self:
			{
				IDamageable damageable3 = TargetingReferenceTileObject.OriginTile.GetDamageable(isUnitPriority: true);
				if (damageable3 != null && damageable3.CanBeDamaged() && (PerkTargetingDefinition.TargetHasStatus == TheLastStand.Model.Status.Status.E_StatusType.None || (damageable3 is Unit unit4 && unit4.StatusOwned.HasFlag(PerkTargetingDefinition.TargetHasStatus) != PerkTargetingDefinition.HasStatusInverted)))
				{
					targetTiles.Add(TargetingReferenceTileObject.OriginTile);
				}
				break;
			}
			case PerkTargetingDefinition.E_TargetingMethod.PlayableUnitsInRange:
				foreach (Tile item2 in (from t in TargetingReferenceTileObject.TileObjectController.GetTilesInRange(num2)
					where !t.HasFog && t.Unit != null && t.Unit is PlayableUnit playableUnit && playableUnit != perk.Owner && playableUnit.CanBeDamaged()
					select t).ToList())
				{
					if (targetTiles.Count >= num)
					{
						break;
					}
					Unit unit = item2.Unit;
					if (unit.IsTargetableByAI() && (PerkTargetingDefinition.TargetHasStatus == TheLastStand.Model.Status.Status.E_StatusType.None || unit.StatusOwned.HasFlag(PerkTargetingDefinition.TargetHasStatus) != PerkTargetingDefinition.HasStatusInverted))
					{
						targetTiles.Add(item2);
					}
				}
				break;
			}
		}
		return targetTiles;
	}

	private void GetDamageablesInRange(Perk perk, ITileObject TargetingReferenceTileObject, int range, int amount, bool shuffle, ref HashSet<Tile> targetTiles)
	{
		List<Tile> tilesInRange = TargetingReferenceTileObject.TileObjectController.GetTilesInRange(range);
		List<Tile> list = new List<Tile>();
		foreach (Tile item in tilesInRange)
		{
			IDamageable damageable = item.GetDamageable(isUnitPriority: true);
			if (!item.HasFog && damageable != null && damageable.CanBeDamaged() && damageable != perk.Owner)
			{
				List<DamageableType> validDamageableTypes = PerkTargetingDefinition.ValidDamageableTypes;
				if ((validDamageableTypes == null || validDamageableTypes.Contains(damageable.DamageableType)) && (PerkTargetingDefinition.TargetHasStatus == TheLastStand.Model.Status.Status.E_StatusType.None || (damageable is Unit unit && unit.StatusOwned.HasFlag(PerkTargetingDefinition.TargetHasStatus) != PerkTargetingDefinition.HasStatusInverted)))
				{
					list.Add(item);
				}
			}
		}
		if (shuffle && amount != int.MaxValue)
		{
			list = RandomManager.Shuffle(TPSingleton<PlayableUnitManager>.Instance, list).ToList();
		}
		foreach (Tile item2 in list)
		{
			if (targetTiles.Count >= amount)
			{
				break;
			}
			IDamageable damageable2 = item2.GetDamageable(isUnitPriority: true);
			if (damageable2 == null || damageable2.IsTargetableByAI())
			{
				targetTiles.Add(item2);
			}
		}
	}

	public HashSet<Tile> GetTargetTiles(PerkDataContainer data, Perk perk, List<Tile> validTargets = null)
	{
		HashSet<Tile> result = new HashSet<Tile>();
		bool flag = PerkTargetingDefinition.TargetingReference != PerkTargetingDefinition.E_TargetingReference.Owner;
		if (data == null && flag)
		{
			return result;
		}
		List<ITileObject> targetingReference = GetTargetingReference(data, perk);
		return GetTargetFromBase(data, perk, targetingReference, validTargets);
	}
}
