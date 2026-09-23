using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa cho Vòng Tròn Phép Thuật (Magic Circle) - mục tiêu bảo vệ cốt lõi trong cốt truyện và lối chơi của The Last Spell.
/// Quản lý số lượng Pháp sư (Mages), ô chứa pháp sư (Mage Slots), và các Ấn phong ấn (Seals).
/// Kế thừa từ BuildingDefinition.
/// </summary>
public class MagicCircleDefinition : BuildingDefinition
{
	#region Properties

	/// <summary>
	/// Số lượng Pháp sư ban đầu làm lễ trong Vòng Tròn Phép.
	/// </summary>
	public int MageCountInit { get; set; }

	/// <summary>
	/// Số vị trí (slot) Pháp sư ban đầu khả dụng.
	/// </summary>
	public int MageSlotInit { get; set; }

	/// <summary>
	/// Số vị trí (slot) Pháp sư tối đa.
	/// </summary>
	public int MageSlotMax { get; set; }

	/// <summary>
	/// Số lượng Ấn phong ấn đang mở ban đầu.
	/// </summary>
	public int OpenSealsInit { get; set; }

	/// <summary>
	/// Số lượng Ấn phong ấn cần hoàn thành để kết thúc màn chơi/thắng trận.
	/// </summary>
	public int SealsToClose { get; set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa Vòng Tròn Phép từ dữ liệu XML.
	/// </summary>
	public MagicCircleDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho các thuộc tính của Vòng Tròn Phép (Magic Circle Settings).
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("MagicCircleSettings");
		if (xElement == null)
		{
			return;
		}
		XElement xElement2 = xElement.Element("MageSlotMax");
		if (xElement2.IsNullOrEmpty())
		{
			Debug.Log("ConstructionDefinition must have a MageSlotMax");
			return;
		}
		if (!int.TryParse(xElement2.Value, out var result))
		{
			Debug.Log("MagicCircle MageSlotMax must be a valid int");
			return;
		}
		MageSlotMax = result;
		XElement xElement3 = xElement.Element("MageSlotInit");
		if (xElement3.IsNullOrEmpty())
		{
			Debug.Log("ConstructionDefinition must have a MageSlotInit");
			return;
		}
		if (!int.TryParse(xElement3.Value, out var result2))
		{
			Debug.Log("MagicCircle MageSlotInit must be a valid int");
			return;
		}
		MageSlotInit = result2;
		XElement xElement4 = xElement.Element("MageCountInit");
		if (xElement4.IsNullOrEmpty())
		{
			Debug.Log("MagicCircle must have a MageCountInit");
			return;
		}
		if (!int.TryParse(xElement4.Value, out var result3))
		{
			Debug.Log("MagicCircle MageCountInit must be a valid int");
			return;
		}
		MageCountInit = result3;
		XElement xElement5 = xElement.Element("OpenSealsInit");
		if (xElement5.IsNullOrEmpty())
		{
			Debug.Log("MagicCircle must have a OpenSealsInit");
			return;
		}
		if (!int.TryParse(xElement5.Value, out var result4))
		{
			Debug.Log("MagicCircle OpenSealsInit must be a valid int");
			return;
		}
		OpenSealsInit = result4;
		XElement xElement6 = xElement.Element("SealsToClose");
		int result5;
		if (xElement6.IsNullOrEmpty())
		{
			Debug.Log("ConstructionDefinition must have a SealsToClose");
		}
		else if (!int.TryParse(xElement6.Value, out result5))
		{
			Debug.Log("MagicCircle SealsToClose must be a valid int");
		}
		else
		{
			SealsToClose = result5;
		}
	}

	#endregion
}

