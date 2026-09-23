using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động dọn dẹp/bới rác/thu gom tàn tích (Scavenge Effect).
/// Mang lại Vàng, Vật liệu, Linh hồn bị nguyền rủa (Damned Souls), trang bị hoặc sát thương lên công trình sau khi dọn dẹp.
/// </summary>
public class ScavengeBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Fields & Properties

	private int gainDamnedSouls;

	private int gainGold;

	private int gainMaterials;

	/// <summary>
	/// Danh sách định nghĩa tạo trang bị/vật phẩm khi dọn dẹp tàn tích.
	/// </summary>
	public List<CreateItemDefinition> CreateItemDefinitions { get; } = new List<CreateItemDefinition>();

	/// <summary>
	/// Sát thương gây ra lên công trình sau mỗi lần dọn dẹp/thu gom tàn tích.
	/// </summary>
	public int Damage { get; private set; }

	/// <summary>
	/// Số Vàng nhận được (đã tính bonus từ Glyph).
	/// </summary>
	public int GainGold
	{
		get
		{
			int num = 0;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += TPSingleton<GlyphManager>.Instance.GoldScavengingPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainGold * num2);
		}
	}

	/// <summary>
	/// Số Linh hồn bị nguyền rủa nhận được (đã tính bonus từ Apocalypse & Glyph).
	/// </summary>
	public int GainDamnedSouls
	{
		get
		{
			uint num = TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += (uint)TPSingleton<GlyphManager>.Instance.DamnedSoulsScavengingPercentageModifier;
				num += TPSingleton<GlyphManager>.Instance.DamnedSoulsPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainDamnedSouls * num2);
		}
	}

	/// <summary>
	/// Số Vật liệu nhận được (đã tính bonus từ Glyph).
	/// </summary>
	public int GainMaterials
	{
		get
		{
			int num = 0;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num += TPSingleton<GlyphManager>.Instance.MaterialScavengingPercentageModifier;
			}
			float num2 = 1f + (float)num / 100f;
			return (int)((float)gainMaterials * num2);
		}
	}

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng dọn dẹp tàn tích từ dữ liệu XML.
	/// </summary>
	public ScavengeBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML (Deserialize) cho phần thưởng tài nguyên, vật phẩm và sát thương dọn dẹp.
	/// </summary>
	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XElement xElement2 = xElement.Element("GainGold");
		if (!xElement2.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				Debug.LogError("A ScavengeGold Building ActionEffect must have a valid GainGold (int)");
				return;
			}
			gainGold = result;
		}
		XElement xElement3 = xElement.Element("GainMaterials");
		if (!xElement3.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement3.Value, out var result2))
			{
				Debug.LogError("A ScavengeMaterials Building ActionEffect must have a valid GainMaterials (int)");
				return;
			}
			gainMaterials = result2;
		}
		XElement xElement4 = xElement.Element("GainDamnedSouls");
		if (!xElement4.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement4.Value, out var result3))
			{
				Debug.LogError("A ScavengeDamnedSouls Building ActionEffect must have a valid GainDamnedSouls (int)");
				return;
			}
			gainDamnedSouls = result3;
		}
		foreach (XElement item in xElement.Elements("CreateItem"))
		{
			CreateItemDefinitions.Add(new CreateItemDefinition(item));
		}
		XElement xElement5 = xElement.Element("Damage");
		int result4;
		if (xElement5.IsNullOrEmpty())
		{
			Debug.LogError("A ScavengeGold Building ActionEffect must have a Damage element");
		}
		else if (!int.TryParse(xElement5.Value, out result4))
		{
			Debug.LogError("A ScavengeGold Building ActionEffect must have a valid Damage (int)");
		}
		else
		{
			Damage = result4;
		}
	}

	#endregion
}

