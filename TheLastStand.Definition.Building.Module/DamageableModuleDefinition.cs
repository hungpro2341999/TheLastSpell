using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class DamageableModuleDefinition : BuildingModuleDefinition
{
	#region Properties
	/// <summary>
	/// ID của hiệu ứng hạt (Particle FX) khi công trình bị hư hại.
	/// </summary>
	public string DamagedParticlesId { get; private set; } = string.Empty;

	/// <summary>
	/// Cho biết công trình có gây tăng chỉ số Panic (hoảng loạn) khi bị tấn công hay không.
	/// </summary>
	public bool CanPanic => TotalPanicValue > 0f;

	/// <summary>
	/// Tổng điểm Panic tạo ra khi công trình bị tấn công mất máu.
	/// </summary>
	public float TotalPanicValue { get; private set; }

	/// <summary>
	/// Tắt hiệu ứng khói (Smoke FX) khi công trình bị phá hủy.
	/// </summary>
	public bool DisableDestructionSmokeFX { get; private set; }

	/// <summary>
	/// Số lượng ngọn lửa hiển thị khi công trình bị cháy/hư hại.
	/// </summary>
	public byte FlameCount { get; private set; }

	/// <summary>
	/// Danh sách các vị trí neo (Vector2 anchor points) để hiển thị ngọn lửa trên công trình.
	/// </summary>
	public List<Vector2> FlamesPositions { get; private set; } = new List<Vector2>();

	/// <summary>
	/// Hệ số phần trăm chỉnh sửa lượng máu từ các Glyph được trang bị.
	/// </summary>
	public int GlyphHealthTotalPercentageModifier
	{
		get
		{
			int num = 0;
			if (BuildingDefinition.IdListIds != null)
			{
				foreach (string idListId in BuildingDefinition.IdListIds)
				{
					num += TPSingleton<GlyphManager>.Instance.BuildingHealthModifiers.GetValueOrDefault(idListId);
				}
			}
			return num;
		}
	}

	/// <summary>
	/// Tổng số máu tối đa của công trình sau khi tính toán các bonus modifier.
	/// </summary>
	public float HealthTotal => BuildingManager.ComputeBuildingTotalHealth(this);

	/// <summary>
	/// Giữ hiển thị HUD thanh máu ngay cả khi công trình bị thương.
	/// </summary>
	public bool KeepHUDDisplayedWhenDamaged { get; private set; }

	/// <summary>
	/// Lượng máu tối đa nguyên bản của công trình từ định nghĩa XML.
	/// </summary>
	public float NativeHealthTotal { get; private set; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo định nghĩa module nhận sát thương của công trình.
	/// </summary>
	public DamageableModuleDefinition(BuildingDefinition buildingDefinition, XContainer damageableDefinition)
		: base(buildingDefinition, damageableDefinition)
	{
	}
	#endregion

	#region Deserialization
	/// <summary>
	/// Đọc thông số máu ban đầu, Panic value, hiệu ứng lửa cháy và các thuộc tính liên quan từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("HealthTotal");
		if (xElement2 != null)
		{
			if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Building " + BuildingDefinition.Id + "'s Health " + HasAnInvalidFloat(xElement2.Value), LogType.Error);
				return;
			}
			NativeHealthTotal = result;
		}
		KeepHUDDisplayedWhenDamaged = xElement.Element("KeepHUDDisplayedWhenDamaged") != null;
		XElement xElement3 = xElement.Element("FlameCount");
		if (xElement3 != null)
		{
			if (byte.TryParse(xElement3.Value, out var result2))
			{
				FlameCount = result2;
			}
			else
			{
				CLoggerManager.Log("Building " + BuildingDefinition.Id + "'s FlameCount " + HasAnInvalid("byte", xElement3.Value), LogType.Warning);
			}
		}
		XElement xElement4 = xElement.Element("FlameAnchorPoints");
		if (xElement4 != null)
		{
			foreach (XElement item2 in xElement4.DescendantNodes())
			{
				try
				{
					Vector2 item = new Vector2(float.Parse(item2.Attribute("PositionX").Value, NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(item2.Attribute("PositionY").Value, NumberStyles.Float, CultureInfo.InvariantCulture));
					FlamesPositions.Add(item);
				}
				catch (FormatException)
				{
					CLoggerManager.Log("While deserializing building " + BuildingDefinition.Id + ", I found an invalid flame position!", LogType.Warning);
				}
			}
			if (FlameCount > FlamesPositions.Count)
			{
				CLoggerManager.Log("Error while deserializing Building " + BuildingDefinition.Id + ": You cannot add more flames to the building than there are flamed positions.", LogType.Error);
			}
		}
		DisableDestructionSmokeFX = xElement.Element("NoSmokeFXOnDestruction") != null;
		if (xElement.Element("DamagedParticles") != null)
		{
			XAttribute xAttribute = xElement.Element("DamagedParticles").Attribute("Id");
			DamagedParticlesId = xAttribute.Value;
		}
		XElement xElement6 = xElement.Element("TotalPanicValue");
		if (!string.IsNullOrEmpty(xElement6?.Value))
		{
			if (float.TryParse(xElement6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				TotalPanicValue = result3;
			}
			else
			{
				CLoggerManager.Log("Error while deserializing Building " + BuildingDefinition.Id + ": Unable to parse totalPanicValueElement.Value into float", LogType.Warning);
			}
		}
	}
	#endregion
}
