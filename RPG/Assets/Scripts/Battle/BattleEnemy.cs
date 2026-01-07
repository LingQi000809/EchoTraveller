using UnityEngine;
using System.Collections;

public class BattleEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float horizontalMoveRange = 3f;
    [SerializeField] private float fixedYPosition = -3f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float projectileDamage = 10f;

    [Header("Audio")]
    [SerializeField] private AudioClip rayShootSound;

    [Header("References")]
    [SerializeField] private Transform projectileSpawnPoint;

    private Transform playerTransform;
    private Animator animator;
    private Vector2 arenaBoundsMin;
    private Vector2 arenaBoundsMax;
    private float minXPosition;
    private float maxXPosition;
    private int movementDirection = -1;
    private float nextAttackTime;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (projectileSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("ProjectileSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(-0.5f, 0, 0);
            projectileSpawnPoint = spawnPoint.transform;
        }
    }

    public void Initialize(BattleSceneData data, Transform player, Vector2 boundsMin, Vector2 boundsMax)
    {
        moveSpeed = data.enemyMoveSpeed;
        attackInterval = data.enemyAttackInterval;
        projectileSpeed = data.enemyProjectileSpeed;
        projectileDamage = data.enemyProjectileDamage;

        playerTransform = player;
        arenaBoundsMin = boundsMin;
        arenaBoundsMax = boundsMax;

        fixedYPosition = transform.position.y;

        minXPosition = 0f;
        maxXPosition = arenaBoundsMax.x;

        nextAttackTime = Time.time + attackInterval;
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        MoveHorizontally();

        if (Time.time >= nextAttackTime)
        {
            ShootProjectile();
            nextAttackTime = Time.time + attackInterval;
        }
    }

    private void MoveHorizontally()
    {
        float currentX = transform.position.x;
        float newX = currentX + (moveSpeed * movementDirection * Time.deltaTime);

        if (newX <= minXPosition)
        {
            newX = minXPosition;
            movementDirection = 1;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (newX >= maxXPosition)
        {
            newX = maxXPosition;
            movementDirection = -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        transform.position = new Vector3(newX, fixedYPosition, 0);
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || playerTransform == null) return;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (rayShootSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound(rayShootSound);
        }

        StartCoroutine(DelayedProjectileSpawn());
    }

    private IEnumerator DelayedProjectileSpawn()
    {
        yield return new WaitForSeconds(0.3f);

        if (projectilePrefab == null || playerTransform == null) yield break;

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Vector3 direction = (playerTransform.position - projectileSpawnPoint.position).normalized;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        rb.linearVelocity = direction * projectileSpeed;

        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<EnemyProjectile>();
        }
        projScript.damage = projectileDamage;

        Destroy(projectile, 5f);
    }

    public void TakeDamage(float damage)
    {
        if (BattleManager.instance != null)
        {
            BattleManager.instance.DamageEnemy(damage);
        }
    }

    public void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        StopAllCoroutines();
        enabled = false;
    }
}
