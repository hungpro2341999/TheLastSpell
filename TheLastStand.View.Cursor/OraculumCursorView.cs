using DG.Tweening;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Cursor;

public class OraculumCursorView : MonoBehaviour
{
	private Tween hideTween;

	private float joystickFastSpeedTimer;

	public bool Enabled => base.gameObject.activeSelf;

	public void Enable(bool isOn)
	{
		if (isOn != Enabled)
		{
			Vector3 position = ACameraView.MainCam.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
			SetPosition(position);
			base.gameObject.SetActive(InputManager.IsLastControllerJoystick && isOn);
		}
	}

	public void SetPosition(Vector3 position)
	{
		base.transform.position = position;
	}

	private void Update()
	{
		float axis = InputManager.GetAxis(77);
		float axis2 = InputManager.GetAxis(78);
		Vector3 vector = new Vector3(axis, axis2);
		float magnitude = vector.magnitude;
		if (magnitude < InputManager.JoystickConfig.DefaultDeadZone)
		{
			vector = Vector3.zero;
		}
		float num;
		if (magnitude < InputManager.JoystickConfig.Cursor.FastSpeedStartInclination)
		{
			num = InputManager.JoystickConfig.Cursor.OraculumSlowSpeed;
			joystickFastSpeedTimer = 0f;
		}
		else
		{
			num = Mathf.Lerp(InputManager.JoystickConfig.Cursor.OraculumFastSpeedMinMax.x, InputManager.JoystickConfig.Cursor.OraculumFastSpeedMinMax.y, joystickFastSpeedTimer);
			if (InputManager.JoystickConfig.Cursor.FastSpeedTransitionDuration == 0f)
			{
				joystickFastSpeedTimer = 1f;
			}
			else
			{
				joystickFastSpeedTimer += Time.deltaTime / InputManager.JoystickConfig.Cursor.OraculumFastSpeedTransitionDuration;
			}
		}
		Vector3 position = base.transform.position + (InputManager.JoystickConfig.Cursor.NormalizeInput ? vector.normalized : vector) * num * Time.deltaTime;
		SetPosition(ClampToScreen(position));
	}

	private static Vector3 ClampToScreen(Vector3 position)
	{
		if (!InputManager.JoystickConfig.Cursor.ClampToScreen)
		{
			return position;
		}
		Vector3 vector = ACameraView.MainCam.ViewportToScreenPoint(Vector2.zero);
		Vector3 vector2 = ACameraView.MainCam.ViewportToScreenPoint(Vector2.one);
		position.x = Mathf.Clamp(position.x, vector.x, vector2.x);
		position.y = Mathf.Clamp(position.y, vector.y, vector2.y);
		return position;
	}
}
