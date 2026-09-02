using System.Collections.Generic;
using System.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.LevelEditor;
using UnityEngine;

namespace TheLastStand.View.LevelEditor;

public class SelectGroundPanel : MonoBehaviour
{
	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnGroundButtonClick(string groundDefinitionId)
	{
		LevelEditorManager.SelectGround(groundDefinitionId);
	}

	private void Awake()
	{
		TransformExtensions.DestroyChildren(base.transform);
		List<string> list = TileDatabase.GroundDefinitions.Keys.ToList();
		list.Sort();
		foreach (string groundId in list)
		{
			Object.Instantiate(LevelEditorManager.LevelEditorButtonPrefab, base.gameObject.transform).Init(groundId, delegate
			{
				OnGroundButtonClick(groundId);
			});
		}
		Object.Instantiate(LevelEditorManager.LevelEditorButtonPrefab, base.gameObject.transform).Init("BACK (Esc)", OnBackButtonClick);
	}
}
