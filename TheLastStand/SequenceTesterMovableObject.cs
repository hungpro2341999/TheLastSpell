using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class SequenceTesterMovableObject : TPSingleton<SequenceTesterMovableObject>
{
	[SerializeField]
	private Transform movableTransform;

	public TaskGroup TaskGroup { get; private set; }

	private void Start()
	{
		TaskGroup = new TaskGroup(new DebugTask("Start sequence..."), new MoveObjectTask(this, movableTransform, new Vector3(0f, 10f), 0.1f), new CoroutineTask(this, WaitCoroutine(2f), "Waiting a bit"));
		TaskGroup.Append(new TaskGroup(new MoveObjectTask(this, movableTransform, new Vector3(-5f, 10f), 0.05f), new MoveCameraTask(new Vector3(-5f, 10f, -10f), 1f)));
		TaskGroup.Run();
	}

	private IEnumerator WaitCoroutine(float waitingTime)
	{
		yield return SharedYields.WaitForSeconds(waitingTime);
	}
}
