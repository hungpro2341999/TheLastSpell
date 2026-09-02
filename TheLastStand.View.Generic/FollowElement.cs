using System;
using System.Collections.Generic;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Generic;

[RequireComponent(typeof(RectTransform))]
public class FollowElement : MonoBehaviour
{
	[Serializable]
	public class FollowDatas
	{
		[Tooltip("Local offset to add to the final position.")]
		[SerializeField]
		private Vector3 offset = Vector3.zero;

		[Tooltip("If true, the local offset will be multiplied by the scale of this object./n/!\\ The scale used here is the lossy scale, not the local!")]
		[SerializeField]
		private bool offsetAffectedByScale;

		[Tooltip("Leave empty to follow the mouse/joystick cursor (when Always Follow Target is true), or define a target to follow.")]
		[SerializeField]
		private Transform followTarget;

		[SerializeField]
		[FormerlySerializedAs("autoMove")]
		private bool alwaysFollow = true;

		[SerializeField]
		private bool clampToParent;

		public bool AlwaysFollow
		{
			get
			{
				return alwaysFollow;
			}
			set
			{
				alwaysFollow = value;
			}
		}

		public bool ClampToParent
		{
			get
			{
				return clampToParent;
			}
			set
			{
				clampToParent = value;
			}
		}

		public Transform FollowTarget
		{
			get
			{
				return followTarget;
			}
			set
			{
				followTarget = value;
			}
		}

		public Vector3 Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public bool OffsetAffectedByScale
		{
			get
			{
				return offsetAffectedByScale;
			}
			set
			{
				offsetAffectedByScale = value;
			}
		}

		public FollowDatas()
		{
		}

		public FollowDatas(FollowDatas copy)
		{
			alwaysFollow = copy.alwaysFollow;
			offset = new Vector3(copy.offset.x, copy.offset.y, copy.offset.z);
			followTarget = copy.followTarget;
			offsetAffectedByScale = copy.offsetAffectedByScale;
		}
	}

	[SerializeField]
	private FollowDatas followElementDatas = new FollowDatas();

	[SerializeField]
	private UnityEngine.Camera targetCamera;

	private HashSet<MonoBehaviour> scriptsUsingMe;

	private Vector3 initFollowOffset;

	public bool ConvertToScreenSpace;

	public FollowDatas FollowElementDatas => followElementDatas;

	public RectTransform RectTransform { get; private set; }

	private Vector3 ScaledOffset => followElementDatas.Offset * base.transform.lossyScale.x;

	public void AutoMove()
	{
		if (FollowElementDatas.FollowTarget != null)
		{
			SetPosition((ConvertToScreenSpace && targetCamera != null) ? targetCamera.WorldToScreenPoint(FollowElementDatas.FollowTarget.position) : FollowElementDatas.FollowTarget.position);
		}
		else
		{
			SetPosition(InputManager.IsLastControllerJoystick ? ACameraView.MainCam.WorldToScreenPoint(InputManager.JoystickCursorPosition) : InputManager.MousePosition);
		}
	}

	public void ChangeFollowDatas(FollowDatas followDatas)
	{
		FollowElementDatas.AlwaysFollow = followDatas.AlwaysFollow;
		FollowElementDatas.ClampToParent = followDatas.ClampToParent;
		FollowElementDatas.FollowTarget = followDatas.FollowTarget;
		FollowElementDatas.Offset = followDatas.Offset;
		FollowElementDatas.OffsetAffectedByScale = followDatas.OffsetAffectedByScale;
		AutoMove();
	}

	public void RestoreFollowDatasOffset()
	{
		FollowElementDatas.Offset = initFollowOffset;
	}

	public void ChangeOffset(Vector3 offset)
	{
		FollowElementDatas.Offset = offset;
		if (!FollowElementDatas.AlwaysFollow)
		{
			AutoMove();
		}
	}

	public void ChangeTarget(Transform newTarget)
	{
		if (FollowElementDatas.FollowTarget != newTarget)
		{
			FollowElementDatas.FollowTarget = newTarget;
			AutoMove();
		}
	}

	public void SetPosition(Vector3 targetPos)
	{
		if (!(RectTransform == null))
		{
			if ((targetPos += (FollowElementDatas.OffsetAffectedByScale ? ScaledOffset : FollowElementDatas.Offset)) != RectTransform.position)
			{
				RectTransform.position = targetPos;
			}
			if (FollowElementDatas.ClampToParent)
			{
				RectTransformExtensions.ClampToParent(RectTransform);
			}
		}
	}

	public void StartUseBy(MonoBehaviour script)
	{
		if (scriptsUsingMe == null)
		{
			scriptsUsingMe = new HashSet<MonoBehaviour>();
		}
		if (scriptsUsingMe.Add(script))
		{
			base.enabled = true;
		}
	}

	public void StopUseBy(MonoBehaviour script)
	{
		if (scriptsUsingMe != null && scriptsUsingMe.Remove(script) && scriptsUsingMe.Count == 0)
		{
			base.enabled = false;
		}
	}

	private void Awake()
	{
		RectTransform = base.transform as RectTransform;
		initFollowOffset = FollowElementDatas.Offset;
	}

	private void LateUpdate()
	{
		if (FollowElementDatas.AlwaysFollow)
		{
			AutoMove();
		}
	}
}
