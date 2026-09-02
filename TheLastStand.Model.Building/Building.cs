using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Controller.Building;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Trap;
using TheLastStand.Manager.Turret;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building;
using TheLastStand.View;
using TheLastStand.View.Building;
using TheLastStand.View.TileMap;

namespace TheLastStand.Model.Building;

public class Building : ISerializable, IDeserializable, IBossPhaseActor, ITileObject, IEntity
{
	public static class Constants
	{
		public static class Ids
		{
			public const string ArmorMaker = "ArmorMaker";

			public const string Barricade = "Barricade";

			public const string Catapult = "Catapult";

			public const string ReinforcedStoneWall = "StoneWallReinforced";

			public const string Blacksmith = "Blacksmith";

			public const string Bowyer = "Bowyer";

			public const string Inn = "Inn";

			public const string LightFogSpawnerAlive = "LightFogSpawner_Alive";

			public const string MagicCircle = "MagicCircle";

			public const string MagicShop = "MagicShop";

			public const string PotionMaker = "PotionMaker";

			public const string Seer = "Seer";

			public const string Shop = "Shop";

			public const string TrinketMaker = "TrinketMaker";

			public const string Teleporter = "Teleporter";

			public const string Temple = "Temple";

			public const string ManaWell = "ManaWell";

			public const string Crystal = "Crystal";

			public const string SeedsIdsList = "Seeds";

			public const string SacredBallista = "SacredBallista";
		}

		public static class ShadowsType
		{
			public const string OneTile = "SingleShadow";

			public const string RuleTile = "TilingShadow";
		}

		public static class SidewalkType
		{
			public const string None = "None";

			public const string Sidewalk = "Sidewalk";
		}
	}

	public class StringToBuildingIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(BuildingDatabase.BuildingDefinitions.Keys);
	}

	public class StringToOneTileBuildingIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(GenericDatabase.IdsListDefinitions["OneTileBuildings"].Ids);
	}

	private string bossPhaseActorId;

	public bool DebugIsIndesctructible;

	public BuildingController BuildingController { get; }

	public BuildingDefinition BuildingDefinition { get; private set; }

	public BuildingView BuildingView { get; }

	public BattleModule BattleModule { get; private set; }

	public BlueprintModule BlueprintModule { get; private set; }

	public BrazierModule BrazierModule { get; private set; }

	public ConstructionModule ConstructionModule { get; private set; }

	public DamageableModule DamageableModule { get; private set; }

	public PassivesModule PassivesModule { get; private set; }

	public ProductionModule ProductionModule { get; private set; }

	public UpgradeModule UpgradeModule { get; private set; }

	public bool IsBarricade => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Barricade);

	public bool IsBonePile => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.BonePile);

	public bool IsLitBrazier => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.LitBrazier);

	public bool IsUnlitBrazier => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.UnlitBrazier);

	public bool IsBrazier
	{
		get
		{
			if (!IsLitBrazier)
			{
				return IsUnlitBrazier;
			}
			return true;
		}
	}

	public bool IsCrystal => BuildingDefinition.Id.Contains("Crystal");

	public bool IsDefensive => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Defensive);

	public bool IsGate => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Gate);

	public bool IsHandledDefense => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.HandledDefense);

	public bool IsLightFogSpawner => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.LightFogSpawner);

	public bool IsObstacle => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Obstacle);

	public bool IsProtoBridge => BuildingDefinition.Id.ToLower().Contains("bridge");

	public bool IsTeleporter => BuildingDefinition.Id == "Teleporter";

	public bool IsTrap => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Trap);

	public bool IsTurret => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Turret);

	public bool IsWall => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Wall);

	public bool IsWalkableHandledDefense => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.WalkableHandledDefense);

	public bool IsWatchtower => BuildingDefinition.BlueprintModuleDefinition.Category.HasFlag(BuildingDefinition.E_BuildingCategory.Watchtower);

	public bool BossActorDeathPrepared { get; private set; }

	public bool IsBossPhaseActor => BossPhaseActorId != null;

	public string BossPhaseActorId
	{
		get
		{
			return bossPhaseActorId;
		}
		set
		{
			if (!(BossPhaseActorId == value))
			{
				if (BossPhaseActorId != null)
				{
					TPSingleton<BossManager>.Instance.BossPhaseActors.TryRemoveAtKey(BossPhaseActorId, this);
				}
				bossPhaseActorId = value;
				TPSingleton<BossManager>.Instance.BossPhaseActors.AddAtKey(BossPhaseActorId, (IBossPhaseActor)this);
			}
		}
	}

	public bool CanSpawnEnemyOnIt
	{
		get
		{
			if (!IsTrap && !IsTeleporter && !IsProtoBridge)
			{
				return IsWalkableHandledDefense;
			}
			return true;
		}
	}

	public string Id => BuildingDefinition.Id;

	public bool IsDemolishableIfIndestructible
	{
		get
		{
			if (!IsTrap && !IsTeleporter && !IsProtoBridge)
			{
				return IsWalkableHandledDefense;
			}
			return true;
		}
	}

	public bool ShouldWaitDeathLikeEffect
	{
		get
		{
			DamageableModule damageableModule = DamageableModule;
			if (damageableModule == null || !damageableModule.IsDestroyAnimating)
			{
				BrazierModule brazierModule = BrazierModule;
				if (brazierModule == null || !brazierModule.IsExtinguishing)
				{
					return BattleModule?.IsDeathRattling ?? false;
				}
			}
			return true;
		}
	}

	public string Name => Id;

	public int RandomId { get; protected set; }

	public List<Tile> OccupiedTiles => BlueprintModule.OccupiedTiles;

	public Tile OriginTile { get; set; }

	public ITileObjectController TileObjectController => BlueprintModule.BlueprintModuleController;

	public ITileObjectDefinition TileObjectDefinition => BlueprintModule.BlueprintModuleDefinition;

	public ITileObjectView TileObjectView => BuildingView;

	public string UniqueIdentifier => $"{Id}_{RandomId}";

	public bool IsInCity => OccupiedTiles.Any((Tile tile) => tile.GroundDefinition.GroundCategory == GroundDefinition.E_GroundCategory.City);

	public bool IsInWorld => OriginTile != null;

	public Building(SerializedBuilding container, BuildingController buildingController, BuildingView buildingView)
	{
		BuildingController = buildingController;
		BuildingView = buildingView;
		Deserialize(container);
	}

	public Building(BuildingDefinition buildingDefinition, BuildingController buildingController, BuildingView buildingView, Tile tile)
	{
		RandomId = RandomManager.GetRandomRange(TPSingleton<BuildingManager>.Instance, 0, int.MaxValue);
		BuildingDefinition = buildingDefinition;
		OriginTile = tile;
		BuildingController = buildingController;
		BuildingView = buildingView;
	}

	public virtual void Init()
	{
		CreateModules();
		BattleModule?.FindSkillsFromDefinition();
	}

	public virtual void Init(SerializedBuilding container)
	{
		CreateModules();
		DeserializeModules(container);
		if (IsTrap)
		{
			TPSingleton<TileMapView>.Instance.DisplayBuildingInstantly(this, OriginTile, (BattleModule.RemainingTrapCharges == 0) ? "_Disabled" : string.Empty);
		}
		else
		{
			TPSingleton<TileMapView>.Instance.DisplayBuildingInstantly(this, OriginTile);
		}
	}

	public bool IsTargetableByAI()
	{
		return BlueprintModule.IsTargetableByAI();
	}

	public void PrepareBossActorDeath()
	{
		if (!BossActorDeathPrepared)
		{
			TPSingleton<BossManager>.Instance.BossPhaseActorsKills.AddValueOrCreateKey(BossPhaseActorId, 1, (int a, int b) => a + b);
			BossActorDeathPrepared = true;
		}
	}

	public void TryCreateEmptyProductionModule()
	{
		if (ProductionModule == null)
		{
			ProductionModule productionModule = (ProductionModule = new ProductionModuleController(BuildingController, BuildingDefinition.ProductionModuleDefinition).ProductionModule);
		}
	}

	private void CreateModules()
	{
		BlueprintModule = (IsGate ? new GateBlueprintModuleController(BuildingController, BuildingDefinition.BlueprintModuleDefinition).GateBlueprintModule : new BlueprintModuleController(BuildingController, BuildingDefinition.BlueprintModuleDefinition).BlueprintModule);
		ConstructionModule = (IsTrap ? new TrapConstructionModuleController(BuildingController, BuildingDefinition.ConstructionModuleDefinition).ConstructionModule : new ConstructionModuleController(BuildingController, BuildingDefinition.ConstructionModuleDefinition).ConstructionModule);
		if (BuildingDefinition.UpgradeModuleDefinition != null)
		{
			UpgradeModule = new UpgradeModuleController(BuildingController, BuildingDefinition.UpgradeModuleDefinition).UpgradeModule;
		}
		if (BuildingDefinition.PassivesModuleDefinition != null)
		{
			PassivesModule = new PassivesModuleController(BuildingController, BuildingDefinition.PassivesModuleDefinition).PassivesModule;
		}
		if (BuildingDefinition.DamageableModuleDefinition != null)
		{
			DamageableModule = (IsTrap ? new TrapDamageableModuleController(BuildingController, BuildingDefinition.DamageableModuleDefinition).DamageableModule : new DamageableModuleController(BuildingController, BuildingDefinition.DamageableModuleDefinition).DamageableModule);
		}
		if (BuildingDefinition.ProductionModuleDefinition != null)
		{
			ProductionModule = new ProductionModuleController(BuildingController, BuildingDefinition.ProductionModuleDefinition).ProductionModule;
		}
		if (BuildingDefinition.BattleModuleDefinition != null)
		{
			BattleModule = new BattleModuleController(BuildingController, BuildingDefinition.BattleModuleDefinition).BattleModule;
		}
		if (BuildingDefinition.BrazierModuleDefinition != null)
		{
			BrazierModule = new BrazierModuleController(BuildingController, BuildingDefinition.BrazierModuleDefinition).BrazierModule;
		}
	}

	public void Log(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		message = $"[{UniqueIdentifier}]: {message}";
		if (IsTurret)
		{
			TPSingleton<TurretManager>.Instance.Log(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
		else if (IsTrap)
		{
			TPSingleton<TrapManager>.Instance.Log(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.Log(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
	}

	public void LogError(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		message = $"[{UniqueIdentifier}]: {message}";
		if (IsTurret)
		{
			TPSingleton<TurretManager>.Instance.LogError(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
		else if (IsTrap)
		{
			TPSingleton<TrapManager>.Instance.LogError(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.LogError(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
	}

	public void LogWarning(object message, CLogLevel logLevel = CLogLevel.NORMAL, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		message = $"[{UniqueIdentifier}]: {message}";
		if (IsTurret)
		{
			TPSingleton<TurretManager>.Instance.LogWarning(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
		else if (IsTrap)
		{
			TPSingleton<TrapManager>.Instance.LogWarning(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.LogWarning(message, BuildingView, logLevel, forcePrintInUnity, printStackTrace);
		}
	}

	public virtual void Deserialize(ISerializedData container, int saveVersion = -1)
	{
		SerializedBuilding serializedBuilding = container as SerializedBuilding;
		if (!BuildingDatabase.BuildingDefinitions.ContainsKey(serializedBuilding.Id))
		{
			TPSingleton<BuildingManager>.Instance.LogWarning("Building " + serializedBuilding.Id + " was found during deserialization, but is unknown to the database definition list (" + string.Join(", ", BuildingDatabase.BuildingDefinitions.Keys) + "). This building will be skipped!");
		}
		else
		{
			RandomId = ((serializedBuilding.RandomId == -1) ? RandomManager.GetRandomRange(TPSingleton<BuildingManager>.Instance, 0, int.MaxValue) : serializedBuilding.RandomId);
			BuildingDefinition = BuildingDatabase.BuildingDefinitions[serializedBuilding.Id];
			OriginTile = TileMapManager.GetTile(serializedBuilding.Position.X, serializedBuilding.Position.Y);
			if (!string.IsNullOrEmpty(serializedBuilding.BossPhaseActorId))
			{
				BossPhaseActorId = serializedBuilding.BossPhaseActorId;
			}
		}
	}

	private void DeserializeModules(ISerializedData container)
	{
		SerializedBuilding serializedBuilding = container as SerializedBuilding;
		DamageableModule?.Deserialize(serializedBuilding);
		BattleModule?.Deserialize(serializedBuilding);
		if (BuildingDefinition.ProductionModuleDefinition == null && serializedBuilding.Level > 1)
		{
			TryCreateEmptyProductionModule();
		}
		ProductionModule?.Deserialize(serializedBuilding);
		BrazierModule?.Deserialize(serializedBuilding);
	}

	public virtual ISerializedData Serialize()
	{
		SerializedBuilding serializedBuilding = new SerializedBuilding
		{
			BossPhaseActorId = BossPhaseActorId,
			Id = BuildingDefinition.Id,
			RandomId = RandomId,
			Position = new SerializableVector2Int(OriginTile.Position)
		};
		DamageableModule?.Serialize(serializedBuilding);
		UpgradeModule?.Serialize(serializedBuilding);
		PassivesModule?.Serialize(serializedBuilding);
		BattleModule?.Serialize(serializedBuilding);
		ProductionModule?.Serialize(serializedBuilding);
		BrazierModule?.Serialize(serializedBuilding);
		return serializedBuilding;
	}
}
