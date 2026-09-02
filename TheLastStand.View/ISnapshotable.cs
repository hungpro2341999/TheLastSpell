using UnityEngine;

namespace TheLastStand.View;

public interface ISnapshotable
{
	Transform SnapshotPosition { get; }

	void OnSnapshotFinished();

	void PrepareForSnapshot();
}
