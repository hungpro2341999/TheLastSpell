using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động công trình nâng cấp chỉ số (Upgrade Stat Effect) cho tướng/đơn vị (Máu tối đa, Mana tối đa, Điểm hành động, Điểm di chuyển, v.v.).
/// </summary>
public class UpgradeStatBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Constants

	public static class Constants
	{
		public const string GainHealthMax = "GainHealthMax";

		public const string GainManaMax = "GainManaMax";
	}

	#endregion

	#region Properties

	/// <summary>
	/// Chỉ số của tướng (E_Stat) được tăng điểm từ hành động này.
	/// </summary>
	public UnitStatDefinition.E_Stat Stat { get; private set; }

	/// <summary>
	/// Giá trị chỉ số thưởng cộng thêm.
	/// </summary>
	public int Bonus { get; private set; }

	/// <summary>
	/// Phạm vi mục tiêu nhận thưởng chỉ số (Tất cả tướng hay 1 tướng).
	/// </summary>
	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	/// <summary>
	/// ID biểu tượng ước tính hiệu ứng nâng cấp chỉ số tương ứng trên giao diện UI.
	/// </summary>
	public override string ActionEstimationIconId => Stat switch
	{
		UnitStatDefinition.E_Stat.HealthTotal => "GainHealthMax", 
		UnitStatDefinition.E_Stat.ManaTotal => "GainManaMax", 
		UnitStatDefinition.E_Stat.MovePointsTotal => UnitStatDefinition.E_Stat.MovePoints.ToString(), 
		UnitStatDefinition.E_Stat.ActionPointsTotal => UnitStatDefinition.E_Stat.ActionPoints.ToString(), 
		_ => Stat.ToString(), 
	};

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng nâng cấp chỉ số từ dữ liệu XML.
	/// </summary>
	public UpgradeStatBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho loại chỉ số, điểm thưởng và mục tiêu tác động.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (container is XElement xElement)
		{
			XElement xElement2 = xElement.Element("Stat");
			if (xElement2 != null)
			{
				if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xElement2.Value, out var result))
				{
					CLoggerManager.Log("UpgradeStat Stat " + HasAnInvalid("E_Stat", xElement2.Value), LogType.Error);
					return;
				}
				Stat = result;
				XElement xElement3 = xElement.Element("Bonus");
				if (xElement3 != null)
				{
					if (!int.TryParse(xElement3.Value, out var result2))
					{
						CLoggerManager.Log("UpgradeStat Bonus " + HasAnInvalid("int", xElement3.Value), LogType.Error);
						return;
					}
					Bonus = result2;
					XElement xElement4 = xElement.Element("Target");
					if (xElement4 != null)
					{
						if (!Enum.TryParse<E_BuildingActionTargeting>(xElement4.Value, out var result3))
						{
							CLoggerManager.Log("UpgradeStat Target " + HasAnInvalid("E_Target", xElement4.Value), LogType.Error);
						}
						else
						{
							BuildingActionTargeting = result3;
						}
					}
					else
					{
						CLoggerManager.Log("UpgradeStat must have a Target", LogType.Error);
					}
				}
				else
				{
					CLoggerManager.Log("UpgradeStat must have a Bonus", LogType.Error);
				}
			}
			else
			{
				CLoggerManager.Log("UpgradeStat must have a stat", LogType.Error);
			}
		}
		else
		{
			CLoggerManager.Log("UpgradeStat doesn't have a XElement", LogType.Error);
		}
	}

	#endregion
}

