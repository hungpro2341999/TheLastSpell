namespace TheLastStand.View.HUD;

public interface IJoystickSkillConfirmHandler : IJoystickSelect
{
	bool IsConfirmSelected { get; set; }

	void SelectConfirmButton();

	void DeselectConfirmButton(bool deselectAll);
}
