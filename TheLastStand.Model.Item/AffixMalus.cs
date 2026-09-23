using System.Collections.Generic;
using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho một AffixMalus (tiền tố/hậu tố PHẠT) trên vật phẩm.
/// Implement IAffix để ItemController có thể merge chung với Affix bonus.
/// 
/// AffixMalus TRỪ chỉ số thay vì cộng, tạo trade-off:
/// - Vật phẩm có nhiều/mạnh Affix bonus → sẽ có AffixMalus nặng hơn.
/// - MalusLevel xác định mức độ phạt (cao hơn → phạt nặng hơn).
/// 
/// Ví dụ: AffixMalus stat=Dodge, MalusLevel=Medium → -5 Dodge.
/// </summary>
public class AffixMalus : IAffix
{
	#region Properties

	/// <summary>Controller quản lý AffixMalus này.</summary>
	public AffixMalusController AffixMalusController { get; private set; }

	/// <summary>
	/// Định nghĩa (Definition) của AffixMalus - chứa loại Stat bị phạt
	/// và bảng MalusPerLevel (giá trị phạt theo mỗi MalusLevel).
	/// </summary>
	public AffixMalusDefinition AffixMalusDefinition { get; private set; }

	/// <summary>
	/// Mức độ phạt hiện tại (None, Low, Medium, High...).
	/// Quyết định giá trị phạt cụ thể qua AffixMalusDefinition.MalusPerLevel[MalusLevel].
	/// </summary>
	public AffixMalusDefinition.E_MalusLevel MalusLevel { get; set; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo AffixMalus từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized chứa MalusLevel và Stat.</param>
	/// <param name="affixMalusController">Controller quản lý AffixMalus này.</param>
	public AffixMalus(SerializedAffixMalus container, AffixMalusController affixMalusController)
	{
		AffixMalusController = affixMalusController;
		Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo AffixMalus mới từ definition (khi sinh vật phẩm mới).
	/// </summary>
	/// <param name="affixMalusDefinition">Định nghĩa AffixMalus.</param>
	/// <param name="affixMalusController">Controller quản lý AffixMalus này.</param>
	public AffixMalus(AffixMalusDefinition affixMalusDefinition, AffixMalusController affixMalusController)
	{
		AffixMalusDefinition = affixMalusDefinition;
		AffixMalusController = affixMalusController;
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize từ save data - đọc MalusLevel và Stat.
	/// Tra cứu AffixMalusDefinition từ ItemDatabase theo Stat type.
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	public void Deserialize(ISerializedData container = null)
	{
		SerializedAffixMalus serializedAffixMalus = container as SerializedAffixMalus;
		MalusLevel = serializedAffixMalus.MalusLevel;
		AffixMalusDefinition = ItemDatabase.AffixMalusDefinitions[serializedAffixMalus.Stat];
	}

	/// <summary>
	/// Lấy stat modifiers cuối cùng (IAffix interface).
	/// Trả về Dictionary chứa 1 entry: Stat → giá trị phạt theo MalusLevel.
	/// Ví dụ: { Dodge: 5.0 } (giá trị dương, sẽ bị TRỪ khi merge trong ItemController).
	/// </summary>
	/// <returns>Dictionary: E_Stat → giá trị phạt.</returns>
	public Dictionary<UnitStatDefinition.E_Stat, float> GetFinalStatModifiers()
	{
		return new Dictionary<UnitStatDefinition.E_Stat, float> { 
		{
			AffixMalusDefinition.Stat,
			AffixMalusDefinition.MalusPerLevel[MalusLevel]
		} };
	}

	/// <summary>
	/// Serialize AffixMalus thành dữ liệu lưu game.
	/// Lưu: MalusLevel, Stat type.
	/// </summary>
	/// <returns>SerializedAffixMalus chứa dữ liệu cần lưu.</returns>
	public ISerializedData Serialize()
	{
		return new SerializedAffixMalus
		{
			MalusLevel = MalusLevel,
			Stat = AffixMalusDefinition.Stat
		};
	}

	#endregion Public Methods
}
