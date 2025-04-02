using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    PlayerController playerController;
    SpriteRenderer spriteRenderer;
    public Animator animator;
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.IsOwner) return;

        // Flip the player sprite in the right direction
        bool facing = false;
        if (playerController.facing == 1){
             facing = true;
        }

        SendAnimationVariablesServerRpc(facing, playerController.isMoving, playerController.grounded, playerController.verticalVelocity);
    }

    [ClientRpc]
    void SwitchFacingDirectionClientRpc(bool right){
        if (right) spriteRenderer.flipX = false;
        else       spriteRenderer.flipX = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void SwitchFacingDirectionServerRpc(bool right){
        SwitchFacingDirectionClientRpc(right);
    }

    [ServerRpc(RequireOwnership = false)] 
    void SendAnimationVariablesServerRpc(bool facing, bool isMoving, bool grounded, float verticalVelocity){
        // Set all animation parameters
        SwitchFacingDirectionServerRpc(facing);

        animator.SetBool("IsWalking", isMoving);

        animator.SetBool("Grounded", grounded);

        animator.SetFloat("VerticalVelocity", verticalVelocity);
    }
}
