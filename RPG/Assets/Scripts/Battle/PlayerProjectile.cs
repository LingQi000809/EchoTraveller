using UnityEngine;
using System;

public class PlayerProjectile : MonoBehaviour
{
    public float damage = 20f;
    public float speed = 10f;
    public Sprite projectileSprite;
    public Color projectileColor = Color.white;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private SpriteRenderer spriteRenderer;
    private Action onReachedTargetCallback;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Initialize(Vector3 target, float dmg, Sprite sprite, Color color, Action onReachedTarget = null)
    {
        targetPosition = target;
        damage = dmg;
        projectileSprite = sprite;
        projectileColor = color;
        onReachedTargetCallback = onReachedTarget;

        if (spriteRenderer != null && projectileSprite != null)
        {
            spriteRenderer.sprite = projectileSprite;
            spriteRenderer.color = projectileColor;
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
        onReachedTargetCallback?.Invoke();
        Destroy(gameObject);
    }
}
