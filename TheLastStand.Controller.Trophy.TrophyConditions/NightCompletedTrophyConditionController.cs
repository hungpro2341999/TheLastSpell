using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Trophy;
using TheLastStand.Serialization.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class NightCompletedTrophyConditionController : TrophyConditionController
{
	public override string Name => "NightCompleted";

	public int ValueProgression { get; set; }

	public string CityId { get; set; }

	public NightCompletedTrophyDefinition NightCompletedTrophyDefinition => base.TrophyConditionDefinition as NightCompletedTrophyDefinition;

	public NightCompletedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " need 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is string cityId))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as second argument a string !", CLogLevel.DETAILED);
			return;
		}
		ValueProgression += num;
		CityId = cityId;
		OnValueChange();
	}

	public override void SetValue(params object[] args)
	{
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " need 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int valueProgression))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is string cityId))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as second argument a string !", CLogLevel.DETAILED);
			return;
		}
		ValueProgression = valueProgression;
		CityId = cityId;
		OnValueChange();
	}

	public override string ToString()
	{
		return $"{Name}: {ValueProgression}/{NightCompletedTrophyDefinition.Value} (CityId: {NightCompletedTrophyDefinition.CityId})" + "\r\n";
	}

	protected override void CheckCompleteState()
	{
		isCompleted = ValueProgression == NightCompletedTrophyDefinition.Value && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id == NightCompletedTrophyDefinition.CityId;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedNightCompletedTrophy serializedNightCompletedTrophy = container as SerializedNightCompletedTrophy;
		ValueProgression = serializedNightCompletedTrophy.ValueProgression;
		CityId = serializedNightCompletedTrophy.CityId;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedNightCompletedTrophy
		{
			Name = Name,
			ValueProgression = ValueProgression,
			CityId = CityId
		};
	}
}
