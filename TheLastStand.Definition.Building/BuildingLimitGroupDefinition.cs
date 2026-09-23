using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa nhóm giới hạn số lượng xây dựng (Building Limit Group) trong thành phố.
/// Giới hạn số lượng công trình tối đa thuộc nhóm có thể xây dựng, đồng thời tính toán các bonus từ Meta Upgrades và Glyphs.
/// </summary>
public class BuildingLimitGroupDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Fields & Properties

	/// <summary>
	/// Danh sách các ID công trình thuộc nhóm giới hạn này.
	/// </summary>
	public List<string> BuildingIds = new List<string>();

	/// <summary>
	/// Giới hạn gốc/mặc định (chưa áp dụng bonus).
	/// </summary>
	private int nativeLimit = -1;

	/// <summary>
	/// Mã ID định danh nhóm giới hạn.
	/// </summary>
	public string Id { get; set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa nhóm giới hạn công trình từ dữ liệu XML.
	/// </summary>
	public BuildingLimitGroupDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Lấy giới hạn xây dựng thực tế của nhóm (đã bao gồm các chỉ số thưởng từ Meta Upgrade và Glyph).
	/// </summary>
	/// <param name="useDefault">True để lấy giới hạn gốc mặc định không bao gồm bonus.</param>
	/// <returns>Số lượng công trình tối đa được phép xây dựng.</returns>
	public int GetBuildLimit(bool useDefault = false)
	{
		if (useDefault)
		{
			return nativeLimit;
		}
		int num = 0;
		if (nativeLimit != -1)
		{
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<BuildingModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int num2 = effects.Length - 1; num2 >= 0; num2--)
				{
					if (effects[num2].BuildingId == Id && effects[num2].MaxCityInstancesBonus != -1)
					{
						num += effects[num2].MaxCityInstancesBonus;
					}
				}
			}
			num += TPSingleton<GlyphManager>.Instance.BuildLimitModifiers.GetValueOrDefault(Id);
		}
		return nativeLimit + num;
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho nhóm giới hạn công trình.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id").Value;
		XAttribute xAttribute = xElement.Attribute("Limit");
		if (xAttribute.IsNullOrEmpty())
		{
			return;
		}
		if (sbyte.TryParse(xAttribute.Value, out var result))
		{
			if (result == 0)
			{
				Debug.LogError("BuildingLimitGroupDefinition " + Id + " has an invalid Limit (must be in the range -1 to 127, excluding 0)!");
			}
			else
			{
				nativeLimit = result;
			}
		}
		else
		{
			Debug.LogError("BuildingLimitGroupDefinition " + Id + " has an invalid Limit (must be in the range -1 to 127, excluding 0)!");
		}
	}

	#endregion
}

