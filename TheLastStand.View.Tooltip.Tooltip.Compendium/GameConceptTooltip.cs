using TPLib.Localization;

namespace TheLastStand.View.Tooltip.Tooltip.Compendium;

public class GameConceptTooltip : CompendiumEntryTooltip
{
	public static class Constants
	{
		public const string WoundsKey = "Wounds";

		public const string TitleFormat = "GameConceptName_{0}";

		public const string DescriptionFormat = "GameConceptDescription_{0}";

		private const string LocaPrefix = "GameConcept";

		private const string TitleInfix = "Name_";

		private const string DescriptionInfix = "Description_";
	}

	public string GameConceptId { get; set; } = string.Empty;

	private string TitleLocaKey => $"GameConceptName_{GameConceptId}";

	private string DescriptionLocaKey => $"GameConceptDescription_{GameConceptId}";

	protected override bool CanBeDisplayed()
	{
		if (GameConceptId != string.Empty && Localizer.Exists(TitleLocaKey))
		{
			return Localizer.Exists(DescriptionLocaKey);
		}
		return false;
	}

	protected override void OnHide()
	{
		base.OnHide();
		GameConceptId = string.Empty;
	}

	protected override void RefreshContent()
	{
		title.text = Localizer.Get(TitleLocaKey);
		description.text = Localizer.Get(DescriptionLocaKey);
	}
}
