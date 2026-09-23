using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động công trình thưởng Vàng (Gain Gold Effect).
/// </summary>
public class GainGoldBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Properties

	/// <summary>
	/// Số lượng Vàng nhận được.
	/// </summary>
	public int GainGold { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng nhận Vàng từ dữ liệu XML.
	/// </summary>
	public GainGoldBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho số Vàng thưởng.
	/// </summary>
	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		if (!xElement.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement.Value, out var result))
			{
				Debug.LogError("A GainGold Building ActionEffect must have a valid GainGold (int)");
			}
			else
			{
				GainGold = result;
			}
		}
	}

	#endregion
}

