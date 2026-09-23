using System;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động công trình hồi Năng lượng (Heal Mana Effect) cho tướng/đơn vị.
/// </summary>
public class HealManaBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Constants

	private static class Constants
	{
		public const string ActionEstimationIconId = "Mana";
	}

	#endregion

	#region Properties

	/// <summary>
	/// Lượng Mana được hồi phục.
	/// </summary>
	public int Amount { get; private set; }

	/// <summary>
	/// Phạm vi mục tiêu được hồi Mana (Tất cả tướng hay 1 tướng).
	/// </summary>
	public E_BuildingActionTargeting BuildingActionTargeting { get; private set; }

	/// <summary>
	/// ID biểu tượng ước tính hiệu ứng Mana trên giao diện UI.
	/// </summary>
	public override string ActionEstimationIconId => "Mana";

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng hồi Mana từ dữ liệu XML.
	/// </summary>
	public HealManaBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho lượng Mana hồi phục và mục tiêu tác động.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (container is XElement xElement)
		{
			XElement xElement2 = xElement.Element("Amount");
			if (xElement2 != null)
			{
				if (!int.TryParse(xElement2.Value, out var result))
				{
					CLoggerManager.Log("HealMana Amount " + HasAnInvalid("int", xElement2.Value), LogType.Error);
					return;
				}
				Amount = result;
				XElement xElement3 = xElement.Element("Target");
				if (xElement3 != null)
				{
					if (!Enum.TryParse<E_BuildingActionTargeting>(xElement3.Value, out var result2))
					{
						CLoggerManager.Log("HealMana Target " + HasAnInvalid("E_Target", xElement3.Value), LogType.Error);
					}
					else
					{
						BuildingActionTargeting = result2;
					}
				}
			}
			else
			{
				CLoggerManager.Log("HealMana must have a Bonus", LogType.Error);
			}
		}
		else
		{
			CLoggerManager.Log("HealMana doesn't have a XElement", LogType.Error);
		}
	}

	#endregion
}

