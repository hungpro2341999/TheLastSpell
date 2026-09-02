namespace TheLastStand.Model;

public interface IBossPhaseActor : ITileObject, IEntity
{
	string BossPhaseActorId { get; set; }

	bool IsBossPhaseActor { get; }

	bool BossActorDeathPrepared { get; }

	void PrepareBossActorDeath();
}
