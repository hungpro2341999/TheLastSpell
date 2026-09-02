using UnityEngine;

namespace TheLastStand.View.Unit;

public class EnemyAttackFeedback : MonoBehaviour
{
	[SerializeField]
	[Range(0.1f, 10f)]
	private float speed = 0.4f;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private int destroyTimer = 20;

	private int spawnTimer = 1;

	public EnemyUnitView EnemyUnitView { get; set; }

	public SpriteRenderer SpriteRenderer => spriteRenderer;

	public Vector3 TargetPosition { get; set; }

	private void Update()
	{
		if (--destroyTimer == 0)
		{
			Object.Destroy(base.gameObject);
		}
		else if (!(Vector3.Distance(base.transform.position, TargetPosition) < 0.1f) && --spawnTimer == 0)
		{
			EnemyAttackFeedback enemyAttackFeedback = Object.Instantiate(EnemyUnitView.EnemyAttackFeedbackPrefab);
			enemyAttackFeedback.transform.position = Vector3.MoveTowards(base.transform.position, TargetPosition, speed);
			enemyAttackFeedback.TargetPosition = TargetPosition;
			enemyAttackFeedback.transform.localScale = base.transform.localScale;
			enemyAttackFeedback.SpriteRenderer.color = spriteRenderer.color;
			enemyAttackFeedback.EnemyUnitView = EnemyUnitView;
		}
	}
}
