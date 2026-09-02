using UnityEngine;

namespace TheLastStand.View;

public interface ITileObjectView
{
	GameObject GameObject { get; }

	bool Hovered { get; set; }

	bool Selected { get; set; }

	bool HoveredOrSelected { get; }

	void ToggleSkillTargeting(bool display);

	void RefreshCursorFeedback();
}
