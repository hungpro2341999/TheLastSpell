using System.Collections.Generic;
using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Item;

/// <summary>
/// Model đại diện cho một Affix (tiền tố/hậu tố BONUS) trên vật phẩm.
/// Implement IAffix để ItemController có thể merge chung với AffixMalus.
/// 
/// Mỗi Affix có:
/// - AffixDefinition: định nghĩa loại stat và giá trị modifier theo level.
/// - Level: cấp độ affix (level cao → modifier mạnh hơn).
/// - IsEpic: cờ đánh dấu affix chất lượng Epic (có thêm EpicStatModifiers).
/// 
/// Ví dụ: Affix "PhysicalDamage" Level 3 → +8 Physical Damage.
///         Nếu IsEpic → +8 Physical Damage + thêm EpicStatModifiers từ definition.
/// </summary>
public class Affix : IAffix
{
	#region Properties

	/// <summary>Controller quản lý Affix này.</summary>
	public AffixController AffixController { get; private set; }

	/// <summary>
	/// Định nghĩa (Definition) của Affix - chứa danh sách LevelDefinitions
	/// với stat modifiers cho mỗi level, và EpicStatModifiers bổ sung.
	/// </summary>
	public AffixDefinition AffixDefinition { get; private set; }

	/// <summary>
	/// Stat modifiers bổ sung cho phiên bản Epic của Affix.
	/// Trả về null nếu IsEpic = false. Lấy từ AffixDefinition.EpicStatModifiers.
	/// </summary>
	public Dictionary<UnitStatDefinition.E_Stat, float> EpicStatModifiers
	{
		get
		{
			if (!IsEpic)
			{
				return null;
			}
			return AffixDefinition.EpicStatModifiers;
		}
	}

	/// <summary>
	/// True nếu Affix này là phiên bản Epic (có thêm bonus đặc biệt).
	/// Epic Affix = StatModifiers thường + EpicStatModifiers bổ sung.
	/// </summary>
	public bool IsEpic { get; set; }

	/// <summary>
	/// Cấp độ hiện tại của Affix (mặc định = 1).
	/// Level cao hơn → giá trị stat modifier lớn hơn.
	/// </summary>
	public int Level { get; set; } = 1;

	/// <summary>
	/// Stat modifiers cơ bản theo level hiện tại.
	/// Lấy từ AffixDefinition.LevelDefinitions[Level].StatModifiers.
	/// </summary>
	public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers => AffixDefinition.LevelDefinitions[Level].StatModifiers;

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor khởi tạo Affix từ dữ liệu save (deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu serialized chứa Id, IsEpic, Level.</param>
	/// <param name="affixController">Controller quản lý Affix này.</param>
	public Affix(SerializedAffix container, AffixController affixController)
	{
		AffixController = affixController;
		Deserialize(container);
	}

	/// <summary>
	/// Constructor tạo Affix mới từ AffixDefinition (khi sinh vật phẩm mới).
	/// </summary>
	/// <param name="affixDefinition">Định nghĩa Affix.</param>
	/// <param name="affixController">Controller quản lý Affix này.</param>
	public Affix(AffixDefinition affixDefinition, AffixController affixController)
	{
		AffixController = affixController;
		AffixDefinition = affixDefinition;
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize từ save data - đọc Id, IsEpic, Level.
	/// Tra cứu AffixDefinition từ ItemDatabase theo Id.
	/// </summary>
	/// <param name="container">Dữ liệu serialized.</param>
	public void Deserialize(ISerializedData container = null)
	{
		SerializedAffix serializedAffix = container as SerializedAffix;
		AffixDefinition = ItemDatabase.AffixDefinitions[serializedAffix.Id];
		IsEpic = serializedAffix.IsEpic;
		Level = serializedAffix.Level;
	}

	/// <summary>
	/// Lấy stat modifiers cuối cùng (IAffix interface).
	/// - Nếu KHÔNG phải Epic: trả về StatModifiers thường.
	/// - Nếu là Epic: clone StatModifiers + cộng thêm EpicStatModifiers.
	/// </summary>
	/// <returns>Dictionary: E_Stat → tổng giá trị modifier.</returns>
	public Dictionary<UnitStatDefinition.E_Stat, float> GetFinalStatModifiers()
	{
		if (!IsEpic)
		{
			return StatModifiers;
		}
		// Clone base modifiers và cộng thêm epic modifiers
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = new Dictionary<UnitStatDefinition.E_Stat, float>(StatModifiers);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> epicStatModifier in EpicStatModifiers)
		{
			if (dictionary.ContainsKey(epicStatModifier.Key))
			{
				dictionary[epicStatModifier.Key] += epicStatModifier.Value;
			}
		}
		return dictionary;
	}

	/// <summary>
	/// Serialize Affix thành dữ liệu lưu game.
	/// Lưu: Id, IsEpic, Level.
	/// </summary>
	/// <returns>SerializedAffix chứa dữ liệu cần lưu.</returns>
	public ISerializedData Serialize()
	{
		return new SerializedAffix
		{
			Id = AffixDefinition.Id,
			IsEpic = IsEpic,
			Level = Level
		};
	}

	#endregion Public Methods
}
