using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Common.Analytics;
using UnityEngine;

public class PlayerController: MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public float collideDist = 0.01f;
    public float interactDist = 0.01f;
    public bool isMoving;
    public bool canMove = true;
    private Vector2 input;
    public Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if (!canMove) return;

        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if(IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }

            }
        }
        
        animator.SetBool("isMoving", isMoving);

        if (Input.GetKeyDown(KeyCode.I))
        {
            Interact();
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPlayerMoveSound();
        }

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, collideDist, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }

    void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, interactDist, interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    public void DisablePlayerMovement()
    {
        canMove = false;
        isMoving = false;
        animator.SetBool("isMoving", false);
    }
    public void EnablePlayerMovement()
    {
        canMove = true;
    }
}
