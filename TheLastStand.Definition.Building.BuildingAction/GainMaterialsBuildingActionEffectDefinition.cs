using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động công trình thưởng Vật liệu (Gain Materials Effect).
/// </summary>
public class GainMaterialsBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Properties

	/// <summary>
	/// Số lượng Vật liệu nhận được.
	/// </summary>
	public int GainMaterials { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng nhận Vật liệu từ dữ liệu XML.
	/// </summary>
	public GainMaterialsBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho số Vật liệu thưởng.
	/// </summary>
	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		if (!xElement.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement.Value, out var result))
			{
				Debug.LogError("A GainMaterials Building ActionEffect must have a valid GainMaterials (int)");
			}
			else
			{
				GainMaterials = result;
			}
		}
	}

	#endregion
}

