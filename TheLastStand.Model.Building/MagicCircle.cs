using TheLastStand.Controller.Building;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;
using TheLastStand.View.Building;
using UnityEngine;

namespace TheLastStand.Model.Building;

public class MagicCircle : Building
{
	public int MageCount { get; set; }

	public float MageLife => base.DamageableModule.HealthTotal / (float)MagicCircleDefinition.MageSlotMax;

	public int MageSlots { get; set; }

	public MagicCircleDefinition MagicCircleDefinition => base.BuildingDefinition as MagicCircleDefinition;

	public MagicCircleView MagicCircleView => base.BuildingView as MagicCircleView;

	public float CurrentHealthTotal => MageLife * (float)MageSlots;

	public MagicCircle(SerializedBuilding container, BuildingController controller, MagicCircleView view)
		: base(container, controller, view)
	{
	}

	public MagicCircle(MagicCircleDefinition magicCircleDefinition, BuildingController controller, MagicCircleView view, Tile tile)
		: base(magicCircleDefinition, controller, view, tile)
	{
	}

	public override void Init()
	{
		base.Init();
		int num = 0;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<AdditionalInitMagesMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			AdditionalInitMagesMetaEffectDefinition[] array = effects;
			foreach (AdditionalInitMagesMetaEffectDefinition additionalInitMagesMetaEffectDefinition in array)
			{
				num += additionalInitMagesMetaEffectDefinition.Amount;
			}
		}
		MageCount = MagicCircleDefinition.MageCountInit + num;
		MageSlots = MagicCircleDefinition.MageSlotInit + num;
		ApplyGlyphsHealthBonus();
		base.DamageableModule.Health = (float)MageCount * MageLife;
		float magicCircleStartingHealthMultiplier = ApocalypseManager.CurrentApocalypse.MagicCircleStartingHealthMultiplier;
		if (magicCircleStartingHealthMultiplier < 1f)
		{
			int num2 = Mathf.RoundToInt((float)MageCount * MageLife * magicCircleStartingHealthMultiplier);
			MageCount = Mathf.CeilToInt((float)num2 / MageLife);
			base.DamageableModule.Health = num2;
		}
	}

	public override void Init(SerializedBuilding container)
	{
		base.Init(container);
		ApplyGlyphsHealthBonus();
	}

	private void ApplyGlyphsHealthBonus()
	{
		_ = ApplicationManager.Application.State.GetName() == "LevelEditor";
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedBuilding serializedBuilding = container as SerializedBuilding;
		base.Deserialize(serializedBuilding, saveVersion);
		SerializedMagicCicleSettings magicCircleSettings = serializedBuilding.MagicCircleSettings;
		MageSlots = magicCircleSettings.MageSlots;
		MageCount = magicCircleSettings.MageCount;
	}

	public override ISerializedData Serialize()
	{
		SerializedBuilding obj = base.Serialize() as SerializedBuilding;
		SerializedMagicCicleSettings magicCircleSettings = new SerializedMagicCicleSettings
		{
			MageSlots = MageSlots,
			MageCount = MageCount
		};
		obj.MagicCircleSettings = magicCircleSettings;
		return obj;
	}
}
