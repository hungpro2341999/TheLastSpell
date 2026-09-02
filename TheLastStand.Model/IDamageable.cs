using TheLastStand.Controller;
using TheLastStand.View;

namespace TheLastStand.Model;

public interface IDamageable
{
	float Armor { get; set; }

	float ArmorTotal { get; }

	IDamageableController DamageableController { get; }

	DamageableType DamageableType { get; }

	IDamageableView DamageableView { get; }

	float Health { get; set; }

	float HealthTotal { get; }

	bool IsDead { get; }

	bool IsTargetableByAI();

	bool CanBeDamaged();
}
