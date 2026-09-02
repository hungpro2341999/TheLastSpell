using UnityEngine;

namespace TheLastStand.Dev;

public class KMeansClustersInfo
{
	public int[] clusterIdByData;

	public int[] clusterSizes;

	public int[] dataIndexCentroids;

	public Vector2[] maxDistanceByCluster;

	public Vector2[] minDistanceByCluster;

	public int iterations = -1;

	public Vector2[] means;

	public float totalDistance;

	public KMeansClustersInfo(int dataCount, int clusterCount)
	{
		clusterIdByData = new int[dataCount];
		clusterSizes = new int[clusterCount];
		dataIndexCentroids = new int[clusterCount];
	}
}
