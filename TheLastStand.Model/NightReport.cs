using System.Collections.Generic;
using TheLastStand.Controller;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization;

namespace TheLastStand.Model;

public class NightReport : ISerializable, IDeserializable
{
	public int BattleRank { get; set; }

	public List<KillReportData> KillsThisNight { get; } = new List<KillReportData>();

	public NightReportController NightReportController { get; }

	public int PanicRank { get; set; }

	public float TonightHpLost { get; set; }

	public int TonightRank { get; set; }

	public NightReport(NightReportController controller)
	{
		NightReportController = controller;
	}

	public int GetTotalKillsThisNightForEntity(IEntity entity)
	{
		int num = 0;
		for (int i = 0; i < KillsThisNight.Count; i++)
		{
			num += KillsThisNight[i].GetKillAmountForEntity(entity);
		}
		return num;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (saveVersion < 18 || container == null)
		{
			return;
		}
		SerializedNightReport serializedNightReport = container as SerializedNightReport;
		int i = 0;
		for (int count = serializedNightReport.SerializedKillReportDataContainer.Count; i < count; i++)
		{
			SerializedKillReportData serializedData = serializedNightReport.SerializedKillReportDataContainer[i];
			EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = EnemyUnitDatabase.EnemyUnitTemplateDefinitions.GetValueOrDefault(serializedData.EnemyUnitSpecificId) ?? EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.GetValueOrDefault(serializedData.EnemyUnitSpecificId);
			if (enemyUnitTemplateDefinition != null)
			{
				KillsThisNight.Add(new KillReportData(serializedData, enemyUnitTemplateDefinition));
			}
		}
	}

	public ISerializedData Serialize()
	{
		List<SerializedKillReportData> list = new List<SerializedKillReportData>();
		int i = 0;
		for (int count = KillsThisNight.Count; i < count; i++)
		{
			list.Add(KillsThisNight[i].Serialize());
		}
		return new SerializedNightReport
		{
			SerializedKillReportDataContainer = list
		};
	}
}
