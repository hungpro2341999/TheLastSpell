using System.Collections.Generic;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller;

/// <summary>
/// Interface cho các Controller của mọi đối tượng chiếm ô trên bản đồ lưới TileMap (Unit, Building, BonePile, Brazier...).
/// </summary>
public interface ITileObjectController
{
	#region Adjacent & Range Queries

	/// <summary>
	/// Lấy danh sách các ô Tile liền kề theo 4 hướng chính (Bắc, Nam, Đông, Tây).
	/// </summary>
	/// <returns>Danh sách các ô liền kề.</returns>
	List<Tile> GetAdjacentTiles();

	/// <summary>
	/// Lấy danh sách các ô Tile liền kề theo cả 8 hướng (bao gồm 4 hướng chéo).
	/// </summary>
	/// <returns>Danh sách các ô liền kề kể cả đường chéo.</returns>
	List<Tile> GetAdjacentTilesWithDiagonals();

	/// <summary>
	/// Lấy danh sách các ô Tile nằm trong khoảng khoảng cách xác định tính từ đối tượng.
	/// </summary>
	/// <param name="maxRange">Khoảng cách tối đa.</param>
	/// <param name="minRange">Khoảng cách tối thiểu (mặc định: 0).</param>
	/// <param name="cardinalOnly">Chỉ lấy theo 4 hướng chính hay lấy toàn bộ (mặc định: false).</param>
	/// <returns>Danh sách các ô Tile nằm trong tầm.</returns>
	List<Tile> GetTilesInRange(int maxRange, int minRange = 0, bool cardinalOnly = false);

	/// <summary>
	/// Lấy danh sách các ô trong tầm kèm theo ánh xạ ô bị chiếm gần nhất tương ứng.
	/// </summary>
	/// <param name="maxRange">Khoảng cách tối đa.</param>
	/// <param name="minRange">Khoảng cách tối thiểu.</param>
	/// <param name="cardinalOnly">Chỉ lấy theo 4 hướng chính.</param>
	/// <returns>Từ điển ánh xạ ô mục tiêu sang ô bị chiếm gần nhất.</returns>
	Dictionary<Tile, Tile> GetTilesInRangeWithClosestOccupiedTile(int maxRange, int minRange = 0, bool cardinalOnly = false);

	#endregion

	#region Tile Occupation Management

	/// <summary>
	/// Giải phóng toàn bộ các ô Tile mà đối tượng đang chiếm đóng (khi di chuyển hoặc bị phá hủy).
	/// </summary>
	void FreeOccupiedTiles();

	#endregion
}
