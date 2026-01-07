using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BattlePlayerController player = collision.GetComponent<BattlePlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
