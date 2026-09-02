using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class DebugTask : Task
{
	private string debugString;

	public DebugTask(string debugString)
	{
		this.debugString = debugString;
	}

	public override void StartTask()
	{
		base.StartTask();
		Debug.Log(debugString);
		Complete();
	}

	public override string ToString()
	{
		return base.ToString() + ": " + debugString;
	}
}
