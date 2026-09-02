using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Collections;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaConditionController
{
	private readonly IList<Type> supportedContextTypes = new List<Type>
	{
		typeof(double),
		typeof(List<string>),
		typeof(Dictionary<string, int>),
		typeof(StringIntDictionary)
	};

	private readonly string[] keywordPotentialLocalizationPrefixes = new string[12]
	{
		"BuildingName_", "BuildingActionName_", "BuildingUpgradeTooltipName_", "DLC_Name_", "EnemyName_", "ItemName_", "PerkName_", "SkillName_", "SkillEffectName_", "UnitStat_Name_",
		"WorldMap_CityName_", "LifetimeStats_"
	};

	private Dictionary<string, MetaConditionContext> contexts = new Dictionary<string, MetaConditionContext>();

	public MetaCondition MetaCondition { get; protected set; }

	public MetaConditionController(MetaCondition condition, MetaConditionSpecificContext runContext, MetaConditionSpecificContext campaignContext, MetaConditionGlobalContext globalContext)
	{
		contexts.Add("ENVIRONMENT", globalContext);
		contexts.Add("CAMPAIGN", campaignContext);
		contexts.Add("RUN", runContext);
		contexts.Add("LOCAL", condition.LocalContext);
		condition.MetaConditionController = this;
		MetaCondition = condition;
	}

	public string GetLocalizedDescription()
	{
		_ = "Localizing " + MetaCondition.Id + ":\n";
		MetaConditionsDatabase.ProgressionDatas progressionValues = GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary);
		string text = ((!string.IsNullOrEmpty(MetaCondition.MetaConditionDefinition.LocalizationKey)) ? MetaCondition.MetaConditionDefinition.LocalizationKey : ParseLocalizationKey());
		if (string.IsNullOrEmpty(text))
		{
			TPSingleton<MetaConditionManager>.Instance.LogWarning("Computed localization key for MetaCondition " + MetaCondition.Id + " is null or empty.", CLogLevel.MAJOR);
			return MetaCondition.Id;
		}
		List<string> list = ParseLocalizationArguments();
		list.Add(IsComplete() ? progressionValues.GoalValueToString() : progressionValues.ProgressionValueToString());
		object[] parameters = list.ToArray();
		string text2 = Localizer.Format(text, parameters);
		if (MetaCondition.MetaConditionDefinition.Occurences > 1)
		{
			text2 = text2 + " " + Localizer.Format("MetaCondition_Occurences", MetaCondition.MetaConditionDefinition.Occurences, MetaCondition.OccurenceProgression);
		}
		string text3 = MetaCondition.MetaConditionDefinition.Arguments.Find((string o) => contexts.ContainsKey(o.Split(':')[0])).Split(':')[0];
		if (Localizer.TryGet("MetaCondition_ContextInfo_" + text3, out var value))
		{
			text2 = text2 + " " + value;
		}
		return text2;
	}

	public string LogLocalizedDescriptionBreakdown()
	{
		string text = "Localizing " + MetaCondition.Id + ":\n";
		text += "<b>Definition arguments:</b>\n";
		for (int i = 0; i < MetaCondition.MetaConditionDefinition.Arguments.Count; i++)
		{
			text = text + "- " + MetaCondition.MetaConditionDefinition.Arguments[i] + "\n";
		}
		MetaConditionsDatabase.ProgressionDatas progressionValues = GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary);
		string text2 = ((!string.IsNullOrEmpty(MetaCondition.MetaConditionDefinition.LocalizationKey)) ? MetaCondition.MetaConditionDefinition.LocalizationKey : ParseLocalizationKey());
		if (string.IsNullOrEmpty(text2))
		{
			TPSingleton<MetaConditionManager>.Instance.LogWarning("Computed localization key for MetaCondition " + MetaCondition.Id + " is null or empty.", CLogLevel.MAJOR);
			return MetaCondition.Id;
		}
		List<string> list = ParseLocalizationArguments();
		list.Add(IsComplete() ? progressionValues.GoalValueToString() : progressionValues.ProgressionValueToString());
		text = text + "<b>Key:</b> " + text2 + "\n";
		text += "<b>Localization format arguments:</b>\n";
		for (int j = 0; j < list.Count; j++)
		{
			text = text + "- " + list[j] + "\n";
		}
		object[] parameters = list.ToArray();
		string text3 = Localizer.Format(text2, parameters);
		if (MetaCondition.MetaConditionDefinition.Occurences > 1)
		{
			text3 = text3 + " " + Localizer.Format("MetaCondition_Occurences", MetaCondition.MetaConditionDefinition.Occurences, MetaCondition.OccurenceProgression);
		}
		text = text + "<b>RESULT =></b> " + text3;
		TPSingleton<MetaConditionManager>.Instance.Log(text);
		return text3;
	}

	public MetaConditionsDatabase.ProgressionDatas GetProgressionValues(MetaConditionsDatabase conditionsLibrary)
	{
		if (conditionsLibrary.ContainsKey(MetaCondition.MetaConditionDefinition.Name))
		{
			List<object> list = new List<object>();
			foreach (string argument2 in MetaCondition.MetaConditionDefinition.Arguments)
			{
				if (ParseArgument(argument2, out var argument))
				{
					list.Add(argument);
					continue;
				}
				TPSingleton<MetaConditionManager>.Instance.LogError($"Aborted script execution of {MetaCondition.MetaConditionDefinition} due to an argument error.", CLogLevel.DETAILED);
				return null;
			}
			return conditionsLibrary[MetaCondition.MetaConditionDefinition.Name](list.ToArray());
		}
		return null;
	}

	public bool IsComplete()
	{
		return MetaCondition.OccurenceProgression >= MetaCondition.MetaConditionDefinition.Occurences;
	}

	public void RefreshProgression(MetaConditionsDatabase conditionsLibrary)
	{
		if (IsComplete())
		{
			return;
		}
		if (conditionsLibrary.ContainsKey(MetaCondition.MetaConditionDefinition.Name))
		{
			List<object> list = new List<object>();
			foreach (string argument2 in MetaCondition.MetaConditionDefinition.Arguments)
			{
				if (ParseArgument(argument2, out var argument))
				{
					list.Add(argument);
					continue;
				}
				TPSingleton<MetaConditionManager>.Instance.LogError($"Aborted script execution of {MetaCondition.MetaConditionDefinition} due to an argument error.", CLogLevel.DETAILED);
				return;
			}
			try
			{
				if (conditionsLibrary[MetaCondition.MetaConditionDefinition.Name](list.ToArray()).IsComplete)
				{
					MetaCondition.OccurenceProgression++;
					TPSingleton<MetaConditionManager>.Instance.Log($"Conditions for meta condition {MetaCondition.MetaUpgradeController} are all fulfilled, bumping occurence progression to {MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences}");
					if (MetaCondition.OccurenceProgression < MetaCondition.MetaConditionDefinition.Occurences)
					{
						TPSingleton<MetaConditionManager>.Instance.Log($"Renewing own self-context for {MetaCondition.MetaUpgradeController} ({MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences}) (occurence was bumped)", CLogLevel.DETAILED);
						RenewLocalContext();
					}
					else
					{
						TPSingleton<MetaConditionManager>.Instance.Log($"Max number of occurences reached for meta condition {MetaCondition.MetaUpgradeController} ({MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences})!\\nThis condition is now entirely fulfilled");
					}
				}
				return;
			}
			catch (FormatException arg)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError($"Script error in {MetaCondition.ToString()}: Wrong data types supplied (Expected Number, received otherwise)\n{arg}", CLogLevel.DETAILED);
				return;
			}
			catch (InvalidCastException arg2)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError($"Script error in {MetaCondition.ToString()}: Wrong data types supplied\n{arg2}", CLogLevel.DETAILED);
				return;
			}
			catch (NullReferenceException arg3)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError($"Script error in {MetaCondition.ToString()}: Wrong data types supplied (Expected List, received otherwise)\n{arg3}", CLogLevel.DETAILED);
				return;
			}
			catch (IndexOutOfRangeException arg4)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError($"Script error in {MetaCondition.ToString()}: Too few arguments supplied\n{arg4}", CLogLevel.DETAILED);
				return;
			}
		}
		TPSingleton<MetaConditionManager>.Instance.LogError("Script error: There is no such condition such as " + MetaCondition.MetaConditionDefinition.Name + " in the condition library! Valid conditions are: " + string.Join(", ", conditionsLibrary.Keys), CLogLevel.DETAILED);
	}

	public void RenewLocalContext()
	{
		MetaCondition.LocalContext = new MetaConditionSpecificContext();
		contexts["LOCAL"] = MetaCondition.LocalContext;
	}

	public void SetRunContext(MetaConditionContext context)
	{
		contexts["RUN"] = context;
	}

	public void SetCampaignContext(MetaConditionContext context)
	{
		contexts["CAMPAIGN"] = context;
	}

	private string GetLocalizedKeyword(string keyword)
	{
		for (int i = 0; i < keywordPotentialLocalizationPrefixes.Length; i++)
		{
			if (Localizer.TryGet(keywordPotentialLocalizationPrefixes[i] + keyword, out var value))
			{
				return value;
			}
		}
		return keyword;
	}

	private bool ParseArgument(string strArgument, out object argument)
	{
		argument = null;
		if (double.TryParse(strArgument, out var result))
		{
			argument = result;
			return true;
		}
		string[] array = strArgument.Split(':');
		if (array.Length > 1)
		{
			string text = array[0];
			string keywordName = array[1];
			if (!contexts.ContainsKey(text))
			{
				TPSingleton<MetaConditionManager>.Instance.LogError("Invalid context name specified: " + text + " in condition " + MetaCondition.ToString() + ". Valid context names are: " + string.Join(", ", contexts.Keys), CLogLevel.DETAILED);
				return false;
			}
			MetaConditionContext metaConditionContext = contexts[text];
			if (metaConditionContext == null)
			{
				TPSingleton<MetaConditionManager>.Instance.LogError($"Could not fetch the appropriate existing context {text} for condition {MetaCondition}! " + "If you did not forget to deserialize Game or App, THIS COULD BE BIG PROBLEM!\nAdditional info: [Upgrade Id:" + MetaCondition.MetaUpgradeController.MetaUpgrade.MetaUpgradeDefinition.Id + "] [" + string.Join(", ", contexts.Select((KeyValuePair<string, MetaConditionContext> o) => o.Key + "=>" + o.Value)) + "]", CLogLevel.DETAILED);
				return false;
			}
			keywordName = keywordName.Trim();
			if (keywordName.Contains(' '))
			{
				string[] array2 = keywordName.Split(' ');
				string functionName = array2[0];
				string[] array3 = array2.Skip(1).ToArray();
				MethodInfo[] methods = metaConditionContext.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				IEnumerable<MethodInfo> source;
				if ((source = methods.Where((MethodInfo o) => o.Name.ToLower() == functionName.ToLower())).Count() > 0)
				{
					MethodInfo methodInfo = source.FirstOrDefault();
					object[] parameters = array3;
					argument = methodInfo.Invoke(metaConditionContext, parameters);
					return true;
				}
				TPSingleton<MetaConditionManager>.Instance.LogError("Invalid context element: function " + functionName + " does NOT exist in context " + text + ". Available elements: " + string.Join(", ", from o in methods
					where !o.Name.StartsWith("get_") && !o.Name.StartsWith("set_")
					select o.Name), CLogLevel.DETAILED);
				return false;
			}
			FieldInfo[] fields = metaConditionContext.GetType().GetFields();
			PropertyInfo[] properties = metaConditionContext.GetType().GetProperties();
			IEnumerable<FieldInfo> source2;
			if ((source2 = fields.Where((FieldInfo o) => o.Name.ToLower() == keywordName.ToLower())).Count() > 0)
			{
				FieldInfo fieldInfo = source2.LastOrDefault();
				if (!supportedContextTypes.Contains(fieldInfo.FieldType))
				{
					TPSingleton<MetaConditionManager>.Instance.LogError("You cannot access member " + keywordName + " from context, because its type is not supported yet.", CLogLevel.DETAILED);
					return false;
				}
				argument = source2.LastOrDefault().GetValue(metaConditionContext);
				return true;
			}
			IEnumerable<PropertyInfo> source3;
			if ((source3 = properties.Where((PropertyInfo o) => o.Name.ToLower() == keywordName.ToLower())).Count() > 0)
			{
				PropertyInfo propertyInfo = source3.FirstOrDefault();
				if (!supportedContextTypes.Contains(propertyInfo.PropertyType))
				{
					TPSingleton<MetaConditionManager>.Instance.LogError("You cannot access member " + keywordName + " from context, because its type is not supported yet.", CLogLevel.DETAILED);
					return false;
				}
				argument = propertyInfo.GetValue(metaConditionContext);
				return true;
			}
			IEnumerable<string> values = (from o in fields
				where supportedContextTypes.Contains(o.FieldType)
				select o.Name).Concat(from o in properties
				where supportedContextTypes.Contains(o.PropertyType)
				select o.Name);
			TPSingleton<MetaConditionManager>.Instance.LogError("Invalid context element: " + keywordName + " does NOT exist in context " + text + ". Available elements: " + string.Join(",", values), CLogLevel.DETAILED);
			return false;
		}
		argument = strArgument;
		return true;
	}

	private List<string> ParseLocalizationArguments()
	{
		List<string> list = new List<string>();
		foreach (string argument in MetaCondition.MetaConditionDefinition.Arguments)
		{
			if (double.TryParse(argument, out var result))
			{
				list.Add(result.ToString());
				continue;
			}
			string[] array = argument.Split(':');
			if (array.Length > 1)
			{
				_ = array[0];
				string text = array[1];
				if (text.Contains(' '))
				{
					string[] array2 = text.Split(' ');
					_ = array2[0];
					string[] array3 = array2.Skip(1).ToArray();
					for (int i = 0; i < array3.Length; i++)
					{
						list.Add(GetLocalizedKeyword(array3[i]));
					}
				}
			}
			else
			{
				list.Add(GetLocalizedKeyword(argument));
			}
		}
		return list;
	}

	private string ParseLocalizationKey()
	{
		foreach (string argument in MetaCondition.MetaConditionDefinition.Arguments)
		{
			string[] array = argument.Split(':');
			if (array.Length <= 1)
			{
				continue;
			}
			string text = array[1];
			if (text.Contains(' '))
			{
				List<string> list = text.Split(' ').ToList();
				string text2 = list[0];
				while (list.Count > 1)
				{
					string text3 = "MetaCondition_";
					for (int i = 0; i < list.Count; i++)
					{
						text3 += list[i];
						if (i < list.Count - 1)
						{
							text3 += "_";
						}
					}
					if (Localizer.TryGet(text3, out var _))
					{
						return text3;
					}
					list.RemoveAt(list.Count - 1);
				}
				return "MetaCondition_" + text2;
			}
			return "MetaCondition_" + text;
		}
		return string.Empty;
	}
}
