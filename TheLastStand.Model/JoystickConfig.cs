using TheLastStand.View.HUD;
using UnityEngine;

namespace TheLastStand.Model;

[CreateAssetMenu(fileName = "New Joystick Config", menuName = "TLS/Joystick Config/Master Config")]
public class JoystickConfig : ScriptableObject
{
	[SerializeField]
	[Range(0f, 0.9f)]
	private float defaultDeadZone = 0.4f;

	[SerializeField]
	private ControllerTypeGuidTable controllerTypeGuidTable;

	[SerializeField]
	private GamepadButtonsSetsTable gamepadButtonsSetsTable;

	[SerializeField]
	private bool buildEnabled;

	[SerializeField]
	private JoystickCursorConfig cursor;

	[SerializeField]
	private JoystickHUDNavigationConfig hudNavigation;

	public float DefaultDeadZone => defaultDeadZone;

	public ControllerTypeGuidTable ControllerTypeGuidTable => controllerTypeGuidTable;

	public GamepadButtonsSetsTable GamepadButtonsSetsTable => gamepadButtonsSetsTable;

	public bool BuildEnabled => buildEnabled;

	public JoystickCursorConfig Cursor => cursor;

	public JoystickHUDNavigationConfig HUDNavigation => hudNavigation;
}
