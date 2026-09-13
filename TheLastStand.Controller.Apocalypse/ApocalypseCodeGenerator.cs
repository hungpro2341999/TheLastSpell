using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse;

namespace TheLastStand.Controller.Apocalypse;

/// <summary>
/// Quản lý việc tạo (Encode) và giải mã (Decode) chuỗi mã chia sẻ cấu hình Apocalypse (Apocalypse Sharing Code).
/// Cho phép người chơi xuất mã cấu hình độ khó hoặc nhập mã từ người chơi khác.
/// </summary>
public static class ApocalypseCodeGenerator
{
	#region Data Structures & Enums

	/// <summary>
	/// Định nghĩa các nguyên nhân thất bại khi giải mã chuỗi mã Apocalypse.
	/// </summary>
	public enum E_FailureReason
	{
		/// <summary>Không có lỗi, giải mã thành công.</summary>
		None = -1,
		/// <summary>Định dạng chuỗi mã không hợp lệ (sai cú pháp regex, độ dài không khớp, ký tự lạ).</summary>
		InvalidCodeFormat,
		/// <summary>Mã định danh rút gọn (CodeSharingId) của Modifier không tồn tại trong Database.</summary>
		InvalidModifierCodeSharingId,
		/// <summary>Modifier này chưa được người chơi mở khóa.</summary>
		LockedModifier,
		/// <summary>Chỉ số cấp độ (Step Index) vượt quá giới hạn số bước được định nghĩa cho Modifier.</summary>
		InvalidStepIndex,
		/// <summary>Modifier bị lặp lại nhiều lần trong cùng một chuỗi mã.</summary>
		DuplicateModifier
	}

	/// <summary>
	/// Đối tượng chứa kết quả và dữ liệu chi tiết sau khi giải mã chuỗi mã Apocalypse.
	/// </summary>
	public class ApocalypseCodeDecodingData
	{
		private StringBuilder failureMessage = new StringBuilder();

		/// <summary>Nguyên nhân thất bại (mặc định là None nếu thành công).</summary>
		public E_FailureReason FailureReason { get; set; } = E_FailureReason.None;

		/// <summary>Đoạn mã con gây ra lỗi (ví dụ: "HP1").</summary>
		public string FailureModifierCode { get; set; }

		/// <summary>Định nghĩa của Modifier gặp lỗi.</summary>
		public ApocalypseModifierDefinition FailureModifierDefinition { get; set; }

		/// <summary>Mã chia sẻ không hợp lệ tìm thấy trong chuỗi.</summary>
		public string InvalidCodeSharingId { get; set; }

		/// <summary>Chỉ số bước không hợp lệ tìm thấy trong chuỗi.</summary>
		public int InvalidStepIndex { get; set; }

		/// <summary>Danh sách các định nghĩa bước Modifier giải mã thành công.</summary>
		public List<ApocalypseModifierStepDefinition> ModifierStepDefinitions { get; private set; } = new List<ApocalypseModifierStepDefinition>();

		/// <summary>Kiểm tra xem quá trình giải mã có thành công hoàn toàn hay không.</summary>
		public bool Success => FailureReason == E_FailureReason.None;

		/// <summary>
		/// Lấy thông báo lỗi đã được bản địa hóa (Localized) dựa trên nguyên nhân thất bại.
		/// </summary>
		/// <returns>Chuỗi thông báo lỗi hiển thị cho người chơi, hoặc rỗng nếu thành công.</returns>
		public string GetFailureMessage()
		{
			if (Success)
			{
				return string.Empty;
			}
			failureMessage.Clear();
			switch (FailureReason)
			{
			case E_FailureReason.DuplicateModifier:
				// Lỗi trùng lặp modifier trong cùng một cấu hình
				failureMessage.Append(Localizer.Format("ApocalypseEditCodePopup_Error_DuplicateModifier", FailureModifierDefinition.GetLocalizedTitle(), FailureModifierCode));
				break;
			case E_FailureReason.LockedModifier:
				// Lỗi modifier chưa được mở khóa bởi người chơi
				failureMessage.Append(Localizer.Format("ApocalypseEditCodePopup_Error_LockedModifier", FailureModifierDefinition.GetLocalizedTitle(), FailureModifierCode));
				break;
			case E_FailureReason.InvalidCodeFormat:
				// Lỗi sai định dạng cấu trúc chuỗi mã
				failureMessage.Append(Localizer.Get("ApocalypseEditCodePopup_Error_InvalidCodeFormat"));
				break;
			case E_FailureReason.InvalidStepIndex:
				// Lỗi chỉ số cấp độ của modifier không hợp lệ hoặc vượt ngưỡng
				failureMessage.Append(Localizer.Format("ApocalypseEditCodePopup_Error_InvalidModifierStepIndex", FailureModifierDefinition.GetLocalizedTitle(), FailureModifierCode, InvalidStepIndex));
				break;
			case E_FailureReason.InvalidModifierCodeSharingId:
				// Lỗi không tìm thấy mã định danh modifier trong hệ thống
				failureMessage.Append(Localizer.Get("ApocalypseEditCodePopup_Error_InvalidCodeFormat"));
				break;
			}
			return failureMessage.ToString();
		}

		/// <summary>
		/// Đặt lại toàn bộ dữ liệu kết quả giải mã về trạng thái ban đầu.
		/// </summary>
		public void Reset()
		{
			FailureReason = E_FailureReason.None;
			FailureModifierCode = string.Empty;
			ModifierStepDefinitions.Clear();
			FailureModifierDefinition = null;
			failureMessage.Clear();
		}
	}

	#endregion

	#region Constants & Regex

	/// <summary>
	/// Các hằng số định nghĩa khóa ngôn ngữ và biểu thức chính quy (Regex).
	/// </summary>
	public static class Constants
	{
		public static class LocalizationKeys
		{
			public const string CodeErrorDuplicateModifier = "ApocalypseEditCodePopup_Error_DuplicateModifier";

			public const string CodeErrorInvalidFormat = "ApocalypseEditCodePopup_Error_InvalidCodeFormat";

			public const string CodeErrorInvalidModifierStepIndex = "ApocalypseEditCodePopup_Error_InvalidModifierStepIndex";

			public const string CodeErrorLockedModifier = "ApocalypseEditCodePopup_Error_LockedModifier";
		}

		/// <summary>Pattern nhận diện các cặp token [Ký tự hoa][Chữ số], ví dụ: "ABC0", "XYZ12".</summary>
		public const string DecodingRegex = "([A-Z]+[0-9]+)";

		/// <summary>Pattern trích xuất phần mã định danh modifier (chuỗi ký tự viết hoa).</summary>
		public const string CodeSharingIdRegex = "[A-Z]+";

		/// <summary>Pattern trích xuất phần chỉ số cấp độ (chuỗi số).</summary>
		public const string StepIndexRegex = "[0-9]+";
	}

	#endregion

	#region Code Generation & Decoding

	/// <summary>
	/// Tạo chuỗi mã chia sẻ từ cấu hình Apocalypse hiện tại.
	/// </summary>
	/// <param name="apocalypse">Model Apocalypse chứa danh sách các bước modifier đã chọn.</param>
	/// <returns>Chuỗi mã ghép các token lại với nhau (ví dụ: "HP0WAVE1"), hoặc rỗng nếu không có modifier nào.</returns>
	public static string GenerateCode(TheLastStand.Model.Apocalypse.Apocalypse apocalypse)
	{
		if (apocalypse == null || apocalypse.ModifierStepDefinitions.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ApocalypseModifierStepDefinition modifierStepDefinition in apocalypse.ModifierStepDefinitions)
		{
			// Lấy định dạng mã chia sẻ của từng bước modifier (ví dụ: "HP0")
			string value = modifierStepDefinition.ToCodeSharingFormat();
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Thử giải mã một chuỗi mã Apocalypse thành danh sách các bước Modifier hợp lệ.
	/// Tiến hành kiểm tra cú pháp, sự tồn tại trong database, trạng thái mở khóa, giới hạn chỉ số và trùng lặp.
	/// </summary>
	/// <param name="apocalypseCode">Chuỗi mã nhập vào (ví dụ từ clipboard).</param>
	/// <returns>Đối tượng ApocalypseCodeDecodingData chứa kết quả phân tích và danh sách bước giải mã.</returns>
	public static ApocalypseCodeDecodingData TryDecodeCode(string apocalypseCode)
	{
		ApocalypseCodeDecodingData apocalypseCodeDecodingData = new ApocalypseCodeDecodingData();
		
		// Tìm tất cả các cặp token hợp lệ dạng [Chữ hoa][Chữ số]
		MatchCollection matchCollection = Regex.Matches(apocalypseCode, "([A-Z]+[0-9]+)");
		if (matchCollection.Count == 0)
		{
			apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidCodeFormat;
			return apocalypseCodeDecodingData;
		}
		
		int length = apocalypseCode.Length;
		int num = 0; // Đếm tổng số ký tự của các token hợp lệ tìm được
		
		foreach (Match item2 in matchCollection)
		{
			// Tách riêng phần chữ (ID) và phần số (Step Index)
			Match match2 = Regex.Match(item2.Value, "[A-Z]+");
			Match match3 = Regex.Match(item2.Value, "[0-9]+");
			if (!match2.Success || !match3.Success)
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidCodeFormat;
				return apocalypseCodeDecodingData;
			}
			
			// 1. Kiểm tra ID có tồn tại trong Database không
			if (!ApocalypseDatabase.ModifierDefinitionsFromCodeSharingId.TryGetValue(match2.Value, out var value))
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidModifierCodeSharingId;
				apocalypseCodeDecodingData.InvalidCodeSharingId = match2.Value;
				return apocalypseCodeDecodingData;
			}
			
			// 2. Kiểm tra Modifier đã được người chơi mở khóa chưa
			if (!ApocalypseManager.IsModifierUnlocked(value))
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.LockedModifier;
				apocalypseCodeDecodingData.FailureModifierCode = item2.Value;
				apocalypseCodeDecodingData.FailureModifierDefinition = value;
				return apocalypseCodeDecodingData;
			}
			
			// 3. Kiểm tra chỉ số cấp độ có vượt quá số bước được định nghĩa không
			int num2 = int.Parse(match3.Value);
			if (num2 >= value.StepDefinitions.Count)
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidStepIndex;
				apocalypseCodeDecodingData.InvalidStepIndex = num2;
				apocalypseCodeDecodingData.FailureModifierCode = item2.Value;
				apocalypseCodeDecodingData.FailureModifierDefinition = value;
				return apocalypseCodeDecodingData;
			}
			
			// 4. Kiểm tra có bị trùng lặp cùng một modifier step trong chuỗi không
			ApocalypseModifierStepDefinition item = value.StepDefinitions[num2];
			if (apocalypseCodeDecodingData.ModifierStepDefinitions.Contains(item))
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.DuplicateModifier;
				apocalypseCodeDecodingData.FailureModifierCode = item2.Value;
				apocalypseCodeDecodingData.FailureModifierDefinition = value;
				return apocalypseCodeDecodingData;
			}
			
			num += match2.Value.Length + match3.Value.Length;
			apocalypseCodeDecodingData.ModifierStepDefinitions.Add(item);
		}
		
		// Nếu tổng độ dài các token không bằng tổng độ dài chuỗi đầu vào (có chứa ký tự rác/ký tự lạ)
		if (length != num)
		{
			apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidCodeFormat;
			return apocalypseCodeDecodingData;
		}
		
		return apocalypseCodeDecodingData;
	}

	#endregion
}
