using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TestTMPIconScaler : MonoBehaviour
{
	public float ExtraScale = 1f;

	private void Update()
	{
		float fontSize = GetComponent<TextMeshProUGUI>().fontSize;
		float num = 16f / fontSize * 100f;
		num *= ExtraScale;
		GetComponent<TextMeshProUGUI>().text = $"Fitting <size={num}%><sprite name=\"spritesheet_1\"/></size> " + $"test <size={num}%><sprite name=\"spritesheet_3\"/> </size><style=\"GoodNbOutlined\">+5</style> Move points";
	}
}
