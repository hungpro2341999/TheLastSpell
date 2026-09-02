using System.Collections;
using TPLib.Yield;
using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class MoveObjectTask : Task
{
	private MonoBehaviour coroutineRunner;

	private Transform target;

	private Vector3 targetPosition;

	private float moveSpeed;

	public MoveObjectTask(MonoBehaviour coroutineRunner, Transform target, Vector3 targetPosition, float moveSpeed)
	{
		this.coroutineRunner = coroutineRunner;
		this.target = target;
		this.targetPosition = targetPosition;
		this.moveSpeed = moveSpeed;
	}

	public override void StartTask()
	{
		base.StartTask();
		coroutineRunner.StartCoroutine(MoveCoroutine());
	}

	public override string ToString()
	{
		return $"{base.ToString()}: {target.name} to {targetPosition} at speed {moveSpeed}";
	}

	private IEnumerator MoveCoroutine()
	{
		while (Vector3.Distance(target.transform.position, targetPosition) > 0.1f)
		{
			target.transform.position = Vector3.Lerp(target.transform.position, targetPosition, moveSpeed);
			yield return SharedYields.WaitForEndOfFrame;
		}
		target.transform.position = targetPosition;
		Complete();
	}
}
