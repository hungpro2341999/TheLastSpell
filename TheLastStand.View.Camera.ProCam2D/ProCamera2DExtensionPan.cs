using System;
using Com.LuisPedroFonseca.ProCamera2D;
using Rewired;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Cursor;
using UnityEngine;

namespace TheLastStand.View.Camera.ProCam2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-pan-and-zoom/")]
public class ProCamera2DExtensionPan : BasePC2D, IPreMover
{
	public enum MouseButton
	{
		Left,
		Right,
		Middle
	}

	public Action OnPanStarted;

	public Action OnPanFinished;

	[Tooltip("If enabled, the user will be able to manually pan the camera")]
	public bool AllowPan = true;

	[Tooltip("Pan the camera by dragging the 'world'")]
	public bool UsePanByDrag = true;

	[Tooltip("Which mouse button do you want to use for panning? Only applicable if mouse is used")]
	public MouseButton PanMouseButton;

	[Tooltip("A normalized screen space area where the drag is active. Leave to default to use the whole screen")]
	public Rect DraggableAreaRect = new Rect(0f, 0f, 1f, 1f);

	[Tooltip("The speed at which to pan the camera")]
	public Vector2 DragPanSpeedMultiplier = new Vector2(1f, 1f);

	[Tooltip("How fast the camera inertia stops once the user starts dragging")]
	[Range(0f, 1f)]
	public float StopSpeedOnDragStart = 0.95f;

	[Tooltip("Pan the camera by moving the mouse to the edges of the screen")]
	public bool UsePanByMoveToEdges;

	[Tooltip("Pan the camera by moving the mouse to the edges of the screen")]
	public bool IgnoreUIOnEdges;

	[Tooltip("The speed at which the camera will move when the mouse reaches the edges of the screen")]
	public Vector2 EdgesPanSpeed = new Vector2(2f, 2f);

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving vertically")]
	[Range(0f, 0.99f)]
	public float TopPanEdge = 0.9f;

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving vertically")]
	[Range(0f, 0.99f)]
	public float BottomPanEdge = 0.9f;

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving horizontally")]
	[Range(0f, 0.99f)]
	public float LeftPanEdge = 0.9f;

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving horizontally")]
	[Range(0f, 0.99f)]
	public float RightPanEdge = 0.9f;

	[Tooltip("Pan the camera by using keyboard (or controller, actually) inputs")]
	public bool UsePanByKeyboard = true;

	[Tooltip("The speed at which the camera will move when moved by the keyboard")]
	public Vector2 KeyboardPanSpeed = new Vector2(20f, 20f);

	[HideInInspector]
	public bool IsPanning;

	[HideInInspector]
	public bool ResetPrevPanPoint;

	private Vector2 panDelta;

	private Vector3 prevMousePosition;

	private bool cursorBordered;

	public Transform PanTarget { get; private set; }

	public int PrMOrder { get; set; }

	protected override void Awake()
	{
		base.Awake();
		PanTarget = new GameObject("PC2DPanTarget").transform;
		base.ProCamera2D.AddPreMover(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)base.ProCamera2D)
		{
			base.ProCamera2D.RemovePreMover(this);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.ProCamera2D.AddCameraTarget(PanTarget);
		CenterPanTargetOnCamera();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ResetPrevPanPoint = true;
		base.ProCamera2D.RemoveCameraTarget(PanTarget);
	}

	private bool CanEdgePan()
	{
		if (!UsePanByMoveToEdges || !Application.isFocused)
		{
			return false;
		}
		switch (TheLastStand.Manager.InputManager.GetLastControllerType())
		{
		case ControllerType.Joystick:
		{
			bool flag = TheLastStand.Manager.InputManager.IsLastControllerJoystick && TPSingleton<CursorView>.Instance.JoystickCursorMoving;
			return panDelta == Vector2.zero && flag;
		}
		default:
			if (panDelta == Vector2.zero)
			{
				if (!IgnoreUIOnEdges && !TheLastStand.Manager.InputManager.IsPointerOverWorld && !TheLastStand.Manager.InputManager.IsPointerOverAllowingCursorUI)
				{
					return cursorBordered;
				}
				return true;
			}
			return false;
		}
	}

	private void Pan(float deltaTime)
	{
		panDelta = Vector2.zero;
		Vector2 vector = DragPanSpeedMultiplier;
		if (UsePanByDrag && TheLastStand.Manager.InputManager.GetButtonDown(27))
		{
			CenterPanTargetOnCamera(StopSpeedOnDragStart);
		}
		Vector3 vector2 = new Vector3(TheLastStand.Manager.InputManager.MousePosition.x, TheLastStand.Manager.InputManager.MousePosition.y, Mathf.Abs(Vector3D(base.ProCamera2D.LocalPosition)));
		bool button = TheLastStand.Manager.InputManager.GetButton(27);
		if (UsePanByDrag && button)
		{
			Vector2 normalizedInput = new Vector2(TheLastStand.Manager.InputManager.MousePosition.x / (float)base.ProCamera2D.GameCamera.pixelWidth, TheLastStand.Manager.InputManager.MousePosition.y / (float)base.ProCamera2D.GameCamera.pixelHeight);
			if (base.ProCamera2D.GameCamera.pixelRect.Contains(vector2) && InsideDraggableArea(normalizedInput))
			{
				Vector3 vector3 = base.ProCamera2D.GameCamera.ScreenToWorldPoint(prevMousePosition);
				if (ResetPrevPanPoint)
				{
					vector3 = base.ProCamera2D.GameCamera.ScreenToWorldPoint(vector2);
					ResetPrevPanPoint = false;
				}
				Vector3 vector4 = base.ProCamera2D.GameCamera.ScreenToWorldPoint(vector2);
				Vector3 arg = vector3 - vector4;
				panDelta = new Vector2(Vector3H(arg), Vector3V(arg));
			}
		}
		else if (!button)
		{
			bool flag = TPSingleton<HUDJoystickNavigationManager>.Exist() && TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn;
			if (UsePanByKeyboard && !flag)
			{
				Vector2 vector5 = new Vector2(TheLastStand.Manager.InputManager.GetAxis(17), TheLastStand.Manager.InputManager.GetAxis(18));
				panDelta = vector5 * deltaTime;
				if (panDelta != Vector2.zero)
				{
					vector = KeyboardPanSpeed;
				}
			}
			Vector3 vector6 = (TheLastStand.Manager.InputManager.IsLastControllerJoystick ? ACameraView.MainCam.WorldToScreenPoint(TheLastStand.Manager.InputManager.JoystickCursorPosition) : TheLastStand.Manager.InputManager.MousePosition);
			cursorBordered = false;
			if (TPSingleton<SettingsManager>.Instance.Settings.IsCursorRestricted && TPSingleton<SettingsManager>.Instance.Settings.WindowMode == SettingsManager.E_WindowMode.Windowed && Application.isFocused && (vector6.x < 0f || vector6.x >= (float)Screen.width || vector6.y < 0f || vector6.y > (float)Screen.height))
			{
				cursorBordered = true;
			}
			if (CanEdgePan())
			{
				float num = ((float)(-Screen.width) * 0.5f + vector6.x) / (float)Screen.width;
				float num2 = ((float)(-Screen.height) * 0.5f + vector6.y) / (float)Screen.height;
				if (num < 0f)
				{
					num = Utils.Remap(num, -0.5f, (0f - LeftPanEdge) * 0.5f, -0.5f, 0f);
				}
				else if (num > 0f)
				{
					num = Utils.Remap(num, RightPanEdge * 0.5f, 0.5f, 0f, 0.5f);
				}
				if (num2 < 0f)
				{
					num2 = Utils.Remap(num2, -0.5f, (0f - BottomPanEdge) * 0.5f, -0.5f, 0f);
				}
				else if (num2 > 0f)
				{
					num2 = Utils.Remap(num2, TopPanEdge * 0.5f, 0.5f, 0f, 0.5f);
				}
				panDelta = new Vector2(num, num2) * deltaTime;
				if (panDelta != Vector2.zero)
				{
					vector = EdgesPanSpeed;
				}
			}
		}
		prevMousePosition = vector2;
		if (panDelta != Vector2.zero)
		{
			Vector3 translation = VectorHV(panDelta.x * vector.x, panDelta.y * vector.y);
			PanTarget.Translate(translation);
			if (!IsPanning && OnPanStarted != null)
			{
				OnPanStarted();
			}
			IsPanning = true;
		}
		if ((base.ProCamera2D.IsCameraPositionLeftBounded && Vector3H(PanTarget.position) < Vector3H(base.ProCamera2D.LocalPosition)) || (base.ProCamera2D.IsCameraPositionRightBounded && Vector3H(PanTarget.position) > Vector3H(base.ProCamera2D.LocalPosition)))
		{
			PanTarget.position = VectorHVD(Vector3H(base.ProCamera2D.LocalPosition) - base.ProCamera2D.GetOffsetX() * 0.9999f, Vector3V(PanTarget.position), Vector3D(PanTarget.position));
		}
		if ((base.ProCamera2D.IsCameraPositionBottomBounded && Vector3V(PanTarget.position) < Vector3V(base.ProCamera2D.LocalPosition)) || (base.ProCamera2D.IsCameraPositionTopBounded && Vector3V(PanTarget.position) > Vector3V(base.ProCamera2D.LocalPosition)))
		{
			PanTarget.position = VectorHVD(Vector3H(PanTarget.position), Vector3V(base.ProCamera2D.LocalPosition) - base.ProCamera2D.GetOffsetY() * 0.9999f, Vector3D(PanTarget.position));
		}
	}

	public void CenterPanTargetOnCamera(float interpolant = 1f)
	{
		if (PanTarget != null)
		{
			PanTarget.position = Vector3.Lerp(PanTarget.position, VectorHV(Vector3H(base.ProCamera2D.LocalPosition) - base.ProCamera2D.GetOffsetX(), Vector3V(base.ProCamera2D.LocalPosition) - base.ProCamera2D.GetOffsetY()), interpolant);
		}
	}

	private bool InsideDraggableArea(Vector2 normalizedInput)
	{
		if (DraggableAreaRect.x == 0f && DraggableAreaRect.y == 0f && DraggableAreaRect.width == 1f && DraggableAreaRect.height == 1f)
		{
			return true;
		}
		if (normalizedInput.x > DraggableAreaRect.x + (1f - DraggableAreaRect.width) / 2f && normalizedInput.x < DraggableAreaRect.x + DraggableAreaRect.width + (1f - DraggableAreaRect.width) / 2f && normalizedInput.y > DraggableAreaRect.y + (1f - DraggableAreaRect.height) / 2f && normalizedInput.y < DraggableAreaRect.y + DraggableAreaRect.height + (1f - DraggableAreaRect.height) / 2f)
		{
			return true;
		}
		return false;
	}

	public void PreMove(float deltaTime)
	{
		if (base.enabled && AllowPan)
		{
			if (IsPanning && OnPanFinished != null)
			{
				OnPanFinished();
			}
			IsPanning = false;
			Pan(deltaTime);
		}
	}
}
