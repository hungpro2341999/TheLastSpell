using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Database;

public class MetaConditionsDatabase : Dictionary<string, Func<object[], MetaConditionsDatabase.ProgressionDatas>>
{
	private static class NumberFunctions
	{
		public static ProgressionDatas ALowerThanB(params object[] args)
		{
			double num = Convert.ToDouble(args[0]);
			double num2 = Convert.ToDouble(args[1]);
			return new ProgressionDatasDouble(num < num2, num, num2);
		}

		public static ProgressionDatas ALowerThanOrEqualToB(params object[] args)
		{
			double num = Convert.ToDouble(args[0]);
			double num2 = Convert.ToDouble(args[1]);
			return new ProgressionDatasDouble(num <= num2, num, num2);
		}

		public static ProgressionDatas AGreaterThanB(params object[] args)
		{
			double num = Convert.ToDouble(args[0]);
			double num2 = Convert.ToDouble(args[1]);
			return new ProgressionDatasDouble(num > num2, num, num2);
		}

		public static ProgressionDatas AGreaterThanOrEqualToB(params object[] args)
		{
			double num = Convert.ToDouble(args[0]);
			double num2 = Convert.ToDouble(args[1]);
			return new ProgressionDatasDouble(num >= num2, num, num2);
		}

		public static ProgressionDatas AEqualToB(params object[] args)
		{
			try
			{
				double num = Convert.ToDouble(args[0]);
				double num2 = Convert.ToDouble(args[1]);
				return new ProgressionDatasDouble(num == num2, num, num2);
			}
			catch (Exception ex)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError($"Error while parsing arguments: {args[0]} to double, {args[1]} to double.\n{ex.Message}");
				return null;
			}
		}
	}

	private static class StringListsFunctions
	{
		public static ProgressionDatas ListAContainsB(params object[] args)
		{
			return ListAContainsAtLeastBAmountOfC(args[0], 1, args[1]);
		}

		public static ProgressionDatas ListAContainsBAmountOfC(params object[] args)
		{
			List<string> list = args[0] as List<string>;
			return new ProgressionDatasListString((double)list.FindAll((string o) => o == args[2].ToString()).Count == Convert.ToDouble(args[1]), list.FindAll((string o) => o == args[2].ToString()), Convert.ToDouble(args[1]));
		}

		public static ProgressionDatas ListAContainsAtLeastBAmountOfC(params object[] args)
		{
			List<string> list = args[0] as List<string>;
			return new ProgressionDatasListString((double)list.FindAll((string o) => o == args[2].ToString()).Count >= Convert.ToDouble(args[1]), list.FindAll((string o) => o == args[2].ToString()), Convert.ToDouble(args[1]));
		}
	}

	private static class StringIntDictionariesFunctions
	{
		public static ProgressionDatas DictAContainsB(params object[] args)
		{
			return DictAContainsAtLeastBAmountOfC(args[0], 1, args[1]);
		}

		public static ProgressionDatas DictAContainsBAmountOfC(params object[] args)
		{
			Dictionary<string, int> dictionary = args[0] as Dictionary<string, int>;
			int value;
			return new ProgressionDatasDictionaryStringInt(dictionary.TryGetValue(args[2].ToString(), out value) && value == Convert.ToInt32(args[1]), dictionary.ContainsKey(args[2].ToString()) ? dictionary[args[2].ToString()] : 0, Convert.ToDouble(args[1]));
		}

		public static ProgressionDatas DictAContainsAtLeastBAmountOfC(params object[] args)
		{
			Dictionary<string, int> dictionary = args[0] as Dictionary<string, int>;
			int value;
			return new ProgressionDatasDictionaryStringInt(dictionary.TryGetValue(args[2].ToString(), out value) && value >= Convert.ToInt32(args[1]), dictionary.ContainsKey(args[2].ToString()) ? dictionary[args[2].ToString()] : 0, Convert.ToDouble(args[1]));
		}
	}

	public abstract class ProgressionDatas
	{
		public object GoalValue { get; private set; }

		public bool IsComplete { get; private set; }

		public object ProgressionValue { get; private set; }

		public ProgressionDatas(bool isComplete, object progressionValue, object goalValue)
		{
			IsComplete = isComplete;
			ProgressionValue = progressionValue;
			GoalValue = goalValue;
		}

		public abstract string GoalValueToString();

		public abstract string ProgressionValueToString();
	}

	public class ProgressionDatasDouble : ProgressionDatas
	{
		public ProgressionDatasDouble(bool isComplete, object progressionValue, object goalValue)
			: base(isComplete, progressionValue, goalValue)
		{
		}

		public override string GoalValueToString()
		{
			return Convert.ToDouble(base.GoalValue).ToString();
		}

		public override string ProgressionValueToString()
		{
			return Convert.ToDouble(base.ProgressionValue).ToString();
		}
	}

	public class ProgressionDatasListString : ProgressionDatas
	{
		public ProgressionDatasListString(bool isComplete, object progressionValue, object goalValue)
			: base(isComplete, progressionValue, goalValue)
		{
		}

		public override string GoalValueToString()
		{
			return Convert.ToDouble(base.GoalValue).ToString();
		}

		public override string ProgressionValueToString()
		{
			return (base.ProgressionValue as List<string>).Count.ToString();
		}
	}

	public class ProgressionDatasDictionaryStringInt : ProgressionDatas
	{
		public ProgressionDatasDictionaryStringInt(bool isComplete, object progressionValue, object goalValue)
			: base(isComplete, progressionValue, goalValue)
		{
		}

		public override string GoalValueToString()
		{
			return Convert.ToInt32(base.GoalValue).ToString();
		}

		public override string ProgressionValueToString()
		{
			return Convert.ToInt32(base.ProgressionValue).ToString();
		}
	}

	public MetaConditionsDatabase()
	{
		Add("NumberALowerThanB", NumberFunctions.ALowerThanB);
		Add("NumberALowerThanOrEqualToB", NumberFunctions.ALowerThanOrEqualToB);
		Add("NumberAGreaterThanB", NumberFunctions.AGreaterThanB);
		Add("NumberAGreaterThanOrEqualToB", NumberFunctions.AGreaterThanOrEqualToB);
		Add("NumberAEqualToB", NumberFunctions.AEqualToB);
		Add("ListAContainsB", StringListsFunctions.ListAContainsB);
		Add("ListAContainsBAmountOfC", StringListsFunctions.ListAContainsBAmountOfC);
		Add("ListAContainsAtLeastBAmountOfC", StringListsFunctions.ListAContainsAtLeastBAmountOfC);
		Add("DictAContainsB", StringIntDictionariesFunctions.DictAContainsB);
		Add("DictAContainsBAmountOfC", StringIntDictionariesFunctions.DictAContainsBAmountOfC);
		Add("DictAContainsAtLeastBAmountOfC", StringIntDictionariesFunctions.DictAContainsAtLeastBAmountOfC);
	}
}
