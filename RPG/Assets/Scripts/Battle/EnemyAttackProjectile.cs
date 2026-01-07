using UnityEngine;

public class EnemyAttackProjectile : MonoBehaviour
{
    public float damage = 15f;
    public float speed = 6f;
    public Sprite projectileSprite;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Initialize(Vector3 target, float dmg, Sprite sprite)
    {
        targetPosition = target;
        damage = dmg;
        projectileSprite = sprite;

        if (spriteRenderer != null && projectileSprite != null)
        {
            spriteRenderer.sprite = projectileSprite;
        }

        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            OnReachedTarget();
        }
    }

    private void OnReachedTarget()
    {
        isMoving = false;
        Destroy(gameObject);
    }
}
