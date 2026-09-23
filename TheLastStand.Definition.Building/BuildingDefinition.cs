using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa tổng quan cho một công trình (Building) trong game.
/// Chứa dữ liệu deserialize từ XML cho các module chức năng (Battle, Blueprint, Brazier, Construction, Damageable, Passives, Production, Upgrade).
/// </summary>
public class BuildingDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Enums

	/// <summary>
	/// Loại hiệu ứng hoạt ảnh xây dựng công trình.
	/// </summary>
	public enum E_ConstructionAnimationType
	{
		None,
		Instantaneous,
		Animated
	}

	/// <summary>
	/// Quy định phạm vi/thể tích chiếm chỗ của công trình đối với các ô xung quanh.
	/// </summary>
	public enum E_OccupationVolumeType
	{
		None,
		Adjacent,
		Ignore
	}

	/// <summary>
	/// Phân loại danh mục công trình (dạng Flag bitmask).
	/// </summary>
	[Flags]
	public enum E_BuildingCategory
	{
		None = 0,
		Obstacle = 1,
		Defensive = 2,
		Production = 4,
		LightFogSpawner = 9,
		LitBrazier = 0x11,
		UnlitBrazier = 0x21,
		Wall = 0x42,
		Watchtower = 0x82,
		Turret = 0x102,
		Trap = 0x202,
		HandledDefense = 0x402,
		Gate = 0x842,
		Barricade = 0x1002,
		BonePile = 0x3002,
		WalkableHandledDefense = 0x4402
	}

	/// <summary>
	/// Phân loại danh mục xây dựng.
	/// </summary>
	[Flags]
	public enum E_ConstructionCategory
	{
		None = 0,
		Defensive = 1,
		Production = 2,
		All = 3
	}

	#endregion

	#region Constants

	public static class Constants
	{
		public static class Ids
		{
			public const string Catapult = "Catapult";
		}

		public const float DamagedSpriteThreshold = 0.5f;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Định nghĩa module chiến đấu (kỹ năng tấn công/phòng thủ).
	/// </summary>
	public BattleModuleDefinition BattleModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module bản thiết kế (kích thước, loại danh mục, đồ họa).
	/// </summary>
	public BlueprintModuleDefinition BlueprintModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module thắp sáng/đốt đỉnh hương (Brazier).
	/// </summary>
	public BrazierModuleDefinition BrazierModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module xây dựng (chi phí, tài nguyên).
	/// </summary>
	public ConstructionModuleDefinition ConstructionModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module nhận sát thương / máu (HP, Armor).
	/// </summary>
	public DamageableModuleDefinition DamageableModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module nội tại (Passives).
	/// </summary>
	public PassivesModuleDefinition PassivesModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module sản xuất (tạo ra trang bị, vàng, vật liệu).
	/// </summary>
	public ProductionModuleDefinition ProductionModuleDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa module nâng cấp của công trình.
	/// </summary>
	public UpgradeModuleDefinition UpgradeModuleDefinition { get; private set; }

	/// <summary>
	/// Mô tả công trình (được bản cục hóa - Localization).
	/// </summary>
	public string Description => Localizer.Get("BuildingDescription_" + Id);

	/// <summary>
	/// Mã ID định danh duy nhất của công trình.
	/// </summary>
	public string Id { get; private set; }

	/// <summary>
	/// Tên hiển thị công trình (được bản cục hóa - Localization).
	/// </summary>
	public string Name => Localizer.Get("BuildingName_" + Id);

	/// <summary>
	/// Danh sách ID nhóm liên quan.
	/// </summary>
	public List<string> IdListIds { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa công trình từ dữ liệu XML container.
	/// </summary>
	public BuildingDefinition(XContainer buildingDefinitionContainer)
		: base(buildingDefinitionContainer)
	{
		if (GenericDatabase.TryGetIdListIdsForEntity(Id, out var foundDefinitions))
		{
			IdListIds = foundDefinitions;
		}
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho công trình và các module liên quan.
	/// </summary>
	public override void Deserialize(XContainer buildingDefinitionContainer)
	{
		XElement xElement = buildingDefinitionContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("Construction");
		if (xElement2 == null)
		{
			CLoggerManager.Log("The Construction element is missing in " + Id + ".", LogType.Error, CLogLevel.MAJOR);
			return;
		}
		ConstructionModuleDefinition = new ConstructionModuleDefinition(this, xElement2);
		XElement xElement3 = xElement.Element("Blueprint");
		if (xElement3 == null)
		{
			CLoggerManager.Log("The Blueprint element is missing in " + Id + ".", LogType.Error, CLogLevel.MAJOR);
			return;
		}
		BlueprintModuleDefinition = new BlueprintModuleDefinition(this, xElement3);
		XElement xElement4 = xElement.Element("Damageable");
		if (xElement4 != null)
		{
			DamageableModuleDefinition = new DamageableModuleDefinition(this, xElement4);
		}
		XElement xElement5 = xElement.Element("Brazier");
		if (xElement5 != null)
		{
			BrazierModuleDefinition = new BrazierModuleDefinition(this, xElement5);
		}
		XElement xElement6 = xElement.Element("Upgrade");
		if (xElement6 != null)
		{
			UpgradeModuleDefinition = new UpgradeModuleDefinition(this, xElement6);
		}
		XElement xElement7 = xElement.Element("Passives");
		if (xElement7 != null)
		{
			PassivesModuleDefinition = new PassivesModuleDefinition(this, xElement7);
		}
		XElement xElement8 = xElement.Element("Battle");
		if (xElement8 != null)
		{
			BattleModuleDefinition = new BattleModuleDefinition(this, xElement8);
		}
		XElement xElement9 = xElement.Element("Production");
		if (xElement9 != null)
		{
			ProductionModuleDefinition = new ProductionModuleDefinition(this, xElement9);
		}
	}

	#endregion
}

