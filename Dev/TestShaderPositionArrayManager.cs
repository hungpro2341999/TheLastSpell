using System.Collections.Generic;
using TPLib;
using UnityEngine;

namespace Dev;

public class TestShaderPositionArrayManager : TPSingleton<TestShaderPositionArrayManager>
{
	[SerializeField]
	private List<TestShaderPositionArrayTarget> targets = new List<TestShaderPositionArrayTarget>();

	private Vector4[] positions;

	private bool isInitialized;

	[ContextMenu("Init (only works once!)")]
	private void Init()
	{
		positions = new Vector4[targets.Count];
		Shader.SetGlobalInt("_Positions_Length", positions.Length);
		isInitialized = true;
	}

	private void Update()
	{
		if (isInitialized)
		{
			for (int i = 0; i < positions.Length; i++)
			{
				positions[i] = targets[i].ShaderPosition;
			}
			Shader.SetGlobalVectorArray("_Positions", positions);
		}
	}
}
