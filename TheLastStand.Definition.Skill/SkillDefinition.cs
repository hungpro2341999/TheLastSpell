using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.CastFx;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Định nghĩa toàn bộ thông số của một kỹ năng (Skill) trong game.
/// Đây là lớp trung tâm của hệ thống kỹ năng, chứa mọi thông tin cần thiết:
/// - Chi phí sử dụng (Action Points, Mana, Health, Move Points)
/// - Phạm vi tấn công (Range) và vùng ảnh hưởng (Area of Effect)
/// - Mục tiêu hợp lệ (ValidTargets) - ai/cái gì có thể bị nhắm
/// - Hành động kỹ năng (SkillAction) - Attack, Generic, Spawn, Build...
/// - Điều kiện ngữ cảnh (ContextualConditions) - khi nào kỹ năng khả dụng
/// - Pha cho phép (AllowDuringPhase) - kỹ năng dùng được trong pha nào
/// - Hiệu ứng hình ảnh/âm thanh (CastFX, SoundId, ArtId)
/// 
/// Hỗ trợ kế thừa qua Template: một skill có thể tham chiếu TemplateId 
/// để kế thừa toàn bộ thuộc tính từ skill khác, sau đó override từng thuộc tính cụ thể.
/// 
/// Dữ liệu được deserialize từ file XML trong thư mục TextAsset.
/// </summary>
public class SkillDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Nested Types - Enums

	/// <summary>
	/// Enum xác định pha nào trong lượt chơi mà kỹ năng được phép sử dụng.
	/// Sử dụng bit flags để có thể kết hợp nhiều pha (ví dụ: Day = Production | Deployment = 3).
	/// </summary>
	public enum E_Phase
	{
		/// <summary>Không thuộc pha nào.</summary>
		None = 0,
		/// <summary>Pha Sản xuất (ban ngày) - xây dựng, nâng cấp, sản xuất tài nguyên.</summary>
		Production = 1,
		/// <summary>Pha Triển khai - bố trí tướng trước khi chiến đấu.</summary>
		Deployment = 2,
		/// <summary>Pha Đêm - pha chiến đấu chống lại quái vật.</summary>
		Night = 4,
		/// <summary>Pha Ban ngày = Production | Deployment (cả sản xuất lẫn triển khai).</summary>
		Day = 3,
		/// <summary>Tất cả các pha = Production | Deployment | Night.</summary>
		All = 7
	}

	/// <summary>
	/// Enum xác định cách hiển thị kỹ năng khi không thể sử dụng (invalid cast).
	/// </summary>
	public enum E_InvalidCastDisplayBehaviour
	{
		/// <summary>Không xử lý đặc biệt.</summary>
		None,
		/// <summary>Ẩn hoàn toàn kỹ năng khỏi UI khi không thể sử dụng.</summary>
		Hidden,
		/// <summary>Hiển thị kỹ năng nhưng ở trạng thái không khả dụng (greyed out).</summary>
		DisplayedUnavailable
	}

	#endregion Nested Types - Enums

	#region Nested Types - Constants

	/// <summary>
	/// Các hằng số toàn cục liên quan đến hệ thống kỹ năng.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// Các ID kỹ năng đặc biệt được hardcode.
		/// </summary>
		public static class Ids
		{
			/// <summary>ID kỹ năng "Bỏ lượt" cho tướng thường.</summary>
			public const string SkipTurn = "SkipTurn";

			/// <summary>ID kỹ năng "Bỏ lượt" cho Gargoyle (quái đặc biệt).</summary>
			public const string GargoyleSkipTurn = "GargoyleSkipTurn2";
		}

		/// <summary>Ký tự đại diện cho ô chịu hiệu ứng chính trong pattern AoE.</summary>
		public const char AreaOfEffectSymbol = 'X';

		/// <summary>Ký tự đại diện cho ô di chuyển (Maneuver) trong pattern AoE.</summary>
		public const char ManeuverEffectSymbol = 'M';

		/// <summary>Ký tự đại diện cho ô hiệu ứng bao quanh (Surrounding Effect) trong pattern AoE.</summary>
		public const char SurroundingEffectSymbol = 'e';

		/// <summary>Ký tự đại diện cho ô trống (không có hiệu ứng) trong pattern AoE.</summary>
		public const char EmptyEffectSymbol = '_';
	}

	#endregion Nested Types - Constants

	#region Fields

	/// <summary>
	/// Xác định loại đơn vị nào bị ảnh hưởng bởi kỹ năng này.
	/// Mặc định = All (tất cả đơn vị). Có thể giới hạn chỉ ảnh hưởng đồng minh hoặc kẻ địch.
	/// </summary>
	public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnits = AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.All;

	#endregion Fields

	#region Properties - Chi phí sử dụng (Cost)

	/// <summary>
	/// Số điểm hành động (Action Points) cần tiêu tốn để sử dụng kỹ năng.
	/// Mỗi tướng có số AP giới hạn mỗi lượt.
	/// </summary>
	public int ActionPointsCost { get; private set; }

	/// <summary>
	/// Số máu (Health) phải trả để sử dụng kỹ năng (chi phí máu).
	/// Dùng cho các kỹ năng mạnh nhưng đòi hỏi hy sinh HP.
	/// </summary>
	public int HealthCost { get; private set; }

	/// <summary>
	/// Số mana cần tiêu tốn để sử dụng kỹ năng.
	/// Mana là tài nguyên chung của đội, có thể hồi phục qua các nguồn khác nhau.
	/// </summary>
	public int ManaCost { get; private set; }

	/// <summary>
	/// Số điểm di chuyển (Move Points) cần tiêu tốn.
	/// Thường dùng cho kỹ năng di chuyển đặc biệt.
	/// </summary>
	public int MovePointsCost { get; private set; }

	#endregion Properties - Chi phí sử dụng (Cost)

	#region Properties - Phạm vi và vùng ảnh hưởng (Range & AoE)

	/// <summary>
	/// Số ô bị ảnh hưởng bởi hiệu ứng chính (đếm ký tự 'X' trong pattern AoE).
	/// </summary>
	public int AffectedTilesCount { get; private set; }

	/// <summary>
	/// Định nghĩa vùng ảnh hưởng (Area of Effect) - hình dạng và tâm điểm.
	/// Nếu không được chỉ định, mặc định là single target (1 ô 'X').
	/// </summary>
	public AreaOfEffectDefinition AreaOfEffectDefinition { get; private set; }

	/// <summary>
	/// True nếu kỹ năng có thể xoay vùng AoE (rotate pattern).
	/// Cho phép người chơi xoay hình dạng AoE theo 4 hướng.
	/// </summary>
	public bool CanRotate { get; private set; }

	/// <summary>
	/// True nếu kỹ năng có thể lật (flip/mirror) vùng AoE.
	/// </summary>
	public bool CanFlip { get; private set; }

	/// <summary>
	/// True nếu khóa tự động xoay AoE theo hướng mục tiêu.
	/// Không thể kết hợp với CanRotate = true (sẽ log lỗi).
	/// </summary>
	public bool LockAutoOrientation { get; private set; }

	/// <summary>
	/// True nếu kỹ năng chỉ có thể nhắm theo 4 hướng chính (Cardinal: lên/xuống/trái/phải).
	/// Không cho phép nhắm theo đường chéo.
	/// </summary>
	public bool CardinalDirectionOnly { get; private set; }

	/// <summary>
	/// True nếu kỹ năng có phạm vi vô hạn (có thể nhắm bất kỳ ô nào trên bản đồ).
	/// </summary>
	public bool InfiniteRange { get; private set; }

	/// <summary>
	/// True nếu phạm vi kỹ năng có thể bị thay đổi bởi các buff/perk.
	/// </summary>
	public bool RangeModifiable { get; private set; }

	/// <summary>
	/// Phạm vi tấn công: x = khoảng cách tối thiểu, y = khoảng cách tối đa (tính bằng ô).
	/// Ví dụ: (1, 3) nghĩa là nhắm được từ 1-3 ô cách đơn vị.
	/// </summary>
	public Vector2Int Range { get; private set; }

	/// <summary>
	/// Số ô chịu hiệu ứng bao quanh (Surrounding Effect) - đếm ký tự 'e' trong pattern AoE.
	/// Surrounding Effect thường là hiệu ứng phụ nhẹ hơn xung quanh vùng chính.
	/// </summary>
	public int SurroundingEffectTilesCount { get; private set; }

	/// <summary>
	/// Tổng số ô bị ảnh hưởng = AffectedTilesCount + SurroundingEffectTilesCount.
	/// </summary>
	public int TotalAreaOfEffectTilesCount => AffectedTilesCount + SurroundingEffectTilesCount;

	#endregion Properties - Phạm vi và vùng ảnh hưởng (Range & AoE)

	#region Properties - Định danh và hiển thị (Identity & Display)

	/// <summary>
	/// ID duy nhất của kỹ năng, đọc từ attribute "Id" trong XML.
	/// Dùng làm key trong SkillDatabase.SkillDefinitions.
	/// </summary>
	public string Id { get; private set; }

	/// <summary>
	/// ID nhóm kỹ năng - các kỹ năng cùng GroupId được coi là biến thể của nhau.
	/// Mặc định = Id nếu không được chỉ định.
	/// </summary>
	public string GroupId { get; private set; }

	/// <summary>
	/// ID dùng cho localization (tên, mô tả kỹ năng theo ngôn ngữ).
	/// Có thể override bằng element OverrideLocalizationId, mặc định = Id.
	/// </summary>
	public string LocalizationId { get; private set; }

	/// <summary>
	/// ID của sprite/hình ảnh đại diện kỹ năng trong UI.
	/// Có thể override bằng element OverrideArtId, mặc định = Id.
	/// </summary>
	public string ArtId { get; private set; }

	/// <summary>
	/// ID âm thanh phát khi sử dụng kỹ năng.
	/// Có thể override bằng element OverrideSoundId, mặc định = GroupId.
	/// </summary>
	public string SoundId { get; private set; }

	/// <summary>
	/// Cấp độ (level) của kỹ năng. Dùng trong hệ thống skill leveling.
	/// </summary>
	public int Level { get; private set; }

	#endregion Properties - Định danh và hiển thị (Identity & Display)

	#region Properties - Pha và điều kiện (Phase & Conditions)

	/// <summary>
	/// Bit flags xác định kỹ năng được PHÉP sử dụng trong pha nào.
	/// Mặc định = Night nếu không chỉ định.
	/// </summary>
	public E_Phase AllowDuringPhase { get; private set; }

	/// <summary>
	/// Bit flags xác định kỹ năng được HIỂN THỊ trong UI ở pha nào.
	/// Mặc định = All (hiển thị ở mọi pha). Khác với AllowDuringPhase (cho phép sử dụng).
	/// </summary>
	public E_Phase DisplayDuringPhase { get; private set; }

	/// <summary>
	/// True nếu kỹ năng cho phép bắn trúng đồng minh (friendly fire).
	/// </summary>
	public bool AllowFriendlyFire { get; private set; }

	/// <summary>
	/// Danh sách các điều kiện ngữ cảnh (contextual conditions) phải thỏa mãn để kỹ năng khả dụng.
	/// Ví dụ: phải đứng trong Watchtower, phải ở gần công trình, chỉ trong pha đêm...
	/// </summary>
	public List<SkillConditionDefinition> ContextualConditions { get; private set; } = new List<SkillConditionDefinition>();

	/// <summary>
	/// True nếu đây là kỹ năng theo ngữ cảnh (contextual skill).
	/// Kỹ năng contextual chỉ xuất hiện khi đáp ứng điều kiện cụ thể.
	/// </summary>
	public bool IsContextual { get; private set; }

	/// <summary>
	/// True nếu kỹ năng bị khóa và cần Perk để mở khóa.
	/// </summary>
	public bool IsLockedByPerk { get; private set; }

	/// <summary>
	/// True nếu kỹ năng chỉ dành riêng cho Brazier (Bàn thờ lửa - công trình đặc biệt).
	/// </summary>
	public bool IsBrazierSpecific { get; private set; }

	/// <summary>
	/// Cách hiển thị kỹ năng khi không thể sử dụng (invalid cast).
	/// Mặc định = Hidden (ẩn hoàn toàn).
	/// </summary>
	public E_InvalidCastDisplayBehaviour InvalidCastDisplayBehaviour { get; private set; }

	#endregion Properties - Pha và điều kiện (Phase & Conditions)

	#region Properties - Hành động và mục tiêu (Action & Targeting)

	/// <summary>
	/// Định nghĩa hành động chính của kỹ năng (Attack, Generic, Spawn, Build, Resupply...).
	/// Chứa danh sách các SkillEffectDefinition mà kỹ năng gây ra.
	/// </summary>
	public SkillActionDefinition SkillActionDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa các mục tiêu hợp lệ mà kỹ năng có thể nhắm tới.
	/// Bao gồm: công trình, đơn vị đồng minh/kẻ địch, ô trống...
	/// </summary>
	public ValidTargets ValidTargets { get; private set; }

	/// <summary>
	/// Số lần tối đa kỹ năng có thể được sử dụng mỗi lượt.
	/// -1 = không giới hạn (mặc định).
	/// </summary>
	public int UsesPerTurnCount { get; private set; } = -1;

	#endregion Properties - Hành động và mục tiêu (Action & Targeting)

	#region Properties - Hiệu ứng hình ảnh (Visual FX)

	/// <summary>
	/// Định nghĩa hiệu ứng hình ảnh phát TRƯỚC khi kỹ năng được thi hành (pre-cast VFX).
	/// </summary>
	public SkillCastFxDefinition PreSkillCastFxDefinition { get; private set; }

	/// <summary>
	/// Định nghĩa hiệu ứng hình ảnh chính khi kỹ năng được thi hành (cast VFX).
	/// </summary>
	public SkillCastFxDefinition SkillCastFxDefinition { get; private set; }

	#endregion Properties - Hiệu ứng hình ảnh (Visual FX)

	#region Constructors

	/// <summary>
	/// Constructor - khởi tạo SkillDefinition từ dữ liệu XML.
	/// Gọi base constructor để tự động kích hoạt Deserialize().
	/// </summary>
	/// <param name="container">XML container chứa toàn bộ dữ liệu cấu hình kỹ năng.</param>
	public SkillDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Kiểm tra xem kỹ năng này có thể ảnh hưởng đến một loại đơn vị cụ thể hay không.
	/// Xử lý riêng biệt cho Surrounding Effect (hiệu ứng bao quanh) và hiệu ứng chính.
	/// </summary>
	/// <param name="type">Loại đơn vị cần kiểm tra (Playable, Enemy, All...).</param>
	/// <param name="isSurroundingEffect">True nếu kiểm tra cho Surrounding Effect, False cho hiệu ứng chính.</param>
	/// <returns>True nếu kỹ năng có thể ảnh hưởng loại đơn vị đó.</returns>
	public bool CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect type, bool isSurroundingEffect)
	{
		// Nếu kiểm tra cho Surrounding Effect - chỉ xét các effect trong key "SurroundingEffect"
		if (isSurroundingEffect)
		{
			if (SkillActionDefinition.SkillEffectDefinitions != null && SkillActionDefinition.SkillEffectDefinitions.TryGetValue("SurroundingEffect", out var value))
			{
				foreach (SkillEffectDefinition item in value)
				{
					if (item is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(type))
					{
						return true;
					}
				}
			}
			return false;
		}
		// Nếu kiểm tra cho hiệu ứng chính - xét tất cả effect NGOẠI TRỪ "SurroundingEffect"
		if (SkillActionDefinition.SkillEffectDefinitions != null)
		{
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition in SkillActionDefinition.SkillEffectDefinitions)
			{
				if (skillEffectDefinition.Key == "SurroundingEffect")
				{
					continue;
				}
				foreach (SkillEffectDefinition item2 in skillEffectDefinition.Value)
				{
					if (item2 is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition2 && affectingUnitSkillEffectDefinition2.AffectedUnits.AffectsUnitType(type))
					{
						return true;
					}
				}
			}
		}
		// Fallback: kiểm tra field AffectedUnits cấp SkillDefinition
		return AffectedUnits.HasFlag(type);
	}

	#endregion Public Methods

	#region Deserialization

	/// <summary>
	/// Deserialize toàn bộ dữ liệu kỹ năng từ XML.
	/// Hỗ trợ hệ thống Template: nếu có TemplateId, kế thừa thuộc tính từ skill template,
	/// sau đó override bằng các giá trị được khai báo trực tiếp.
	/// 
	/// Thứ tự xử lý:
	/// 1. Đọc Id và TemplateId → tìm skill template
	/// 2. Đọc các cờ boolean (AllowFriendlyFire, IsContextual, IsLockedByPerk...)
	/// 3. Đọc tên/art/sound (LocalizationId, ArtId, SoundId) với fallback về template
	/// 4. Đọc chi phí (ActionPointsCost, ManaCost, HealthCost, MovePointsCost, UsesPerTurnCount)
	/// 5. Đọc phạm vi (Range, InfiniteRange, CardinalDirectionOnly)
	/// 6. Parse vùng AoE (AreaOfEffect pattern)
	/// 7. Parse SkillAction (Attack/Generic/Spawn/Build/Resupply...)
	/// 8. Validate Maneuver effect + maneuver tile
	/// 9. Parse ValidTargets (Buildings, Units, Tiles)
	/// 10. Parse AllowDuringPhases và DisplayDuringPhases
	/// 11. Parse ContextualConditions
	/// 12. Parse InvalidCastDisplayBehaviour
	/// 13. Parse CastFXs và PreCastFXs
	/// 14. Tạo AoE mặc định nếu chưa có (single target 'X')
	/// </summary>
	/// <param name="container">XML container (XElement) chứa toàn bộ cấu hình kỹ năng.</param>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		SkillDefinition skillDefinition = null;

		#region 1. Đọc Id và Template

		// Đọc ID bắt buộc - nếu không có sẽ log lỗi và return
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute == null)
		{
			CLoggerManager.Log("The skill has no ID !", LogType.Error);
			return;
		}
		Id = xAttribute.Value;

		// Kiểm tra TemplateId - nếu có, lấy skill template làm giá trị mặc định
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			if (SkillDatabase.SkillDefinitions.ContainsKey(xAttribute2.Value))
			{
				skillDefinition = SkillDatabase.SkillDefinitions[xAttribute2.Value];
			}
			else
			{
				CLoggerManager.Log("Error while parsing Template parameter of skill " + Id + "! The skill with ID " + xAttribute2.Value + " does'nt exist.", LogType.Error);
			}
		}

		#endregion 1. Đọc Id và Template

		#region 2. Đọc các cờ boolean và tên hiển thị

		// AllowFriendlyFire: có element = true, không có thì fallback về template
		AllowFriendlyFire = xElement.Element("AllowFriendlyFire") != null || (skillDefinition?.AllowFriendlyFire ?? false);

		// GroupId: attribute → template → mặc định = Id
		GroupId = xElement.Attribute("GroupId")?.Value ?? skillDefinition?.GroupId ?? Id;

		// IsContextual: attribute boolean
		XAttribute xAttribute3 = xElement.Attribute("IsContextual");
		IsContextual = xAttribute3 != null && bool.Parse(xAttribute3.Value);

		// IsLockedByPerk: attribute boolean
		XAttribute xAttribute4 = xElement.Attribute("IsLockedByPerk");
		IsLockedByPerk = xAttribute4 != null && bool.Parse(xAttribute4.Value);

		// IsBrazierSpecific: attribute boolean
		XAttribute xAttribute5 = xElement.Attribute("IsBrazierSpecific");
		IsBrazierSpecific = xAttribute5 != null && bool.Parse(xAttribute5.Value);

		// LocalizationId, ArtId, SoundId: override element → template → mặc định
		LocalizationId = xElement.Element("OverrideLocalizationId")?.Value ?? skillDefinition?.LocalizationId ?? Id;
		ArtId = xElement.Element("OverrideArtId")?.Value ?? skillDefinition?.ArtId ?? Id;
		SoundId = xElement.Element("OverrideSoundId")?.Value ?? skillDefinition?.SoundId ?? GroupId;

		#endregion 2. Đọc các cờ boolean và tên hiển thị

		#region 3. Đọc chi phí sử dụng (Costs)

		// ActionPointsCost
		XElement xElement2 = xElement.Element("ActionPointsCost");
		int result;
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing ActionPointsCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			ActionPointsCost = result;
		}
		else if (skillDefinition != null)
		{
			ActionPointsCost = skillDefinition.ActionPointsCost;
		}

		// MovePointsCost
		XElement xElement3 = xElement.Element("MovePointsCost");
		if (xElement3 != null)
		{
			if (!int.TryParse(xElement3.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing MovePointsCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			MovePointsCost = result;
		}
		else if (skillDefinition != null)
		{
			MovePointsCost = skillDefinition.MovePointsCost;
		}

		// ManaCost
		XElement xElement4 = xElement.Element("ManaCost");
		if (xElement4 != null)
		{
			if (!int.TryParse(xElement4.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing ManaCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			ManaCost = result;
		}
		else if (skillDefinition != null)
		{
			ManaCost = skillDefinition.ManaCost;
		}

		// HealthCost
		XElement xElement5 = xElement.Element("HealthCost");
		if (xElement5 != null)
		{
			if (!int.TryParse(xElement5.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing HealthCost parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			HealthCost = result;
		}
		else if (skillDefinition != null)
		{
			HealthCost = skillDefinition.HealthCost;
		}

		// UsesPerTurnCount
		XElement xElement6 = xElement.Element("UsesPerTurnCount");
		if (xElement6 != null)
		{
			if (!int.TryParse(xElement6.Value, out result))
			{
				CLoggerManager.Log($"Error while parsing UsesPerTurnCount parameter of level {Level} of skill {Id} !", LogType.Error);
				return;
			}
			UsesPerTurnCount = result;
		}
		else if (skillDefinition != null)
		{
			UsesPerTurnCount = skillDefinition.UsesPerTurnCount;
		}

		#endregion 3. Đọc chi phí sử dụng (Costs)

		#region 4. Đọc phạm vi tấn công (Range)

		XElement xElement7 = xElement.Element("Range");
		if (xElement7 != null)
		{
			// Parse Min/Max range từ attributes
			Range = new Vector2Int(int.Parse(xElement7.Attribute("Min").Value), int.Parse(xElement7.Attribute("Max").Value));

			// CardinalDirectionOnly: chỉ nhắm theo 4 hướng chính
			XAttribute xAttribute6 = xElement7.Attribute("CardinalDirectionOnly");
			if (xAttribute6 != null)
			{
				if (!bool.TryParse(xAttribute6.Value, out var result2))
				{
					CLoggerManager.Log($"The level {Level} of skill {Id} has an invalid CardinalDirectionOnly!", LogType.Error);
					return;
				}
				CardinalDirectionOnly = result2;
			}

			// Modifiable: phạm vi có thể bị buff/perk thay đổi
			XAttribute xAttribute7 = xElement7.Attribute("Modifiable");
			if (xAttribute7 != null)
			{
				if (!bool.TryParse(xAttribute7.Value, out var result3))
				{
					CLoggerManager.Log($"The level {Level} of skill {Id} has an invalid Modifiable!", LogType.Error);
					return;
				}
				RangeModifiable = result3;
			}
		}
		else if (skillDefinition != null)
		{
			// Kế thừa từ template
			Range = skillDefinition.Range;
			CardinalDirectionOnly = skillDefinition.CardinalDirectionOnly;
			RangeModifiable = skillDefinition.RangeModifiable;
		}

		// InfiniteRange: có element = true
		InfiniteRange = xElement.Element("InfiniteRange") != null || (skillDefinition?.InfiniteRange ?? false);

		#endregion 4. Đọc phạm vi tấn công (Range)

		#region 5. Parse vùng ảnh hưởng (Area of Effect)

		int num = 0; // Đếm số ô Maneuver ('M') trong pattern
		XElement xElement8 = xElement.Element("AreaOfEffect");
		if (xElement8 != null)
		{
			// Tạo AoE definition mới với origin từ attributes
			AreaOfEffectDefinition = new AreaOfEffectDefinition
			{
				Origin = new Vector2Int(int.Parse(xElement8.Attribute("OriginX").Value), int.Parse(xElement8.Attribute("OriginY").Value)),
				Pattern = new List<List<char>>(),
				IsSingleTarget = false
			};

			// Parse pattern text thành ma trận 2D ký tự
			// Pattern được đọc từ dưới lên trên (reverse) để khớp với hệ tọa độ game
			string[] array = xElement8.Value.Split('\n');
			for (int num2 = array.Length - 1; num2 >= 0; num2--)
			{
				array[num2] = array[num2].RemoveWhitespace();
				if (array[num2] != string.Empty)
				{
					AreaOfEffectDefinition.Pattern.Add(new List<char>(array[num2].Length));
					for (int i = 0; i < array[num2].Length; i++)
					{
						AreaOfEffectDefinition.Pattern[AreaOfEffectDefinition.Pattern.Count - 1].Add(array[num2][i]);
						if (array[num2][i] == 'X')
						{
							AffectedTilesCount++; // Đếm ô hiệu ứng chính
						}
						else if (array[num2][i] == 'e')
						{
							SurroundingEffectTilesCount++; // Đếm ô hiệu ứng bao quanh
						}
						else if (array[num2][i] == 'M')
						{
							num++; // Đếm ô Maneuver
						}
					}
				}
			}

			// Đánh dấu single target nếu chỉ có 1 ô 'X'
			AreaOfEffectDefinition.IsSingleTarget |= AffectedTilesCount == 1;

			// Validate: chỉ cho phép tối đa 1 ô Maneuver
			if (num > 1)
			{
				CLoggerManager.Log("Skill " + Id + " must have 0 or 1 maneuver tile in area of effect!", LogType.Error);
				return;
			}
		}
		else if (skillDefinition != null)
		{
			// Kế thừa AoE từ template
			AreaOfEffectDefinition = skillDefinition.AreaOfEffectDefinition;
			AffectedTilesCount = skillDefinition.AffectedTilesCount;
			SurroundingEffectTilesCount = skillDefinition.SurroundingEffectTilesCount;
		}

		// Parse các cờ xoay/lật AoE
		CanRotate = xElement.Element("CanRotate") != null || (skillDefinition?.CanRotate ?? false);
		CanFlip = xElement.Element("CanFlip") != null || (skillDefinition?.CanFlip ?? false);
		LockAutoOrientation = xElement.Element("LockAutoOrientation") != null || (skillDefinition?.LockAutoOrientation ?? false);

		// Validate: CanRotate và LockAutoOrientation không thể cùng true
		if (CanRotate && LockAutoOrientation)
		{
			CLoggerManager.Log("Both CanRotate and LockAutoOrientation are set to true in the skill " + Id + ", something is probably wrong here.", LogType.Error, CLogLevel.MAJOR);
		}

		// Parse AffectedUnits (loại đơn vị bị ảnh hưởng)
		XElement xElement9 = xElement.Element("AffectedUnits");
		if (xElement9 != null)
		{
			AffectedUnits.Deserialize(xElement9);
		}

		#endregion 5. Parse vùng ảnh hưởng (Area of Effect)

		#region 6. Parse SkillAction (loại hành động của kỹ năng)

		XElement xElement10 = xElement.Element("SkillAction");
		if (xElement10 != null)
		{
			// Duyệt element con để xác định loại SkillAction
			// Mỗi skill chỉ được có 1 SkillAction
			foreach (XElement item in xElement10.Elements())
			{
				if (SkillActionDefinition != null)
				{
					CLoggerManager.Log("Skill " + Id + " already has a skill action!", LogType.Error);
					break;
				}
				switch (item.Name.LocalName)
				{
				case "Attack":
					SkillActionDefinition = new AttackSkillActionDefinition(xElement10);
					continue;
				case "Generic":
					SkillActionDefinition = new GenericSkillActionDefinition(xElement10);
					continue;
				case "GoIntoWatchtower":
					SkillActionDefinition = new GoIntoWatchtowerSkillActionDefinition(xElement10);
					continue;
				case "SkipTurn":
					SkillActionDefinition = new SkipTurnSkillActionDefinition(xElement10);
					continue;
				case "QuitWatchtower":
					SkillActionDefinition = new QuitWatchtowerSkillActionDefinition(xElement10);
					continue;
				case "Spawn":
					SkillActionDefinition = new SpawnSkillActionDefinition(xElement10);
					continue;
				case "Build":
					SkillActionDefinition = new BuildSkillActionDefinition(xElement10);
					continue;
				case "Resupply":
					SkillActionDefinition = new ResupplySkillActionDefinition(xElement10);
					continue;
				}
				CLoggerManager.Log("Unknown skill effect type: " + item.Name.LocalName + " on skill " + Id + ".", LogType.Error);
			}
		}
		else
		{
			// Kế thừa SkillAction từ template
			SkillActionDefinition = skillDefinition.SkillActionDefinition;
		}

		// Validate Maneuver: nếu có effect Maneuver thì phải có ô 'M' trong AoE và ngược lại
		if (SkillActionDefinition.HasEffect("Maneuver"))
		{
			if (num == 0 && skillDefinition == null)
			{
				CLoggerManager.Log("Skill " + Id + " has the maneuver skill effect but no maneuver tile in area of effect!", LogType.Error);
				return;
			}
		}
		else if (num > 0)
		{
			CLoggerManager.Log("Skill " + Id + " has a maneuver tile in area of effect but not the maneuver skill effect!", LogType.Error);
			return;
		}

		#endregion 6. Parse SkillAction (loại hành động của kỹ năng)

		#region 7. Parse ValidTargets (mục tiêu hợp lệ)

		XElement xElement11 = xElement.Element("ValidTargets");
		if (xElement11 != null)
		{
			ValidTargets = new ValidTargets
			{
				Buildings = new Dictionary<string, ValidTargets.Constraints>()
			};

			// Parse từng Building riêng lẻ với ràng buộc MustBeEmpty và NeedRepair
			foreach (XElement item2 in xElement11.Elements("Building"))
			{
				XAttribute xAttribute8 = item2.Attribute("Id");
				if (xAttribute8.IsNullOrEmpty())
				{
					CLoggerManager.Log("ValidTargets' building of skill " + Id + " must have a valid Id", LogType.Error);
					continue;
				}
				XAttribute xAttribute9 = item2.Attribute("MustBeEmpty");
				bool result4 = false;
				if (xAttribute9 != null && !bool.TryParse(xAttribute9.Value, out result4))
				{
					CLoggerManager.Log("Invalid MustBeEmptyAttribute", LogType.Error);
					continue;
				}
				XAttribute xAttribute10 = item2.Attribute("NeedRepair");
				bool result5 = false;
				if (xAttribute10 != null && !bool.TryParse(xAttribute10.Value, out result5))
				{
					CLoggerManager.Log("Invalid NeedRepairAttribute", LogType.Error);
				}
				else
				{
					ValidTargets.Buildings.Add(xAttribute8.Value, new ValidTargets.Constraints(result4, result5));
				}
			}

			// Parse BuildingsList - thêm tất cả building trong danh sách IdsListDefinitions
			foreach (XElement item3 in xElement11.Elements("BuildingsList"))
			{
				XAttribute xAttribute11 = item3.Attribute("Id");
				if (xAttribute11.IsNullOrEmpty())
				{
					CLoggerManager.Log("ValidTargets' buildings list of skill " + Id + " must have a valid Id", LogType.Error);
					continue;
				}
				XAttribute xAttribute12 = item3.Attribute("NeedRepair");
				bool result6 = false;
				if (xAttribute12 != null && !bool.TryParse(xAttribute12.Value, out result6))
				{
					CLoggerManager.Log("Invalid NeedRepairAttribute", LogType.Error);
					continue;
				}
				foreach (string id in GenericDatabase.IdsListDefinitions[xAttribute11.Value].Ids)
				{
					if (!ValidTargets.Buildings.ContainsKey(id))
					{
						ValidTargets.Buildings.Add(id, new ValidTargets.Constraints(mustBeEmpty: false, result6));
					}
				}
			}

			// Parse BuildingCategory - thêm tất cả building thuộc category nhất định
			foreach (XElement item4 in xElement11.Elements("BuildingCategory"))
			{
				XAttribute xAttribute13 = item4.Attribute("Category");
				if (xAttribute13.IsNullOrEmpty() || !Enum.TryParse<BuildingDefinition.E_BuildingCategory>(xAttribute13.Value, out var result7))
				{
					CLoggerManager.Log("ValidTargets' building category of skill " + Id + " must have a valid category : \"" + xAttribute13?.Value + "\"", LogType.Error);
					continue;
				}
				foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
				{
					if (buildingDefinition.Value.BlueprintModuleDefinition.Category.HasFlag(result7))
					{
						ValidTargets.Buildings.Add(buildingDefinition.Key, new ValidTargets.Constraints(mustBeEmpty: false, needRepair: false));
					}
				}
			}

			// Parse các cờ mục tiêu đơn vị và ô
			ValidTargets.PlayableUnits = xElement11.Element("PlayableUnits") != null;
			ValidTargets.EnemyUnits = xElement11.Element("EnemyUnits") != null;
			ValidTargets.EmptyTiles = xElement11.Element("EmptyTiles") != null;
			ValidTargets.WalkableCityTiles = xElement11.Element("WalkableCityTiles") != null;
			ValidTargets.WalkableTiles = xElement11.Element("WalkableTiles") != null;
			ValidTargets.UncrossableGrounds = xElement11.Element("UncrossableGrounds") != null;
		}
		else if (skillDefinition?.ValidTargets != null)
		{
			// Kế thừa từ template
			ValidTargets = skillDefinition.ValidTargets;
		}

		#endregion 7. Parse ValidTargets (mục tiêu hợp lệ)

		#region 8. Parse AllowDuringPhases và DisplayDuringPhases

		// AllowDuringPhases: xác định pha nào cho phép sử dụng kỹ năng
		XElement xElement12 = xElement.Element("AllowDuringPhases");
		if (xElement12 != null)
		{
			foreach (XElement item5 in xElement12.Elements())
			{
				if (!Enum.TryParse<E_Phase>(item5.Name.LocalName, out var result8))
				{
					CLoggerManager.Log("Could not parse " + item5.Name.LocalName + " to a valid E_Phase.", LogType.Error);
					return;
				}
				AllowDuringPhase |= result8; // Dùng OR để kết hợp nhiều pha
			}
		}
		else if (skillDefinition != null)
		{
			AllowDuringPhase = skillDefinition.AllowDuringPhase;
		}
		else
		{
			AllowDuringPhase = E_Phase.Night; // Mặc định: chỉ dùng trong pha đêm
		}

		// DisplayDuringPhases: xác định pha nào hiển thị kỹ năng trong UI
		XElement xElement13 = xElement.Element("DisplayDuringPhases");
		if (xElement13 != null)
		{
			foreach (XElement item6 in xElement13.Elements())
			{
				if (!Enum.TryParse<E_Phase>(item6.Name.LocalName, out var result9))
				{
					CLoggerManager.Log("Could not parse " + item6.Name.LocalName + " to a valid E_Phase.", LogType.Error);
					return;
				}
				DisplayDuringPhase |= result9;
			}
		}
		else if (skillDefinition != null)
		{
			DisplayDuringPhase = skillDefinition.DisplayDuringPhase;
		}
		else
		{
			DisplayDuringPhase = E_Phase.All; // Mặc định: hiển thị ở mọi pha
		}

		#endregion 8. Parse AllowDuringPhases và DisplayDuringPhases

		#region 9. Parse ContextualConditions (điều kiện ngữ cảnh)

		XElement xElement14 = xElement.Element("ContextualConditions");
		if (xElement14 != null)
		{
			ContextualConditions = new List<SkillConditionDefinition>();
			// Duyệt từng element con và tạo condition definition tương ứng
			foreach (XElement item7 in xElement14.Elements())
			{
				switch (item7.Name.ToString())
				{
				case "InPlayableUnitRange":
					ContextualConditions.Add(new InPlayableUnitRangConditionDefinition(item7));
					break;
				case "InWatchtower":
					ContextualConditions.Add(new InWatchtowerConditionDefinition(item7));
					break;
				case "OnlyDuringPhase":
					ContextualConditions.Add(new OnlyDuringPhaseConditionDefinition(item7));
					break;
				case "MaxTargetHealthLeft":
					ContextualConditions.Add(new MaxTargetHealthLeftConditionDefinition(item7));
					break;
				case "NotInBuilding":
					ContextualConditions.Add(new NotInBuildingConditionDefinition(item7));
					break;
				case "NextToBuilding":
					ContextualConditions.Add(new NextToBuildingConditionDefinition(item7));
					break;
				case "OntoBuilding":
					ContextualConditions.Add(new OntoBuildingConditionDefinition(item7));
					break;
				case "MinTargetInjuryStage":
					ContextualConditions.Add(new MinTargetInjuryStageConditionDefinition(item7));
					break;
				}
			}
		}
		else if (skillDefinition != null)
		{
			ContextualConditions = skillDefinition.ContextualConditions;
		}

		#endregion 9. Parse ContextualConditions (điều kiện ngữ cảnh)

		#region 10. Parse InvalidCastDisplayBehaviour và CastFXs

		// InvalidCastDisplayBehaviour: cách hiển thị kỹ năng khi không thể sử dụng
		XElement xElement15 = xElement.Element("InvalidCastDisplayBehaviour");
		if (xElement15 != null)
		{
			if (!Enum.TryParse<E_InvalidCastDisplayBehaviour>(xElement15.Value, out var result10))
			{
				CLoggerManager.Log("Could not parse " + xElement15.Value + " to a valid E_InvalidCastDisplayBehaviour.", LogType.Error);
				return;
			}
			InvalidCastDisplayBehaviour = result10;
		}
		else if (skillDefinition != null)
		{
			InvalidCastDisplayBehaviour = skillDefinition.InvalidCastDisplayBehaviour;
		}
		else
		{
			InvalidCastDisplayBehaviour = E_InvalidCastDisplayBehaviour.Hidden; // Mặc định: ẩn
		}

		// CastFXs: hiệu ứng hình ảnh khi thi hành kỹ năng
		XElement xElement16 = xElement.Element("CastFXs");
		SkillCastFxDefinition = ((xElement16 != null) ? new SkillCastFxDefinition(xElement16) : skillDefinition?.SkillCastFxDefinition);

		// PreCastFXs: hiệu ứng hình ảnh trước khi thi hành
		XElement xElement17 = xElement.Element("PreCastFXs");
		PreSkillCastFxDefinition = ((xElement17 != null) ? new SkillCastFxDefinition(xElement17) : skillDefinition?.PreSkillCastFxDefinition);

		#endregion 10. Parse InvalidCastDisplayBehaviour và CastFXs

		#region 11. Tạo AoE mặc định nếu chưa có

		// Nếu không có AoE definition nào được parse hoặc kế thừa,
		// tạo AoE mặc định là single target (1 ô 'X')
		if (AreaOfEffectDefinition == null)
		{
			AreaOfEffectDefinition = new AreaOfEffectDefinition
			{
				Pattern = new List<List<char>>
				{
					new List<char> { 'X' }
				},
				IsSingleTarget = true
			};
		}

		#endregion 11. Tạo AoE mặc định nếu chưa có
	}

	#endregion Deserialization
}
