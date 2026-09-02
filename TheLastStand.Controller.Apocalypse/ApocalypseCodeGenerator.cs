using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse;

namespace TheLastStand.Controller.Apocalypse;

public static class ApocalypseCodeGenerator
{
	public enum E_FailureReason
	{
		None = -1,
		InvalidCodeFormat,
		InvalidModifierCodeSharingId,
		LockedModifier,
		InvalidStepIndex,
		DuplicateModifier
	}

	public class ApocalypseCodeDecodingData
	{
		private StringBuilder failureMessage = new StringBuilder();

		public E_FailureReason FailureReason { get; set; } = E_FailureReason.None;

		public string FailureModifierCode { get; set; }

		public ApocalypseModifierDefinition FailureModifierDefinition { get; set; }

		public string InvalidCodeSharingId { get; set; }

		public int InvalidStepIndex { get; set; }

		public List<ApocalypseModifierStepDefinition> ModifierStepDefinitions { get; private set; } = new List<ApocalypseModifierStepDefinition>();

		public bool Success => FailureReason == E_FailureReason.None;

		public string GetFailureMessage()
		{
			if (Success)
			{
				return string.Empty;
			}
			failureMessage.Clear();
			switch (FailureReason)
			{
			case E_FailureReason.DuplicateModifier:
				failureMessage.Append(Localizer.Format("ApocalypseEditCodePopup_Error_DuplicateModifier", FailureModifierDefinition.GetLocalizedTitle(), FailureModifierCode));
				break;
			case E_FailureReason.LockedModifier:
				failureMessage.Append(Localizer.Format("ApocalypseEditCodePopup_Error_LockedModifier", FailureModifierDefinition.GetLocalizedTitle(), FailureModifierCode));
				break;
			case E_FailureReason.InvalidCodeFormat:
				failureMessage.Append(Localizer.Get("ApocalypseEditCodePopup_Error_InvalidCodeFormat"));
				break;
			case E_FailureReason.InvalidStepIndex:
				failureMessage.Append(Localizer.Format("ApocalypseEditCodePopup_Error_InvalidModifierStepIndex", FailureModifierDefinition.GetLocalizedTitle(), FailureModifierCode, InvalidStepIndex));
				break;
			case E_FailureReason.InvalidModifierCodeSharingId:
				failureMessage.Append(Localizer.Get("ApocalypseEditCodePopup_Error_InvalidCodeFormat"));
				break;
			}
			return failureMessage.ToString();
		}

		public void Reset()
		{
			FailureReason = E_FailureReason.None;
			FailureModifierCode = string.Empty;
			ModifierStepDefinitions.Clear();
			FailureModifierDefinition = null;
			failureMessage.Clear();
		}
	}

	public static class Constants
	{
		public static class LocalizationKeys
		{
			public const string CodeErrorDuplicateModifier = "ApocalypseEditCodePopup_Error_DuplicateModifier";

			public const string CodeErrorInvalidFormat = "ApocalypseEditCodePopup_Error_InvalidCodeFormat";

			public const string CodeErrorInvalidModifierStepIndex = "ApocalypseEditCodePopup_Error_InvalidModifierStepIndex";

			public const string CodeErrorLockedModifier = "ApocalypseEditCodePopup_Error_LockedModifier";
		}

		public const string DecodingRegex = "([A-Z]+[0-9]+)";

		public const string CodeSharingIdRegex = "[A-Z]+";

		public const string StepIndexRegex = "[0-9]+";
	}

	public static string GenerateCode(TheLastStand.Model.Apocalypse.Apocalypse apocalypse)
	{
		if (apocalypse == null || apocalypse.ModifierStepDefinitions.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ApocalypseModifierStepDefinition modifierStepDefinition in apocalypse.ModifierStepDefinitions)
		{
			string value = modifierStepDefinition.ToCodeSharingFormat();
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	public static ApocalypseCodeDecodingData TryDecodeCode(string apocalypseCode)
	{
		ApocalypseCodeDecodingData apocalypseCodeDecodingData = new ApocalypseCodeDecodingData();
		MatchCollection matchCollection = Regex.Matches(apocalypseCode, "([A-Z]+[0-9]+)");
		if (matchCollection.Count == 0)
		{
			apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidCodeFormat;
			return apocalypseCodeDecodingData;
		}
		int length = apocalypseCode.Length;
		int num = 0;
		foreach (Match item2 in matchCollection)
		{
			Match match2 = Regex.Match(item2.Value, "[A-Z]+");
			Match match3 = Regex.Match(item2.Value, "[0-9]+");
			if (!match2.Success || !match3.Success)
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidCodeFormat;
				return apocalypseCodeDecodingData;
			}
			if (!ApocalypseDatabase.ModifierDefinitionsFromCodeSharingId.TryGetValue(match2.Value, out var value))
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidModifierCodeSharingId;
				apocalypseCodeDecodingData.InvalidCodeSharingId = match2.Value;
				return apocalypseCodeDecodingData;
			}
			if (!ApocalypseManager.IsModifierUnlocked(value))
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.LockedModifier;
				apocalypseCodeDecodingData.FailureModifierCode = item2.Value;
				apocalypseCodeDecodingData.FailureModifierDefinition = value;
				return apocalypseCodeDecodingData;
			}
			int num2 = int.Parse(match3.Value);
			if (num2 >= value.StepDefinitions.Count)
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidStepIndex;
				apocalypseCodeDecodingData.InvalidStepIndex = num2;
				apocalypseCodeDecodingData.FailureModifierCode = item2.Value;
				apocalypseCodeDecodingData.FailureModifierDefinition = value;
				return apocalypseCodeDecodingData;
			}
			ApocalypseModifierStepDefinition item = value.StepDefinitions[num2];
			if (apocalypseCodeDecodingData.ModifierStepDefinitions.Contains(item))
			{
				apocalypseCodeDecodingData.FailureReason = E_FailureReason.DuplicateModifier;
				apocalypseCodeDecodingData.FailureModifierCode = item2.Value;
				apocalypseCodeDecodingData.FailureModifierDefinition = value;
				return apocalypseCodeDecodingData;
			}
			num += match2.Value.Length + match3.Value.Length;
			apocalypseCodeDecodingData.ModifierStepDefinitions.Add(item);
		}
		if (length != num)
		{
			apocalypseCodeDecodingData.FailureReason = E_FailureReason.InvalidCodeFormat;
			return apocalypseCodeDecodingData;
		}
		return apocalypseCodeDecodingData;
	}
}
