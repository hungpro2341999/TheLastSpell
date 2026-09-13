using TheLastStand.Definition;
using TheLastStand.Model;
using TheLastStand.View;

namespace TheLastStand.Controller;

/// <summary>
/// Điều khiển các câu thoại ngắn (Bark) xuất hiện trên đầu nhân vật hoặc kẻ địch trong trận đấu.
/// </summary>
public class BarkController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu câu thoại hiện tại.
	/// </summary>
	public Bark Bark { get; private set; }

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo một BarkController mới, gán View và tạo nội dung câu thoại ngẫu nhiên từ BarkDefinition.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu của nhóm câu thoại.</param>
	/// <param name="view">View hiển thị bong bóng thoại trên giao diện.</param>
	/// <param name="barker">Đối tượng phát ra câu thoại (Hero, Enemy, NPC...).</param>
	public BarkController(BarkDefinition definition, BarkView view, IBarker barker)
	{
		Bark = new Bark(definition, this, view);
		view.Bark = Bark;
		Bark.Barker = barker;
		// Lấy câu thoại phù hợp dựa trên ngữ cảnh và đối tượng phát ngôn
		Bark.Sentence = BarkDefinition.GetSentence(definition, barker);
	}

	#endregion
}
