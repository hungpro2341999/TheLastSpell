using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Serialization;

namespace TheLastStand.Controller.Item;

/// <summary>
/// Controller quản lý một Affix (tiền tố/hậu tố bonus) trên vật phẩm.
/// Affix là các chỉ số bonus (stat modifier) được gắn thêm vào vật phẩm khi tạo ra,
/// ví dụ: "+5 Physical Damage", "+10% Critical". Mỗi Affix có AffixDefinition
/// xác định loại stat và giá trị cộng thêm.
/// Controller này chỉ đảm nhiệm việc khởi tạo Model Affix từ definition hoặc save data.
/// </summary>
public class AffixController
{
	#region Properties

	/// <summary>
	/// Model Affix mà controller này quản lý.
	/// Chứa dữ liệu runtime: AffixDefinition, giá trị stat modifier thực tế.
	/// </summary>
	public Affix Affix { get; private set; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo Affix từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized của Affix từ file save.</param>
	public AffixController(SerializedAffix container)
	{
		Affix = new Affix(container, this);
	}

	/// <summary>
	/// Constructor khởi tạo Affix mới từ AffixDefinition (khi sinh vật phẩm mới).
	/// </summary>
	/// <param name="affixDefinition">Định nghĩa Affix chứa loại stat và range giá trị.</param>
	public AffixController(AffixDefinition affixDefinition)
	{
		Affix = new Affix(affixDefinition, this);
	}

	#endregion Constructors
}

