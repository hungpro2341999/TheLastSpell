using DG.Tweening;
using UnityEngine;

namespace TheLastStand.Model;

[CreateAssetMenu(fileName = "New Joystick Cursor Config", menuName = "TLS/Joystick Config/Cursor")]
public class JoystickCursorConfig : ScriptableObject
{
	[SerializeField]
	[Tooltip("Allow the player to keep the camera focus button pressed to make the camera follow the cursor without having to click multiple times.")]
	private bool canHoldCameraFocusDown;

	[SerializeField]
	[Tooltip("Clamps the cursor position to screen bounds (should be set to true although the option is exposed here in case we need to do some testing).")]
	private bool clampToScreen = true;

	[SerializeField]
	[Tooltip("Uses Camera UI mask to consider cursor as hidden or not. If not, only the screen bounds will be used.")]
	private bool useCameraUIMask = true;

	[SerializeField]
	[Min(0.1f)]
	[Tooltip("Normalize the joystick input when applying free motion speed")]
	private bool normalizeInput = true;

	[SerializeField]
	[Range(0.1f, 1f)]
	[Tooltip("Multiplier applied to free speed when camera is zoomed in, to have a more precise control.")]
	private float freeSpeedZoomedInMultiplier = 1f;

	[Header("Slow Speed")]
	[SerializeField]
	[Min(0.1f)]
	[Tooltip("Cursor speed when moving freely with low joystick inclination.")]
	private float slowSpeed = 10f;

	[Header("Fast Speed")]
	[SerializeField]
	[Min(0.1f)]
	[Tooltip("Cursor speed range when moving freely with high joystick inclination.\nx: initial speed.\ny: is the speed reached when holding the joystick inclination high.")]
	private Vector2 fastSpeedMinMax = new Vector2(15f, 20f);

	[SerializeField]
	[Min(0f)]
	[Tooltip("Duration of the full acceleration of fast range when holding a high joystick inclination.")]
	private float fastSpeedTransitionDuration = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Minimum joystick inclination percentage to start moving fast.\nMust be higher than or equal to dead zone.")]
	private float fastSpeedStartInclination = 0.8f;

	[SerializeField]
	[Range(1f, 60f)]
	[Tooltip("Cursor speed when moving across tilemap (= tiles per second).")]
	private int tilemapSpeed = 15;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Time to wait before snapping the cursor to the currently hovered tile, when cursor is not moving.")]
	private float tileSnapDelay = 0.5f;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Duration of the snap to the currently hovered tile.")]
	private float tileSnapDuration = 0.3f;

	[SerializeField]
	[Tooltip("Easing curve of the snap to the currently hovered tile.")]
	private Ease tileSnapEasing = Ease.OutQuint;

	[Header("Slow Speed")]
	[SerializeField]
	[Min(0.1f)]
	[Tooltip("Cursor speed when moving freely with low joystick inclination.")]
	private float oraculumSlowSpeed = 10f;

	[Header("Fast Speed")]
	[SerializeField]
	[Min(0.1f)]
	[Tooltip("Cursor speed range when moving freely with high joystick inclination.\nx: initial speed.\ny: is the speed reached when holding the joystick inclination high.")]
	private Vector2 oraculumFastSpeedMinMax = new Vector2(15f, 20f);

	[SerializeField]
	[Min(0f)]
	[Tooltip("Duration of the full acceleration of fast range when holding a high joystick inclination.")]
	private float oraculumFastSpeedTransitionDuration = 1f;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Time to wait before hiding the cursor when it is not moving.")]
	private float hideDelay = 1.3f;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Time the cursors takes to fade out.")]
	private float hideDuration = 0.3f;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Scale tween duration when zooming in/out.")]
	private float scaleDuration = 0.25f;

	private bool IsHideDelayInvalid => HideDelay < TileSnapDelay + TileSnapDuration;

	public bool CanHoldCameraFocusDown => canHoldCameraFocusDown;

	public float SlowSpeed => slowSpeed;

	public Vector2 FastSpeedMinMax => fastSpeedMinMax;

	public float FastSpeedTransitionDuration => fastSpeedTransitionDuration;

	public float FastSpeedStartInclination => fastSpeedStartInclination;

	public float FreeSpeedZoomedInMultiplier => freeSpeedZoomedInMultiplier;

	public int TilemapSpeed => tilemapSpeed;

	public float TileSnapDelay => tileSnapDelay;

	public float TileSnapDuration => tileSnapDuration;

	public Ease TileSnapEasing => tileSnapEasing;

	public bool ClampToScreen => clampToScreen;

	public bool UseCameraUIMask => useCameraUIMask;

	public bool NormalizeInput => normalizeInput;

	public float HideDelay => hideDelay;

	public float HideDuration => hideDuration;

	public float ScaleDuration => scaleDuration;

	public float OraculumSlowSpeed => oraculumSlowSpeed;

	public Vector2 OraculumFastSpeedMinMax => oraculumFastSpeedMinMax;

	public float OraculumFastSpeedTransitionDuration => oraculumFastSpeedTransitionDuration;
}
