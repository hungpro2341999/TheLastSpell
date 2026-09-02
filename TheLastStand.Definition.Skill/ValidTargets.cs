using System.Collections.Generic;

namespace TheLastStand.Definition.Skill;

public class ValidTargets
{
	public class Constraints
	{
		public bool MustBeEmpty { get; set; }

		public bool NeedRepair { get; set; }

		public Constraints(bool mustBeEmpty, bool needRepair)
		{
			MustBeEmpty = mustBeEmpty;
			NeedRepair = needRepair;
		}
	}

	public bool AllUnits
	{
		get
		{
			if (EnemyUnits)
			{
				return PlayableUnits;
			}
			return false;
		}
	}

	public bool AnyUnits
	{
		get
		{
			if (!EnemyUnits)
			{
				return PlayableUnits;
			}
			return true;
		}
	}

	public Dictionary<string, Constraints> Buildings { get; set; }

	public bool EmptyTiles { get; set; }

	public bool EnemyUnits { get; set; }

	public bool PlayableUnits { get; set; }

	public bool WalkableCityTiles { get; set; }

	public bool WalkableTiles { get; set; }

	public bool UncrossableGrounds { get; set; }
}
