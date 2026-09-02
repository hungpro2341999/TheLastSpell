using Rewired.Dev;

namespace RewiredConsts;

public static class Action
{
	public static class Default
	{
		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Cursor Horizontal")]
		public const int CursorHorizontal = 20;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Cursor Vertical")]
		public const int CursorVertical = 21;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Submit")]
		public const int Submit = 22;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "SubmitNoThroughUI")]
		public const int SubmitNoThroughUI = 24;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Cancel")]
		public const int Cancel = 23;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "OpenSettings")]
		public const int OpenSettings = 54;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "ToggleFullscreen")]
		public const int ToggleFullscreen = 55;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Validate")]
		public const int Validate = 66;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Deselect")]
		public const int Deselect = 137;
	}

	public static class Camera
	{
		[ActionIdFieldInfo(categoryName = "Camera", friendlyName = "Cam Vertical")]
		public const int CamVertical = 18;

		[ActionIdFieldInfo(categoryName = "Camera", friendlyName = "Cam Horizontal")]
		public const int CamHorizontal = 17;

		[ActionIdFieldInfo(categoryName = "Camera", friendlyName = "Zoom")]
		public const int Zoom = 14;

		[ActionIdFieldInfo(categoryName = "Camera", friendlyName = "Pan")]
		public const int Pan = 27;

		[ActionIdFieldInfo(categoryName = "Camera", friendlyName = "Focus Joystick Tile")]
		public const int FocusJoystickTile = 81;

		[ActionIdFieldInfo(categoryName = "Camera", friendlyName = "Focus Selected Hero")]
		public const int FocusSelectedHero = 136;
	}

	public static class Management
	{
		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "End Turn")]
		public const int EndTurn = 7;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Construction Production")]
		public const int ConstructionProduction = 62;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Construction Defensive")]
		public const int ConstructionDefensive = 5;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Destroy Building")]
		public const int DestroyBuilding = 6;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "CharacterSheet")]
		public const int CharacterSheet = 16;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Inventory")]
		public const int Inventory = 10;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Shop")]
		public const int Shop = 30;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "MetaShops")]
		public const int MetaShops = 58;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Close")]
		public const int Close = 29;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Recrutement Panel")]
		public const int Inn = 52;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Select Next Unit")]
		public const int SelectNextUnit = 0;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "Select Previous Unit")]
		public const int SelectPreviousUnit = 11;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "ChangeSkillTooltip")]
		public const int ChangeSkillTooltip = 48;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "ChangeEquipment")]
		public const int ChangeEquipment = 60;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit0")]
		public const int SelectUnit0 = 68;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit1")]
		public const int SelectUnit1 = 69;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit2")]
		public const int SelectUnit2 = 70;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit3")]
		public const int SelectUnit3 = 71;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit4")]
		public const int SelectUnit4 = 72;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit5")]
		public const int SelectUnit5 = 73;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "SelectUnit6")]
		public const int SelectUnit6 = 74;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "GameAcceleration")]
		public const int GameAcceleration = 76;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "PreviousConstructionState")]
		public const int PreviousConstructionState = 95;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "NextConstructionState")]
		public const int NextConstructionState = 96;

		[ActionIdFieldInfo(categoryName = "Management", friendlyName = "ChangePerkSkillTooltip")]
		public const int ChangePerkSkillTooltip = 140;
	}

	public static class Combat
	{
		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "SelectPreviousSkill")]
		public const int SelectPreviousSkill = 83;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "SelectNextSkill")]
		public const int SelectNextSkill = 82;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Rotate skill")]
		public const int RotateSkill = 61;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "CancelMovement")]
		public const int CancelMovement = 53;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "ShowEnemiesMoveRange")]
		public const int ShowEnemiesMoveRange = 59;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Weapon 1")]
		public const int SkillWeapon1 = 116;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skil lWeapon 2")]
		public const int SkillWeapon2 = 117;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Weapon 3")]
		public const int SkillWeapon3 = 118;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Weapon 4")]
		public const int SkillWeapon4 = 119;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Punch")]
		public const int SkillPunch = 120;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Armor")]
		public const int SkillArmor = 121;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Equipment 1")]
		public const int SkillEquipment1 = 122;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Equipment 2")]
		public const int SkillEquipment2 = 123;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Equipment 3")]
		public const int SkillEquipment3 = 124;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Equipment 4")]
		public const int SkillEquipment4 = 125;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Equipment 5")]
		public const int SkillEquipment5 = 126;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Equipment 6")]
		public const int SkillEquipment6 = 127;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 1")]
		public const int SkillContextual1 = 128;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 2")]
		public const int SkillContextual2 = 129;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 3")]
		public const int SkillContextual3 = 130;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 4")]
		public const int SkillContextual4 = 131;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 5")]
		public const int SkillContextual5 = 132;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 6")]
		public const int SkillContextual6 = 133;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 7")]
		public const int SkillContextual7 = 134;

		[ActionIdFieldInfo(categoryName = "Combat", friendlyName = "Skill Contextual 8")]
		public const int SkillContextual8 = 135;
	}

	public static class LevelEditor
	{
		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "Undo")]
		public const int Undo = 25;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "Redo")]
		public const int Redo = 26;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "OpenSelectGround")]
		public const int OpenSelectGround = 32;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "OpenSelectBuilding")]
		public const int OpenSelectBuilding = 33;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "OpenDestroyBuilding")]
		public const int OpenDestroyBuilding = 34;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "Save")]
		public const int Save = 35;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "ToggleLevelArt")]
		public const int ToggleLevelArt = 57;

		[ActionIdFieldInfo(categoryName = "LevelEditor", friendlyName = "OpenSetTileFlag")]
		public const int OpenSetTileFlag = 65;
	}

	public static class Debug
	{
		[ActionIdFieldInfo(categoryName = "Debug", friendlyName = "DebugSpeedUp")]
		public const int DebugSpeedUp = 49;

		[ActionIdFieldInfo(categoryName = "Debug", friendlyName = "DebugSpeedDown")]
		public const int DebugSpeedDown = 51;
	}

	public static class Dropdown
	{
		[ActionIdFieldInfo(categoryName = "Dropdown", friendlyName = "CloseDropdown")]
		public const int CloseDropdown = 63;

		[ActionIdFieldInfo(categoryName = "Dropdown", friendlyName = "SelectItem")]
		public const int SelectItem = 64;
	}

	public static class UI
	{
		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIHorizontal")]
		public const int UIHorizontal = 77;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIVertical")]
		public const int UIVertical = 78;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UISubmit")]
		public const int UISubmit = 79;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UICancel")]
		public const int UICancel = 80;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "HUD Navigation Switch")]
		public const int HUDNavigationSwitch = 84;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "HUD Navigation Horizontal")]
		public const int HUDNavigationHorizontal = 85;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "HUD Navigation Vertical")]
		public const int HUDNavigationVertical = 86;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Display Tooltip")]
		public const int DisplayTooltip = 87;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Perk Reroll Mode")]
		public const int PerkRerollMode = 143;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "CharacterSheet Next Tab")]
		public const int CharacterSheetNextTab = 88;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "CharacterSheet Previous Tab")]
		public const int CharacterSheetPreviousTab = 89;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Train Perk")]
		public const int TrainPerk = 90;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Level Up Reroll")]
		public const int LevelUpReroll = 91;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Level Up Switch Box Right")]
		public const int LevelUpSwitchBoxRight = 92;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Level Up Switch Box Left")]
		public const int LevelUpSwitchBoxLeft = 93;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Open Level Up")]
		public const int OpenLevelUp = 94;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Shop Reroll")]
		public const int ShopReroll = 101;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "RewardReroll")]
		public const int RewardReroll = 102;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Recruitment Reroll")]
		public const int RecruitmentReroll = 104;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Recruitment Next")]
		public const int RecruitmentNext = 105;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Recruitment Previous")]
		public const int RecruitmentPrevious = 106;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Settings Next Tab")]
		public const int SettingsNextTab = 109;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Settings Previous Tab")]
		public const int SettingsPreviousTab = 110;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Unit Customization Confirm")]
		public const int UnitCustomizationConfirm = 112;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Unit Customization Next Option")]
		public const int UnitCustomizationNextOption = 113;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Unit Customization Previous Option")]
		public const int UnitCustomizationPreviousOption = 114;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Sort Meta Shop")]
		public const int SortMetaShop = 115;

		[ActionIdFieldInfo(categoryName = "UI", friendlyName = "PerkBookmark")]
		public const int PerkBookmark = 138;
	}

	public static class WorldMap
	{
		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Open Oraculum")]
		public const int OpenOraculum = 97;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Easy Mode On")]
		public const int EasyModeOn = 98;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Easy Mode Off")]
		public const int EasyModeOff = 99;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Start Game")]
		public const int StartGame = 100;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Select Next City")]
		public const int SelectNextCity = 107;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Select Previous City")]
		public const int SelectPreviousCity = 108;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Open Weapon Restrictions")]
		public const int OpenWeaponRestrictions = 139;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "SelectStoryMap")]
		public const int SelectStoryMap = 141;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "SelectDLCMap")]
		public const int SelectDLCMap = 142;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Select Apocalypse Max Level View")]
		public const int SelectApocalypseMaxLevelView = 144;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Dropdown Previous")]
		public const int DropdownPrevious = 145;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Dropdown Next")]
		public const int DropdownNext = 146;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Apocalypse Sort Or Code Validation")]
		public const int ApocalypseSortOrCodeValidation = 147;

		[ActionIdFieldInfo(categoryName = "WorldMap", friendlyName = "Switch Glyph Apocalypse Panel")]
		public const int SwitchGlyphApocalypsePanel = 148;
	}

	public static class TutorialPopup
	{
		[ActionIdFieldInfo(categoryName = "TutorialPopup", friendlyName = "Validate Tutorial Popup")]
		public const int ValidateTutorialPopup = 111;
	}
}
