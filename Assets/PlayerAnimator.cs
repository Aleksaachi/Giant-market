using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    PlayerController playerController;
    SpriteRenderer spriteRenderer;
    public Animator animator;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.IsOwner) return;

        // Flip the player sprite in the right direction
        if (playerController.facing == 1) spriteRenderer.flipX = false;
        else                              spriteRenderer.flipX = true;

        // Set all animation parameters
        animator.SetBool("IsWalking", playerController.isMoving);

        animator.SetBool("Grounded", playerController.grounded);

        animator.SetFloat("VerticalVelocity", playerController.verticalVelocity);
    }
}
