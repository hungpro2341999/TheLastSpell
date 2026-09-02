using TMPro;
using UnityEngine;

namespace TheLastStand.View.Shop;

public class ShopCategoryButtonView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI categoryNameText;

	public string Category { get; set; }

	public void Init(string category)
	{
		Category = category;
		categoryNameText.text = category;
	}

	public void ChangeButtonDisplay(string selectedCategory)
	{
		categoryNameText.color = ((selectedCategory == Category) ? Color.white : Color.grey);
	}
}
