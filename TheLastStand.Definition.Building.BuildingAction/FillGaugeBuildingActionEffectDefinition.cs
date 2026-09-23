using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động làm đầy thanh tiến trình/năng lượng sản xuất (Fill Gauge Effect).
/// </summary>
public class FillGaugeBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Properties

	/// <summary>
	/// Lượng tiến trình/năng lượng làm đầy.
	/// </summary>
	public int Amount { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng làm đầy thanh Gauge từ dữ liệu XML.
	/// </summary>
	public FillGaugeBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho thuộc tính Amount làm đầy.
	/// </summary>
	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = (xContainer as XElement).Element("Amount");
		int result;
		if (xElement.IsNullOrEmpty())
		{
			Debug.LogError("A FillGauge Building ActionEffect must have an Amount element");
		}
		else if (!int.TryParse(xElement.Value, out result))
		{
			Debug.LogError("A FillGauge Building ActionEffect must have a valid Amount (int)");
		}
		else
		{
			Amount = result;
		}
	}

	#endregion
}

