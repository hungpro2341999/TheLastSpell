using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Serialization.Building;
using TheLastStand.View;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class DamageableModule : BuildingModule, IDamageable, IEntity
{
	public enum E_State
	{
		Error = -1,
		Alive,
		Dead
	}

	public DamageableModuleController DamageableModuleController => base.BuildingModuleController as DamageableModuleController;

	public DamageableModuleDefinition DamageableModuleDefinition => base.BuildingModuleDefinition as DamageableModuleDefinition;

	public IDamageableController DamageableController => DamageableModuleController;

	public DamageableType DamageableType
	{
		get
		{
			if (!base.BuildingParent.IsObstacle)
			{
				return DamageableType.Building;
			}
			return DamageableType.Obstacle;
		}
	}

	public IDamageableView DamageableView => base.BuildingParent.BuildingView;

	public float Armor
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float ArmorTotal => 0f;

	public float Health { get; set; }

	public float HealthTotal { get; set; }

	public bool IsDead => State == E_State.Dead;

	public bool IsDestroyAnimating { get; set; }

	public bool IsUnderDamagedThreshold => Health / HealthTotal < 0.5f;

	public int RandomId => base.BuildingParent.RandomId;

	public E_State State { get; set; }

	public string UniqueIdentifier => base.BuildingParent.UniqueIdentifier;

	public DamageableModule(Building buildingParent, DamageableModuleDefinition damageableModuleDefinition, DamageableModuleController damageableModuleController)
		: base(buildingParent, damageableModuleDefinition, damageableModuleController)
	{
		HealthTotal = DamageableModuleDefinition.HealthTotal;
		Health = HealthTotal;
	}

	public bool CanBeDamaged()
	{
		return true;
	}

	public virtual bool IsTargetableByAI()
	{
		return base.BuildingParent.IsTargetableByAI();
	}

	public void Deserialize(SerializedBuilding buildingElement)
	{
		Health = Mathf.Min(buildingElement.Health, HealthTotal);
	}

	public void Serialize(SerializedBuilding buildingElement)
	{
		buildingElement.Health = Health;
	}

	public void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		base.BuildingParent.Log(message, logLevel, forcePrintInUnity, printStackTrace);
	}

	public void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		base.BuildingParent.LogError(message, logLevel, forcePrintInUnity, printStackTrace);
	}

	public void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		base.BuildingParent.LogWarning(message, logLevel, forcePrintInUnity, printStackTrace);
	}
}
