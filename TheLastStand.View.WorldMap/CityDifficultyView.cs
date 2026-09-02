using TheLastStand.Definition.WorldMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class CityDifficultyView : MonoBehaviour
{
	[SerializeField]
	private Image[] difficultySkulls;

	public void RefreshDifficultySkulls(CityDefinition cityDefinition)
	{
		for (int i = 0; i < difficultySkulls.Length; i++)
		{
			difficultySkulls[i].gameObject.SetActive(value: false);
			if (i < cityDefinition.DifficultySkullsNb)
			{
				difficultySkulls[i].gameObject.SetActive(value: true);
			}
		}
	}
}
