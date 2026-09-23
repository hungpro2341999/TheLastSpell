using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động đẩy lùi sương mù độc (Repel Fog Effect).
/// </summary>
public class RepelFogBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Properties

	/// <summary>
	/// Khoảng cách/số ô sương mù bị đẩy lùi.
	/// </summary>
	public int Amount { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng đẩy lùi sương mù từ dữ liệu XML.
	/// </summary>
	public RepelFogBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho khoảng cách đẩy lùi sương mù.
	/// </summary>
	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = (xContainer as XElement).Element("Amount");
		int result;
		if (xElement.IsNullOrEmpty())
		{
			Debug.LogError("A RepelFog Building ActionEffect must have an Amount element");
		}
		else if (!int.TryParse(xElement.Value, out result))
		{
			Debug.LogError("A RepelFog Building ActionEffect must have a valid Amount (int)");
		}
		else
		{
			Amount = result;
		}
	}

	#endregion
}

