using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLastStand.Dev;

public static class KMeans
{
	private static System.Random rnd = new System.Random();

	public static KMeansClustersInfo GetBestClusters(List<Vector2> data, int k, int repetitions = 1, int maxIterations = 50, int[] initialDataIndexCentroids = null, Func<KMeansClustersInfo, bool> clusterValidator = null)
	{
		KMeansClustersInfo kMeansClustersInfo = null;
		bool? flag = null;
		for (int i = 0; i < repetitions; i++)
		{
			KMeansClustersInfo clusters = GetClusters(data, k, maxIterations, initialDataIndexCentroids);
			bool? flag2 = clusterValidator?.Invoke(clusters);
			if ((kMeansClustersInfo == null || flag != true || flag2 == true) && (kMeansClustersInfo == null || (flag2 == true && flag != true) || (flag2 != false && clusters.totalDistance < kMeansClustersInfo.totalDistance)))
			{
				kMeansClustersInfo = clusters;
				flag = flag2;
			}
			if (kMeansClustersInfo.totalDistance == 0f)
			{
				break;
			}
		}
		return kMeansClustersInfo;
	}

	public static KMeansClustersInfo GetClusters(List<Vector2> data, int k, int maxIterations, int[] initialDataIndexCentroids = null)
	{
		KMeansClustersInfo kMeansClustersInfo = new KMeansClustersInfo(data.Count, k);
		bool flag = true;
		if (initialDataIndexCentroids != null && initialDataIndexCentroids.Length == k)
		{
			kMeansClustersInfo.dataIndexCentroids = initialDataIndexCentroids;
			AssignClusters(data, kMeansClustersInfo, k);
		}
		else
		{
			kMeansClustersInfo.clusterIdByData = RandomlyAssignClusters(data.Count, k);
		}
		while (flag && ++kMeansClustersInfo.iterations < maxIterations)
		{
			UpdateClustersInfo(data, kMeansClustersInfo, k);
			flag = AssignClusters(data, kMeansClustersInfo, k);
		}
		return kMeansClustersInfo;
	}

	private static bool AssignClusters(List<Vector2> data, KMeansClustersInfo kMeansClustersInfo, int clusterCount)
	{
		bool result = false;
		for (int i = 0; i < data.Count; i++)
		{
			float num = float.MaxValue;
			int num2 = -1;
			for (int j = 0; j < clusterCount; j++)
			{
				float magnitude = (data[i] - kMeansClustersInfo.means[j]).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					num2 = j;
				}
			}
			if (num2 != -1 && kMeansClustersInfo.clusterIdByData[i] != num2)
			{
				result = true;
				kMeansClustersInfo.clusterIdByData[i] = num2;
			}
		}
		return result;
	}

	private static void UpdateClustersInfo(List<Vector2> data, KMeansClustersInfo kMeansClustersInfo, int clusterCount)
	{
		kMeansClustersInfo.clusterSizes = new int[clusterCount];
		kMeansClustersInfo.maxDistanceByCluster = new Vector2[clusterCount];
		kMeansClustersInfo.minDistanceByCluster = new Vector2[clusterCount].Select((Vector2 x) => Vector2.one * float.MaxValue).ToArray();
		kMeansClustersInfo.means = new Vector2[clusterCount];
		kMeansClustersInfo.totalDistance = 0f;
		for (int num = 0; num < data.Count; num++)
		{
			Vector2 vector = data[num];
			int num2 = kMeansClustersInfo.clusterIdByData[num];
			kMeansClustersInfo.clusterSizes[num2]++;
			kMeansClustersInfo.means[num2] += vector;
		}
		for (int num3 = 0; num3 < clusterCount; num3++)
		{
			int num4 = kMeansClustersInfo.clusterSizes[num3];
			kMeansClustersInfo.means[num3] /= (float)((num4 <= 0) ? 1 : num4);
		}
		for (int num5 = 0; num5 < data.Count; num5++)
		{
			int num6 = kMeansClustersInfo.clusterIdByData[num5];
			Vector2 vector2 = data[num5] - kMeansClustersInfo.means[num6];
			kMeansClustersInfo.totalDistance += vector2.magnitude;
			if (vector2.magnitude < kMeansClustersInfo.minDistanceByCluster[num6].magnitude)
			{
				kMeansClustersInfo.minDistanceByCluster[num6] = vector2;
				kMeansClustersInfo.dataIndexCentroids[num6] = num5;
			}
			if (Mathf.Abs(vector2.x) > kMeansClustersInfo.maxDistanceByCluster[num6].x)
			{
				kMeansClustersInfo.maxDistanceByCluster[num6].x = Mathf.Abs(vector2.x);
			}
			if (Mathf.Abs(vector2.y) > kMeansClustersInfo.maxDistanceByCluster[num6].y)
			{
				kMeansClustersInfo.maxDistanceByCluster[num6].y = Mathf.Abs(vector2.y);
			}
		}
	}

	private static int[] RandomlyAssignClusters(int dataCount, int clusterCount)
	{
		int[] array = new int[dataCount];
		for (int i = 0; i < dataCount; i++)
		{
			array[i] = rnd.Next(0, clusterCount);
		}
		return array;
	}
}
