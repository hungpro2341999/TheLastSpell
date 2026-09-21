using System.Collections.Generic;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Định nghĩa các loại mục tiêu hợp lệ mà một kỹ năng có thể nhắm tới.
/// Bao gồm: công trình (Buildings), đơn vị đồng minh (PlayableUnits), đơn vị kẻ địch (EnemyUnits),
/// ô trống (EmptyTiles), ô đi được trong thành phố (WalkableCityTiles), ô đi được (WalkableTiles),
/// và ô không thể đi qua (UncrossableGrounds).
/// Khi kỹ năng được sử dụng, hệ thống kiểm tra ValidTargets để xác định
/// ô nào trên bản đồ là mục tiêu hợp lệ (highlight xanh/đỏ).
/// </summary>
public class ValidTargets
{
	#region Nested Types

	/// <summary>
	/// Ràng buộc bổ sung cho mục tiêu là công trình (Building).
	/// </summary>
	public class Constraints
	{
		#region Properties

		/// <summary>
		/// True nếu công trình mục tiêu phải trống (không có đơn vị đứng bên trong).
		/// </summary>
		public bool MustBeEmpty { get; set; }

		/// <summary>
		/// True nếu công trình mục tiêu phải đang bị hư hỏng (cần sửa chữa).
		/// Ví dụ: kỹ năng Repair chỉ nhắm vào công trình đã mất máu.
		/// </summary>
		public bool NeedRepair { get; set; }

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Khởi tạo ràng buộc cho mục tiêu công trình.
		/// </summary>
		/// <param name="mustBeEmpty">Công trình phải trống không có đơn vị.</param>
		/// <param name="needRepair">Công trình phải đang cần sửa chữa.</param>
		public Constraints(bool mustBeEmpty, bool needRepair)
		{
			MustBeEmpty = mustBeEmpty;
			NeedRepair = needRepair;
		}

		#endregion Constructors
	}

	#endregion Nested Types

	#region Properties

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào TẤT CẢ các loại đơn vị (cả đồng minh lẫn kẻ địch).
	/// Yêu cầu cả EnemyUnits và PlayableUnits đều là true.
	/// </summary>
	public bool AllUnits
	{
		get
		{
			if (EnemyUnits)
			{
				return PlayableUnits;
			}
			return false;
		}
	}

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào BẤT KỲ loại đơn vị nào (đồng minh HOẶC kẻ địch).
	/// Chỉ cần một trong hai EnemyUnits/PlayableUnits là true.
	/// </summary>
	public bool AnyUnits
	{
		get
		{
			if (!EnemyUnits)
			{
				return PlayableUnits;
			}
			return true;
		}
	}

	/// <summary>
	/// Dictionary chứa danh sách công trình hợp lệ làm mục tiêu.
	/// Key = BuildingDefinitionId, Value = Constraints (ràng buộc thêm cho công trình đó).
	/// </summary>
	public Dictionary<string, Constraints> Buildings { get; set; }

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào ô trống (không có đơn vị hay công trình).
	/// </summary>
	public bool EmptyTiles { get; set; }

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào đơn vị kẻ địch.
	/// </summary>
	public bool EnemyUnits { get; set; }

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào đơn vị đồng minh (Playable Units / Heroes).
	/// </summary>
	public bool PlayableUnits { get; set; }

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào ô đi được nằm trong phạm vi thành phố.
	/// </summary>
	public bool WalkableCityTiles { get; set; }

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào bất kỳ ô đi được nào trên bản đồ.
	/// </summary>
	public bool WalkableTiles { get; set; }

	/// <summary>
	/// True nếu kỹ năng có thể nhắm vào ô không thể đi qua (UncrossableGround).
	/// </summary>
	public bool UncrossableGrounds { get; set; }

	#endregion Properties
}
