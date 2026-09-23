using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Controller quản lý một AffixMalus (tiền tố/hậu tố PHẠT) trên vật phẩm.
/// AffixMalus ngược với Affix - thay vì cộng bonus, nó TRỪ chỉ số của vật phẩm.
/// Ví dụ: "-3 Dodge", "-5% Resistance". AffixMalus có MalusLevel xác định mức độ phạt,
/// tạo trade-off cho vật phẩm có Affix mạnh (vật phẩm tốt hơn → malus nặng hơn).
/// </summary>
public class AffixMalusController
{
	#region Properties

	/// <summary>
	/// Model AffixMalus mà controller này quản lý.
	/// Chứa dữ liệu runtime: AffixMalusDefinition, MalusLevel, giá trị stat modifier.
	/// </summary>
	public AffixMalus AffixMalus { get; private set; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo AffixMalus từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized của AffixMalus từ file save.</param>
	public AffixMalusController(SerializedAffixMalus container)
	{
		AffixMalus = new AffixMalus(container, this);
	}

	/// <summary>
	/// Constructor khởi tạo AffixMalus mới từ definition (khi sinh vật phẩm mới).
	/// </summary>
	/// <param name="affixMalusDefinition">Định nghĩa AffixMalus chứa loại stat và range phạt.</param>
	public AffixMalusController(AffixMalusDefinition affixMalusDefinition)
	{
		AffixMalus = new AffixMalus(affixMalusDefinition, this);
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Đặt mức độ phạt (MalusLevel) cho AffixMalus.
	/// MalusLevel cao hơn → giá trị phạt lớn hơn. Được gọi khi tạo vật phẩm
	/// để cân bằng với số lượng/chất lượng Affix bonus.
	/// </summary>
	/// <param name="malusLevel">Mức độ phạt cần đặt.</param>
	public void SetLevel(AffixMalusDefinition.E_MalusLevel malusLevel)
	{
		AffixMalus.MalusLevel = malusLevel;
	}

	#endregion Public Methods
}

