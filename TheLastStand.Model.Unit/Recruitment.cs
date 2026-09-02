using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Events;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit;

public class Recruitment : ISerializable, IDeserializable
{
	public List<PlayableUnit> CurrentGeneratedUnits { get; set; }

	public bool HasMage { get; set; }

	public float MageGenerationCurrentProbability { get; set; }

	public PlayableUnit SelectedUnit { get; set; }

	public int UnitLimitBonus { get; set; } = 1;

	public int UnitsLimit
	{
		get
		{
			int count = PlayableUnitDatabase.UnitsGenerationStartDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationDefinitionId].Count;
			count += UnitLimitBonus;
			if (GlyphManager.TryGetGlyphEffects(out List<GlyphBonusUnitsEffectDefinition> glyphEffects))
			{
				for (int num = glyphEffects.Count - 1; num >= 0; num--)
				{
					if (glyphEffects[num].IncreaseUnitsLimit)
					{
						count += glyphEffects[num].UnitGenerationDefinitions.Count;
					}
				}
			}
			return count;
		}
	}

	private int AveragePlayableUnitLevel => ComputeAveragePlayableUnitLevel();

	private int MaxPlayableUnitLevel => ComputeMaxPlayableUnitLevel();

	public Recruitment()
	{
	}

	public Recruitment(ISerializedData container, int saveVersion)
	{
		Deserialize(container, saveVersion);
	}

	public void ListenToBuildingDestroyEvent()
	{
		EventManager.AddListener(typeof(BuildingDestroyedEvent), OnBuildingDestroy);
	}

	private void OnBuildingDestroy(TheLastStand.Framework.EventSystem.Event e)
	{
		if ((e as BuildingDestroyedEvent).Id == "Inn")
		{
			UnitLimitBonus = 1;
		}
	}

	private int ComputeAveragePlayableUnitLevel()
	{
		float num = 0f;
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			num += (float)(int)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].Level;
		}
		return Mathf.RoundToInt(num / (float)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count);
	}

	private int ComputeMaxPlayableUnitLevel()
	{
		int num = 1;
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			if ((int)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].Level > num)
			{
				num = (int)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].Level;
			}
		}
		return num;
	}

	private int Random(double a, double b)
	{
		if (b > a)
		{
			double num = b;
			b = a;
			a = num;
		}
		int b2 = Mathf.RoundToInt(RandomManager.GetRandomRange(this, (float)a, (float)b));
		return Mathf.Max(1, b2);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedRecruitment serializedRecruitment))
		{
			return;
		}
		CurrentGeneratedUnits = new List<PlayableUnit>();
		int num = 0;
		int num2 = 0;
		foreach (SerializedPlayableUnit recruitableUnit in serializedRecruitment.RecruitableUnits)
		{
			if (recruitableUnit == null)
			{
				num++;
				num2++;
				CurrentGeneratedUnits.Add(null);
			}
			else
			{
				PlayableUnit playableUnit = new PlayableUnitController(recruitableUnit, saveVersion).PlayableUnit;
				PlayableUnitManager.SetPlayableUnitGhost(playableUnit, num - num2, snapshotOnly: true);
				num++;
				CurrentGeneratedUnits.Add(playableUnit);
			}
		}
		UnitLimitBonus = serializedRecruitment.UnitLimitBonus;
		MageGenerationCurrentProbability = serializedRecruitment.MageGenerationCurrentProbability;
		HasMage = serializedRecruitment.HasMage;
	}

	public ISerializedData Serialize()
	{
		SerializedRecruitment serializedRecruitment = new SerializedRecruitment();
		if (CurrentGeneratedUnits != null && CurrentGeneratedUnits.Count > 0)
		{
			serializedRecruitment.RecruitableUnits = CurrentGeneratedUnits.Select((PlayableUnit o) => o?.Serialize() as SerializedPlayableUnit).ToList();
		}
		serializedRecruitment.HasMage = HasMage;
		serializedRecruitment.MageGenerationCurrentProbability = MageGenerationCurrentProbability;
		serializedRecruitment.UnitLimitBonus = UnitLimitBonus;
		return serializedRecruitment;
	}
}
