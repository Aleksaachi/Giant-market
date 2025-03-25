using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Player collider box:")]
    public int raycastAmountPerSide;
    public float bottomColliderDistance;
    public float sideColliderDistance;
    public float topColliderDistance;
    [Header("Player settings:")]
    public float horizontalSpeed;
    public float walkTiltAngle;
    public float climbingSpeed;
    public float jumpStrenght;
    public float jumpHoldStrenght;
    public float jumpMaxHoldTime;
    public float coyoteTime;
    public float maxGravity;
    public float gravityStrength;
    [Header("Particle objects:")]
    public GameObject psPlayerFall;
    float verticalVelocity;
    float currentGravity;
    bool grounded;
    float coyoteTimer;
    bool ignorePlatforms;
    PlayerInformationHolder playerInformationHolder;
    Transform currentLadder;
    float ladderHeight;
    float jumpHoldTimer;
    float addedJumpForce;
    bool inJump;
    Animator animator;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        playerInformationHolder = GameObject.Find("PlayerInformationHolder" + OwnerClientId).GetComponent<PlayerInformationHolder>();
        name = "Player" + OwnerClientId;
        
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (IsOwner){
            Camera.main.GetComponent<StickOnPlayer>().StickOnObject(transform);
        }

        transform.position = new Vector3(-11, 7, 0);
    }
    void FixedUpdate()
    {   
        // This code should be executed by this objects owner only
        if (!IsOwner) return;

        // Check for ladder input
        CheckLadder();

        // If the player is not holding on to a ladder
        if (currentLadder == null){
            // Calculate last frames real vertical velocity

            // Do gravity checks
            CheckDropthrough();
            Gravity();
        }

        // Do movement checks
        InputHorizontal();
        InputVertical();

        // Update vertical position
        Vector3 newVerticalPosition = transform.position + new Vector3(0, verticalVelocity * Time.deltaTime, 0);
        transform.position = newVerticalPosition;

        // Try to send information to server
        if (playerInformationHolder != null){
            // Update position on server
            playerInformationHolder.SendPositionServerRpc(transform.position, name, OwnerClientId);
        }else{
            // Try to get playerInformationHolder
            playerInformationHolder = GameObject.Find("PlayerInformationHolder" + OwnerClientId).GetComponent<PlayerInformationHolder>();
        }
    }
    void InputHorizontal(){
        float input = Input.GetAxis("Horizontal");
        if (input != 0){
            // The player is trying to move
            animator.StopPlayback();

            // Calculate move amount
            float moveAmount = horizontalSpeed * input * Time.deltaTime;

            // Drop from ladder
            if (currentLadder != null){
                currentLadder = null;
                coyoteTimer = coyoteTime;
            }

            // Reset raycast variables
            RaycastHit2D[] hits = new RaycastHit2D[0];
            int groundIndex = 0;
            bool willHitWall = false;

            // Check for collision
            if (input > 0){
                // Right check
                spriteRenderer.flipX = false;
                
                // Raycast to the right
                for (int i = 0; i < raycastAmountPerSide; i++){
                    Vector3 relativeVectorOrigin = new Vector3(0, Mathf.Lerp(bottomColliderDistance * 0.99f, topColliderDistance* 0.99f, (float)i/(raycastAmountPerSide-1)), 0);
                    hits = Physics2D.RaycastAll(transform.position + relativeVectorOrigin, Vector2.right, Mathf.Abs(moveAmount) + sideColliderDistance, LayerMask.GetMask("Ground"));
                    for (int j = 0; j < hits.Length; j++){
                        if (hits[j].transform != null && hits[j].transform.CompareTag("Ground")){
                            // Hit ground
                            willHitWall = true;
                            groundIndex = j;
                            break;
                        }
                    }
                    if (willHitWall) break;
                }

                // Move against wall
                if (willHitWall){
                    moveAmount = hits[groundIndex].distance - sideColliderDistance;
                }
            }else if (input < 0){
                // Left check
                spriteRenderer.flipX = true;

                // Raycast to the left
                for (int i = 0; i < raycastAmountPerSide; i++){
                    Vector3 relativeVectorOrigin = new Vector3(0, Mathf.Lerp(bottomColliderDistance * 0.99f, topColliderDistance* 0.99f, (float)i/(raycastAmountPerSide-1)), 0);
                    hits = Physics2D.RaycastAll(transform.position + relativeVectorOrigin, Vector2.left, Mathf.Abs(moveAmount) + sideColliderDistance, LayerMask.GetMask("Ground"));
                    for (int j = 0; j < hits.Length; j++){
                        if (hits[j].transform != null && hits[j].transform.CompareTag("Ground")){
                            // Hit ground
                            willHitWall = true;
                            groundIndex = j;
                            break;
                        }
                    }
                    if (willHitWall) break;
                }

                // Move against wall
                if (willHitWall){
                    moveAmount = -(hits[groundIndex].distance - sideColliderDistance);
                }
            }
            // Move the player object
            Vector3 newPosition = transform.position + new Vector3(moveAmount, 0, 0);
            transform.position = newPosition;

            Quaternion newRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-walkTiltAngle, walkTiltAngle, input/2 + 0.5f));
            transform.rotation = newRotation;
        }else{
            animator.StartPlayback();
        }
    }
    void InputVertical(){
        // Jump hold timer
        if (inJump && jumpHoldTimer < jumpMaxHoldTime){
            jumpHoldTimer += Time.deltaTime;
        }else if (inJump){
             inJump = false;
        }

        // The player can jump if he is grounded or on a ladder
        if (Input.GetKey(KeyCode.Space)){
            if (!inJump && (coyoteTimer > 0 || grounded || currentLadder != null) && verticalVelocity <= 0){
                verticalVelocity = jumpStrenght/10;
                currentLadder = null;
                inJump = true;

                addedJumpForce = 0;
                jumpHoldTimer = 0;

                coyoteTimer = 0;
            }

            if (inJump){
                float force = jumpStrenght * Time.deltaTime;
                addedJumpForce += force;
                if (addedJumpForce > jumpHoldStrenght){
                     force = addedJumpForce - jumpHoldStrenght;
                     inJump = false;
                }

                verticalVelocity += force;
            }
        }

        // Roof check
        if (verticalVelocity > 0){
            // Raycast upwards
            bool hitRoof = false;
            RaycastHit2D hit = new();

            for (int i = 0; i < raycastAmountPerSide; i++){
                Vector3 relativeVectorOrigin = new Vector3(Mathf.Lerp(-sideColliderDistance* 0.99f, sideColliderDistance* 0.99f, (float)i/(raycastAmountPerSide-1)), 0, 0);
                hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, Vector2.up, verticalVelocity*Time.deltaTime + topColliderDistance, LayerMask.GetMask("Ground"));
                if (hit.transform != null && hit.transform.CompareTag("Ground")){
                    // Hit ground
                    hitRoof = true;
                    break;
                }
            }

            if (hitRoof){
                verticalVelocity = hit.distance - topColliderDistance;
            }
        }
    }
    void Gravity(){
        // Grounded check
        grounded = false;
        RaycastHit2D hit = new();

        if (coyoteTimer > 0){
            coyoteTimer -= Time.deltaTime;
        }

        // Raycast into the ground
        for (int i = 0; i < raycastAmountPerSide; i++){
            Vector3 relativeVectorOrigin = new Vector3(Mathf.Lerp(-sideColliderDistance* 0.98f, sideColliderDistance* 0.98f, (float)i/(raycastAmountPerSide-1)), 0, 0);
            //Visualize rays: Debug.DrawRay(transform.position + relativeVectorOrigin, -Vector2.down * (realVerticalVelocity + bottomColliderDistance), Color.red, Time.deltaTime);
            hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, -Vector2.down, verticalVelocity*Time.deltaTime + bottomColliderDistance, LayerMask.GetMask("Ground"));
            if (hit.transform != null){
                // Hit ground
                if (hit.transform.CompareTag("Ground") || (hit.transform.CompareTag("Platform") && !ignorePlatforms)){
                    grounded = true;
                    coyoteTimer = coyoteTime;
                    break;
                }
            }
        }
        // Ground the player
        if (grounded && currentGravity != 0){
            bool spawnFallParticles = false;
            if (verticalVelocity < maxGravity/3){
                spawnFallParticles = true;
            }
            currentGravity = -(hit.distance + bottomColliderDistance);
            verticalVelocity = 0;

            // Update vertical position
            Vector3 newVerticalPosition = transform.position + new Vector3(0, currentGravity, 0);
            transform.position = newVerticalPosition;

            if (spawnFallParticles){
                playerInformationHolder.SpawnObjectServerRpc(psPlayerFall.name, new Vector3(transform.position.x, transform.position.y - bottomColliderDistance, transform.position.z));
            }
        }   

        // Update gravity
        if (grounded){
            currentGravity = 0;
        }else{
            currentGravity -= gravityStrength * Time.deltaTime;
            currentGravity = Mathf.Clamp(currentGravity, maxGravity, 0);
        }

        verticalVelocity += currentGravity * Time.deltaTime;
    }
    void CheckDropthrough(){
        if (Input.GetKey(KeyCode.S) || verticalVelocity > 0){
            ignorePlatforms = true;
        }else{
            // Raycast into the ground
            RaycastHit2D hit;
            bool inPlatform = false;
            
            for (int i = 0; i < raycastAmountPerSide; i++){
                Vector3 relativeVectorOrigin = new Vector3(Mathf.Lerp(-sideColliderDistance* 0.99f, sideColliderDistance* 0.99f, (float)i/(raycastAmountPerSide-1)), 0, 0);
                hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, -Vector2.down, -verticalVelocity*Time.deltaTime + bottomColliderDistance, LayerMask.GetMask("Ground"));
                if (hit.transform != null && hit.transform.CompareTag("Platform")){
                    // Hit platform
                    inPlatform = true;
                    break;
                }
            }

            // If something is hit continiue ignoring platforms
            if (!inPlatform){
                ignorePlatforms = false;
            }
        }
    }
    void CheckLadder(){
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (verticalVelocity <= 0 && verticalInput != 0 && horizontalInput == 0 && currentLadder == null){
            // Check for ladders
            RaycastHit2D ladderHit = Physics2D.Raycast(transform.position - new Vector3(0, topColliderDistance, 0), Vector2.down, bottomColliderDistance + topColliderDistance, LayerMask.GetMask("Ladder"));
            if (ladderHit.transform != null){
                // Attach to the ladder
                currentLadder = ladderHit.transform;
                currentGravity = 0;
                verticalVelocity = 0;
                ladderHeight = ladderHit.transform.GetComponent<SpriteRenderer>().size.y;
                ignorePlatforms = true;
            }
        }else if (currentLadder != null){
            // Calculate moveAmount
            float moveAmount = verticalInput * Time.deltaTime * climbingSpeed;

            // Check if the player isn't at one of the ends of the ladder
            float ladderPoint = GetLadderPoint(moveAmount);
            if (ladderPoint > 1 || ladderPoint < 0){
                moveAmount = 0;
            }

            // Collision checks:
            if (moveAmount > 0){
                // Up check
                RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.up, moveAmount + topColliderDistance, LayerMask.GetMask("Ground"));
                if (groundHit.transform != null && groundHit.transform.CompareTag("Ground")){
                    moveAmount = groundHit.distance - topColliderDistance;
                }
            }else if (moveAmount < 0){
                RaycastHit2D groundHit = Physics2D.Raycast(transform.position, -Vector2.down, moveAmount + bottomColliderDistance, LayerMask.GetMask("Ground"));
                if (groundHit.transform != null && groundHit.transform.CompareTag("Ground")){
                    moveAmount = -(groundHit.distance + bottomColliderDistance);
                }
            }
            
            // Move player
            transform.position = new Vector3(transform.position.x, transform.position.y + moveAmount, transform.position.z);
        }
    }
    float GetLadderPoint(float offset){
        float yDifference = transform.position.y + offset - currentLadder.transform.position.y;
        return Mathf.LerpUnclamped(0, 1, yDifference/ladderHeight + 0.5f);
    }
}
