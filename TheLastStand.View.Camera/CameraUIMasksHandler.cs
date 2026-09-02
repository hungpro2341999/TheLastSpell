using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib.Log;
using TheLastStand.Framework.Maths;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraUIMasksHandler : SerializedMonoBehaviour
{
	public class Polygon
	{
		private readonly UnityEngine.Camera camera;

		private readonly Canvas relatedCanvas;

		private readonly Transform[] cornersTransforms;

		private readonly List<Vector2> cornersPositions = new List<Vector2>();

		private Vector2 centre = Vector2.zero;

		private float boundingCircleInSqr;

		private float boundingCircleOutSqr;

		public Vector2[] CornersWorldSpace { get; }

		public Polygon(Transform[] corners, Canvas relatedCanvas, UnityEngine.Camera camera)
		{
			this.camera = camera;
			cornersTransforms = corners;
			CornersWorldSpace = new Vector2[corners.Length];
			this.relatedCanvas = relatedCanvas;
			Refresh();
		}

		public static float GetFurthestCornerToCenterSqr(Vector2[] polygon, Vector3 centre)
		{
			float num = 0f;
			for (int num2 = polygon.Length - 1; num2 >= 0; num2--)
			{
				float sqrMagnitude = (polygon[num2] - (Vector2)centre).sqrMagnitude;
				if (num < sqrMagnitude)
				{
					num = sqrMagnitude;
				}
			}
			return num;
		}

		public bool IsPointInside(Vector2 point)
		{
			if (relatedCanvas != null && !relatedCanvas.enabled)
			{
				return false;
			}
			if (cornersTransforms != null && cornersTransforms[0] != null && !cornersTransforms[0].gameObject.activeInHierarchy)
			{
				return false;
			}
			Refresh();
			if ((point - centre).sqrMagnitude < boundingCircleInSqr && boundingCircleInSqr < float.PositiveInfinity)
			{
				CLoggerManager.Log("Point is hidden (canvas: " + ((relatedCanvas != null) ? relatedCanvas.name : "None") + ").", (relatedCanvas != null) ? relatedCanvas.gameObject : null, LogType.Log, CLogLevel.DETAILED);
				return true;
			}
			if ((point - centre).sqrMagnitude > boundingCircleOutSqr)
			{
				return false;
			}
			cornersPositions.Clear();
			cornersPositions.AddRange(CornersWorldSpace);
			cornersPositions.Add(CornersWorldSpace[^1]);
			int num = 0;
			for (int num2 = CornersWorldSpace.Length - 1; num2 >= 0; num2--)
			{
				if (cornersPositions[num2].x <= point.x)
				{
					if (cornersPositions[num2 + 1].x > point.x && Maths.IsPointLeftToEdge(cornersPositions[num2], cornersPositions[num2 + 1], point) > 0)
					{
						num++;
					}
				}
				else if (cornersPositions[num2 + 1].x <= point.x && Maths.IsPointLeftToEdge(cornersPositions[num2], cornersPositions[num2 + 1], point) < 0)
				{
					num--;
				}
			}
			if (num != 0)
			{
				CLoggerManager.Log("Point is hidden (canvas: " + ((relatedCanvas != null) ? relatedCanvas.name : "None") + ").", (relatedCanvas != null) ? relatedCanvas.gameObject : null, LogType.Log, CLogLevel.DETAILED);
			}
			return num != 0;
		}

		private void Refresh()
		{
			for (int i = 0; i < cornersTransforms.Length; i++)
			{
				if (cornersTransforms[i] == null)
				{
					CLoggerManager.Log("Missing RectTransform found while refreshing Polygon corners (canvas: " + ((relatedCanvas != null) ? relatedCanvas.name : "None") + ").", (relatedCanvas != null) ? relatedCanvas.gameObject : null, LogType.Warning);
					break;
				}
				CornersWorldSpace[i] = camera.ScreenToWorldPoint(cornersTransforms[i].position);
			}
			centre = Maths.ComputePolygonCentre(CornersWorldSpace);
			boundingCircleInSqr = (centre - Maths.GetClosestPointOnPolygon(CornersWorldSpace, centre)).sqrMagnitude;
			boundingCircleOutSqr = GetFurthestCornerToCenterSqr(CornersWorldSpace, centre);
		}
	}

	[SerializeField]
	private UnityEngine.Camera mainCamera;

	[SerializeField]
	private GridLayout grid;

	private readonly List<Polygon> polygons = new List<Polygon>();

	public UnityEngine.Camera Camera => mainCamera;

	public void RegisterMask(CameraUIMask mask)
	{
		polygons.Add(new Polygon(mask.Vertices, mask.Canvas, Camera));
	}

	public bool IsPointOffscreen(Vector3 point)
	{
		return !IsPointInCameraFrustum(point);
	}

	public bool IsPointOffscreenOrHiddenByUI(Vector3 point)
	{
		if (IsPointOffscreen(point))
		{
			return true;
		}
		if (polygons == null)
		{
			return false;
		}
		for (int num = polygons.Count - 1; num >= 0; num--)
		{
			if (polygons[num].IsPointInside(point))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPointOffscreenOrHiddenByUI(Tile tile)
	{
		return IsPointOffscreenOrHiddenByUI(grid.CellToWorld(new Vector3Int(tile.X, tile.Y, 0)));
	}

	public bool IsPointOffscreenOrHiddenByUI(Vector3 point, Vector3 offset)
	{
		return IsPointOffscreenOrHiddenByUI(point + offset);
	}

	public bool IsPointOffscreenOrHiddenByUI(Tile tile, Vector3 offset)
	{
		return IsPointOffscreenOrHiddenByUI(grid.CellToWorld(new Vector3Int(tile.X, tile.Y, 0)) + offset);
	}

	private bool IsPointInCameraFrustum(Vector3 point, float boundsSize = 0f)
	{
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), new Bounds(point, Vector3.one * boundsSize));
	}
}
