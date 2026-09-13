using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Controller;

/// <summary>
/// Bộ điều khiển cây xác suất cấp độ (Level Probabilities Tree):
/// Quản lý thuật toán RNG tính toán cấp độ của vật phẩm hoặc trang bị rớt ra,
/// có tính đến các bổ trợ từ Glyph (Ký tự cổ) và nâng cấp Meta (Oraculum Meta Upgrades).
/// </summary>
public sealed class LevelProbabilitiesTreeController
{
	#region Fields & Properties

	/// <summary>Mức điều chỉnh cấp độ cao nhất trong cây xác suất.</summary>
	private int highestModifierLevel;

	/// <summary>Mức điều chỉnh cấp độ thấp nhất trong cây xác suất.</summary>
	private int lowestModifierLevel;

	/// <summary>Từ điển trọng số xác suất theo từng mốc cấp độ.</summary>
	private Dictionary<int, int> modifiersByLevel;

	/// <summary>Định nghĩa cây xác suất nạp từ dữ liệu game.</summary>
	private ProbabilityTreeEntriesDefinition probabilityTreeDefinition;

	/// <summary>Thực thể sở hữu cấp độ gốc (ví dụ: ngày chơi hiện tại, loại rương...).</summary>
	public ILevelOwner LevelOwner { get; private set; }

	/// <summary>Cấp độ cơ sở nếu không có LevelOwner.</summary>
	public int Level { get; private set; } = -1;

	#endregion

	#region Constructors

	/// <summary>Khởi tạo cây xác suất với cấp độ cố định và từ điển modifier.</summary>
	public LevelProbabilitiesTreeController(int level, Dictionary<int, int> modifiersByLevel)
	{
		Level = level;
		this.modifiersByLevel = ((modifiersByLevel != null && modifiersByLevel.Count > 0) ? new Dictionary<int, int>(modifiersByLevel) : null);
		BoundariesModifierLevel();
	}

	/// <summary>Khởi tạo cây xác suất với cấp độ cố định và định nghĩa cây xác suất.</summary>
	public LevelProbabilitiesTreeController(int level, ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		Level = level;
		modifiersByLevel = ((probabilityTreeEntriesDefinition.ProbabilityLevels != null && probabilityTreeEntriesDefinition.ProbabilityLevels.Count > 0) ? new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels) : null);
		probabilityTreeDefinition = probabilityTreeEntriesDefinition;
		BoundariesModifierLevel();
	}

	/// <summary>Khởi tạo cây xác suất với LevelOwner và định nghĩa cây xác suất.</summary>
	public LevelProbabilitiesTreeController(ILevelOwner levelOwner, ProbabilityTreeEntriesDefinition probabilityTreeEntriesDefinition)
	{
		LevelOwner = levelOwner;
		modifiersByLevel = ((probabilityTreeEntriesDefinition.ProbabilityLevels != null && probabilityTreeEntriesDefinition.ProbabilityLevels.Count > 0) ? new Dictionary<int, int>(probabilityTreeEntriesDefinition.ProbabilityLevels) : null);
		probabilityTreeDefinition = probabilityTreeEntriesDefinition;
		BoundariesModifierLevel();
	}

	/// <summary>Khởi tạo cây xác suất với LevelOwner và từ điển modifier.</summary>
	public LevelProbabilitiesTreeController(ILevelOwner levelOwner, Dictionary<int, int> modifiersByLevel)
	{
		LevelOwner = levelOwner;
		this.modifiersByLevel = ((modifiersByLevel != null && modifiersByLevel.Count > 0) ? new Dictionary<int, int>(modifiersByLevel) : null);
		BoundariesModifierLevel();
	}

	#endregion

	#region Level Generation Logic

	/// <summary>
	/// Quay số ngẫu nhiên (RNG) để sinh cấp độ cuối cùng của vật phẩm:
	/// Kết hợp cây xác suất gốc với các bonus từ Glyph và Meta Upgrade, sau đó roll theo thứ tự từ cấp cao nhất xuống thấp nhất.
	/// </summary>
	/// <returns>Cấp độ được tạo ra (tối thiểu là 0).</returns>
	public int GenerateLevel()
	{
		if (modifiersByLevel == null)
		{
			return LevelOwner?.Level ?? Level;
		}
		
		// Tính toán cây xác suất sau khi cộng dồn Glyph và Meta Upgrades
		Dictionary<int, int> dictionary = ComputeProbablyTree();
		if (dictionary != null)
		{
			foreach (int key in dictionary.Keys)
			{
				if (key > highestModifierLevel)
				{
					highestModifierLevel = key;
				}
				else if (key < lowestModifierLevel)
				{
					lowestModifierLevel = key;
				}
			}
		}
		
		// Duyệt từ cấp cao nhất xuống thấp nhất để thử vận may
		for (int num = highestModifierLevel; num >= lowestModifierLevel; num--)
		{
			if (RandomManager.GetRandomRange(this, 0, 100) < dictionary[num])
			{
				int num2 = num + ((LevelOwner != null) ? LevelOwner.Level : Level);
				TPSingleton<RandomManager>.Instance.Log($"[{((LevelOwner != null) ? LevelOwner.Name : string.Empty)}] Generated level {num2}.", CLogLevel.DETAILED);
				return Mathf.Max(0, num2);
			}
		}
		
		TPSingleton<RandomManager>.Instance.Log("[" + ((LevelOwner != null) ? LevelOwner.Name : string.Empty) + "] Generated level 0.", CLogLevel.DETAILED);
		return 0;
	}

	#endregion

	#region Probability Calculations & Helpers

	/// <summary>
	/// Xác định giới hạn mốc cấp độ cao nhất và thấp nhất trong từ điển xác suất.
	/// </summary>
	private void BoundariesModifierLevel()
	{
		if (modifiersByLevel == null)
		{
			return;
		}
		foreach (int key in modifiersByLevel.Keys)
		{
			if (key > highestModifierLevel)
			{
				highestModifierLevel = key;
			}
			else if (key < lowestModifierLevel)
			{
				lowestModifierLevel = key;
			}
		}
	}

	/// <summary>
	/// Tính toán cây xác suất hoàn chỉnh:
	/// Cộng dồn xác suất gốc từ Definition với các hiệu ứng tăng cấp độ từ Glyph và Meta Upgrade.
	/// </summary>
	private Dictionary<int, int> ComputeProbablyTree()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(probabilityTreeDefinition.ProbabilityLevels);
		
		// 1. Áp dụng bổ trợ từ Glyph cho trang bị khởi đầu
		Dictionary<int, int> dictionary2 = TPSingleton<GlyphManager>.Instance.StartingGearLevelModifiers.GetValueOrDefault(probabilityTreeDefinition.Id) ?? new Dictionary<int, int>();
		if (TPSingleton<GlyphManager>.Instance.LevelProbabilityTreeModifiers.TryGetValue(probabilityTreeDefinition.Id, out var value))
		{
			dictionary = dictionary.Add(value);
		}
		
		// 2. Áp dụng bổ trợ từ Meta Upgrades đang kích hoạt
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<ItemLevelProbabilityMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated) && effects.TryFind((ItemLevelProbabilityMetaEffectDefinition x) => x.LevelTreeId == probabilityTreeDefinition.Id, out var value2))
		{
			foreach (KeyValuePair<int, int> item in value2.WeightBonusByLevelProbability)
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
		return dictionary.Add(dictionary2);
	}

	#endregion

	#region Debug & Logging

	/// <summary>
	/// Ghi log chi tiết nội dung cây xác suất ra hệ thống log.
	/// </summary>
	public void Log()
	{
		string text = $"<b>#--- PROBAS TREE (Level {LevelOwner.Level}) ---#</b>\n";
		foreach (KeyValuePair<int, int> item in modifiersByLevel)
		{
			text += $"{item.Key} / {item.Value}\n";
		}
		TPSingleton<RandomManager>.Instance.Log("#Probabilities Tree.#" + text);
	}

	#endregion
}
