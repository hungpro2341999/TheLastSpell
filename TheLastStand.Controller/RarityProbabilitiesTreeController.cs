using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Controller;

/// <summary>
/// Bộ điều khiển xác suất độ hiếm của vật phẩm (Rarity Probabilities Tree):
/// Quản lý thuật toán RNG tính toán độ hiếm (Common, Uncommon, Rare, Epic...)
/// kết hợp với các bổ trợ nâng cao từ Glyph và Meta Upgrade.
/// </summary>
public class RarityProbabilitiesTreeController
{
	#region Rarity Generation Logic

	/// <summary>
	/// Tính toán độ hiếm ngẫu nhiên dựa trên định nghĩa cây xác suất.
	/// </summary>
	/// <param name="probabilityTreeEntriesDefinition">Định nghĩa cây xác suất độ hiếm.</param>
	/// <param name="minRarityIndex">Chỉ số độ hiếm tối thiểu cho phép.</param>
	/// <param name="maxRarityIndex">Chỉ số độ hiếm tối đa cho phép.</param>
	/// <returns>Độ hiếm của vật phẩm được chọn.</returns>
	public static ItemDefinition.E_Rarity GenerateRarity(ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition, int minRarityIndex = -1, int maxRarityIndex = -1)
	{
		return GenerateRarity(ComputeRarityProbabilities(probabilityTreeEntriesDefinition), minRarityIndex, maxRarityIndex);
	}

	/// <summary>
	/// Quay số ngẫu nhiên độ hiếm dựa trên từ điển xác suất cụ thể:
	/// Roll từ mốc độ hiếm cao nhất xuống thấp nhất.
	/// </summary>
	/// <param name="probabilities">Từ điển xác suất theo từng bậc độ hiếm.</param>
	/// <param name="minRarityIndex">Chỉ số độ hiếm tối thiểu.</param>
	/// <param name="maxRarityIndex">Chỉ số độ hiếm tối đa.</param>
	/// <returns>Độ hiếm kết quả.</returns>
	public static ItemDefinition.E_Rarity GenerateRarity(Dictionary<int, int> probabilities, int minRarityIndex = -1, int maxRarityIndex = -1)
	{
		int num = ((maxRarityIndex != -1) ? maxRarityIndex : 4);
		int num2 = ((minRarityIndex == -1) ? 1 : minRarityIndex);
		
		// Duyệt từ độ hiếm cao nhất về độ hiếm thấp nhất
		for (int num3 = num; num3 >= num2; num3--)
		{
			if (probabilities.ContainsKey(num3) && RandomManager.GetRandomRange("RarityProbabilitiesTreeController", 0, 100) < probabilities[num3])
			{
				return (ItemDefinition.E_Rarity)num3;
			}
		}
		return (ItemDefinition.E_Rarity)num2;
	}

	#endregion

	#region Rarity Probability Computation

	/// <summary>
	/// Tính toán bảng xác suất độ hiếm đầy đủ sau khi đã cộng gộp bổ trợ từ Glyph và Meta Upgrade.
	/// </summary>
	/// <param name="probabilityTreeEntriesDefinition">Định nghĩa cây xác suất gốc.</param>
	/// <returns>Từ điển xác suất độ hiếm sau điều chỉnh.</returns>
	public static Dictionary<int, int> ComputeRarityProbabilities(ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels);
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		
		// 1. Áp dụng bổ trợ từ Glyph
		if (TPSingleton<GlyphManager>.Instance.RarityProbabilityTreeModifiers.TryGetValue(probabilityTreeEntriesDefinition.Id, out var value))
		{
			dictionary = dictionary.Add(value);
		}
		
		// 2. Áp dụng bổ trợ từ Meta Upgrades đang kích hoạt
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<ItemRaritiesMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num = effects.Length - 1; num >= 0; num--)
			{
				ItemRaritiesMetaEffectDefinition itemRaritiesMetaEffectDefinition = effects[num];
				if (itemRaritiesMetaEffectDefinition.RarityTreeId == probabilityTreeEntriesDefinition.Id)
				{
					foreach (KeyValuePair<int, int> item in itemRaritiesMetaEffectDefinition.WeightBonusByRarityLevel)
					{
						if (dictionary2.ContainsKey(item.Key))
						{
							dictionary2[item.Key] += item.Value;
						}
						else
						{
							dictionary2.Add(item.Key, item.Value);
						}
					}
				}
			}
		}
		return dictionary.Add(dictionary2);
	}

	#endregion

	#region Item Tag & Minimum Rarity Helpers

	/// <summary>
	/// Lấy chỉ số độ hiếm tối thiểu cho một vật phẩm dựa trên các Tag của nó và cài đặt Glyph hiện tại.
	/// </summary>
	/// <param name="itemDefinition">Định nghĩa của vật phẩm.</param>
	/// <returns>Chỉ số độ hiếm tối thiểu, hoặc -1 nếu không có ràng buộc.</returns>
	public static int GetMinRarityIndexFromItemDefinition(ItemDefinition itemDefinition)
	{
		int num = -1;
		if (!TPSingleton<GlyphManager>.Exist() || itemDefinition == null)
		{
			return num;
		}
		foreach (string tag in itemDefinition.Tags)
		{
			if (TPSingleton<GlyphManager>.Instance.ItemByTagRarityModifier.TryGetValue(tag, out var value) && num < value.MinRarityIndex)
			{
				num = value.MinRarityIndex;
			}
		}
		return num;
	}

	#endregion
}
