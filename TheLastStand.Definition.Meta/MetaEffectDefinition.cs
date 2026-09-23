using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Lớp cơ sở trừu tượng cho tất cả các bản thiết kế Hiệu ứng Meta (Meta Effect Definition).
/// <para>Mọi nâng cấp trong Oraculum khi kích hoạt sẽ áp dụng một hoặc nhiều MetaEffect kế thừa từ class này.</para>
/// </summary>
public abstract class MetaEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng Meta từ XML container.
	/// </summary>
	/// <param name="container">Đối tượng XML chứa dữ liệu cấu hình của hiệu ứng.</param>
	public MetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Phương thức ảo được gọi khi hiệu ứng Meta được kích hoạt hoặc hủy kích hoạt trong phiên chơi.
	/// <para>Cho phép các class con cập nhật trực tiếp trạng thái runtime (ví dụ: làm mới danh sách item trong ItemRestrictionManager).</para>
	/// </summary>
	/// <param name="hasBeenActivated">True nếu hiệu ứng vừa được kích hoạt; False nếu bị hủy/tắt.</param>
	public virtual void OnMetaEffectActivated(bool hasBeenActivated)
	{
	}
}
