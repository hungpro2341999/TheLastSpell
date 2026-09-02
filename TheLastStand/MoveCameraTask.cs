using DG.Tweening;
using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class MoveCameraTask : Task
{
	private Vector3 targetPosition;

	private float duration;

	public MoveCameraTask(Vector3 targetPosition, float duration)
	{
		this.targetPosition = targetPosition;
		this.duration = duration;
	}

	public override void StartTask()
	{
		base.StartTask();
		Camera.main.transform.DOMove(targetPosition, duration, snapping: true).OnComplete(delegate
		{
			Complete();
		});
	}

	public override string ToString()
	{
		return $"{base.ToString()}: Camera to {targetPosition} during {duration} seconds";
	}
}
