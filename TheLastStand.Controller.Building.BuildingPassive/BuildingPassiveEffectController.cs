using TheLastStand.Model.Building.BuildingPassive;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) cho tất cả các Controller hiệu ứng nội tại của công trình.
/// Định nghĩa các phương thức vòng đời: Áp dụng (Apply), Cải thiện (ImproveEffect), Hoàn tác (Unapply) và Khi chết (OnDeath).
/// </summary>
public abstract class BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của hiệu ứng nội tại công trình tương ứng.
	/// </summary>
	public BuildingPassiveEffect BuildingPassiveEffect { get; protected set; }

	#endregion

	#region Lifecycle Methods

	/// <summary>
	/// Áp dụng hiệu ứng nội tại lên công trình, tài nguyên hoặc môi trường trò chơi.
	/// </summary>
	public abstract void Apply();

	/// <summary>
	/// Tăng cường sức mạnh hoặc giá trị chỉ số của hiệu ứng (thường khi nâng cấp công trình).
	/// </summary>
	/// <param name="bonus">Giá trị cộng thêm.</param>
	public virtual void ImproveEffect(int bonus)
	{
	}

	/// <summary>
	/// Hoàn tác / hủy áp dụng hiệu ứng (dành cho các hiệu ứng dạng duy trì/vĩnh viễn khi công trình bị phá dỡ).
	/// </summary>
	public virtual void Unapply()
	{
	}

	/// <summary>
	/// Xử lý logic phản hồi hoặc dọn dẹp khi công trình bị tiêu diệt / phá hủy hoàn toàn.
	/// </summary>
	public virtual void OnDeath()
	{
	}

	#endregion
}
