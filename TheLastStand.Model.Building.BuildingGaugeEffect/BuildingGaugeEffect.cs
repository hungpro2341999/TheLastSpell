using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;

namespace TheLastStand.Model.Building.BuildingGaugeEffect;

public abstract class BuildingGaugeEffect : ISerializable, IDeserializable
{
	public ProductionModule ProductionBuilding { get; }

	public BuildingGaugeEffectController BuildingGaugeEffectController { get; }

	public BuildingGaugeEffectDefinition BuildingGaugeEffectDefinition { get; }

	public BuildingGaugeEffectView BuildingGaugeEffectView { get; }

	public int Units { get; set; }

	public int UnitsThreshold => BuildingGaugeEffectDefinition.FirstGaugeUnits;

	public BuildingGaugeEffect(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition, BuildingGaugeEffectController controller, BuildingGaugeEffectView buildingGaugeEffectView)
	{
		ProductionBuilding = productionBuilding;
		BuildingGaugeEffectDefinition = definition;
		BuildingGaugeEffectController = controller;
		BuildingGaugeEffectView = buildingGaugeEffectView;
	}

	public int GetProductionValue()
	{
		return GetGaugeLoopsCountPerProduction() * GetOneLoopProductionValue();
	}

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedGaugeEffect serializedGaugeEffect = container as SerializedGaugeEffect;
		Units = serializedGaugeEffect.Units;
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedGaugeEffect
		{
			Id = BuildingGaugeEffectDefinition.Id,
			Units = Units
		};
	}

	protected int GetGaugeLoopsCountPerProduction()
	{
		return GetFillEffectValue() / UnitsThreshold;
	}

	private int GetFillEffectValue()
	{
		if (ProductionBuilding.BuildingParent.PassivesModule?.BuildingPassives != null)
		{
			foreach (TheLastStand.Model.Building.BuildingPassive.BuildingPassive buildingPassife in ProductionBuilding.BuildingParent.PassivesModule.BuildingPassives)
			{
				foreach (BuildingPassiveEffect passiveEffect in buildingPassife.PassiveEffects)
				{
					if (passiveEffect is FillEffectGauge fillEffectGauge)
					{
						return fillEffectGauge.FillEffectGaugeController.ComputePassiveValue();
					}
				}
			}
		}
		return 0;
	}

	public virtual int GetOneLoopProductionValue()
	{
		return 1;
	}
}
