using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public int vectorAmountPerSide;
    public float bottomColliderDistance;
    public float sideColliderDistance;
    public float topColliderDistance;
    public float horizontalSpeed;
    float verticalVelocity;
    public float jumpStrenght;
    float currentGravity;
    public float maxGravity;
    float gravityTimer;
    bool grounded;
    PlayerInformationHolder playerInformationHolder;
    // Start is called before the first frame update
    void Start()
    {
        playerInformationHolder = GameObject.Find("PlayerInformationHolder" + OwnerClientId).GetComponent<PlayerInformationHolder>();
        name = "Player" + OwnerClientId;
        Camera.main.GetComponent<StickOnPlayer>().StickOnObject(transform);
    }

    // Update is called once per frame
    void Update()
    {   
        // This code should be executed by this objects owner only
        if (!IsOwner) return;

        Gravity();
        InputHorizontal();
        InputVertical();

        if (playerInformationHolder != null){
            // Update position on server
            playerInformationHolder.SendPositionServerRpc(transform.position, name, OwnerClientId);
        }else{
            // Try to get playerInformation holder
            playerInformationHolder = GameObject.Find("PlayerInformationHolder" + OwnerClientId).GetComponent<PlayerInformationHolder>();
        }
    }

    void InputHorizontal(){
        float input = Input.GetAxis("Horizontal");

        float moveAmount = horizontalSpeed * input * Time.deltaTime;
        
        bool willHitWall = false;

        // Check for collision
        if (input > 0){
            // Right check
            RaycastHit2D hit = new();
            
            // Raycast to the right
            for (int i = 0; i < vectorAmountPerSide; i++){
                Vector3 relativeVectorOrigin = new Vector3(0, Mathf.Lerp(bottomColliderDistance * 0.99f, topColliderDistance* 0.99f, (float)i/(vectorAmountPerSide-1)), 0);
                hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, Vector2.right, Mathf.Abs(moveAmount) + sideColliderDistance, LayerMask.GetMask("Ground"));
                if (hit.transform != null){
                    // Hit ground
                    willHitWall = true;
                    break;
                }
            }

            if (willHitWall){
                moveAmount = hit.distance - sideColliderDistance;
            }
        }else if (input < 0){
            // Right check
            RaycastHit2D hit = new();
            
            // Raycast to the left
            for (int i = 0; i < vectorAmountPerSide; i++){
                Vector3 relativeVectorOrigin = new Vector3(0, Mathf.Lerp(bottomColliderDistance * 0.99f, topColliderDistance* 0.99f, (float)i/(vectorAmountPerSide-1)), 0);
                hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, Vector2.left, Mathf.Abs(moveAmount) + sideColliderDistance, LayerMask.GetMask("Ground"));
                if (hit.transform != null){
                    // Hit ground
                    willHitWall = true;
                    break;
                }
            }

            if (willHitWall){
                moveAmount = -(hit.distance - sideColliderDistance);
            }
        }

        

        Vector3 newPosition = transform.position + new Vector3(moveAmount, 0, 0);

        transform.position = newPosition;
    }
    void InputVertical(){
        if (Input.GetKey(KeyCode.Space)){
            if (grounded && verticalVelocity == 0){
                verticalVelocity += jumpStrenght/5;
            }
        }

        if (verticalVelocity > 0){
            // Raycast upwards
            bool hitRoof = false;
            RaycastHit2D hit = new();

            for (int i = 0; i < vectorAmountPerSide; i++){
                Vector3 relativeVectorOrigin = new Vector3(Mathf.Lerp(-sideColliderDistance* 0.99f, sideColliderDistance* 0.99f, (float)i/(vectorAmountPerSide-1)), 0, 0);
                hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, Vector2.up, verticalVelocity*Time.deltaTime + topColliderDistance, LayerMask.GetMask("Ground"));
                if (hit.transform != null){
                    // Hit ground
                    hitRoof = true;
                    break;
                }
            }

            if (hitRoof){
                verticalVelocity = hit.distance - topColliderDistance;
            }

            Vector3 newPosition = transform.position + new Vector3(0, verticalVelocity * Time.deltaTime , 0);

            transform.position = newPosition;

            verticalVelocity += maxGravity * Time.deltaTime;
        }else{
            verticalVelocity = 0;
        }
    }
    void Gravity(){
        // Grounded check
        grounded = false;
        RaycastHit2D hit = new();
        
        // Raycast into the ground
        for (int i = 0; i < vectorAmountPerSide; i++){
            Vector3 relativeVectorOrigin = new Vector3(Mathf.Lerp(-sideColliderDistance* 0.99f, sideColliderDistance* 0.99f, (float)i/(vectorAmountPerSide-1)), 0, 0);
            //Visualize rays: Debug.DrawRay(transform.position + relativeVectorOrigin, -Vector2.down * (realVerticalVelocity + bottomColliderDistance), Color.red, Time.deltaTime);
            hit = Physics2D.Raycast(transform.position + relativeVectorOrigin, -Vector2.down, currentGravity + bottomColliderDistance, LayerMask.GetMask("Ground"));
            if (hit.transform != null){
                // Hit ground
                grounded = true;
                break;
            }
        }
        
        // Ground the player
        if (grounded && currentGravity != 0){
            currentGravity = -(hit.distance + bottomColliderDistance);
            verticalVelocity = 0;
        }   

        // Move
        Vector3 newPosition = transform.position + new Vector3(0, currentGravity, 0);
        transform.position = newPosition;

        // Update gravity
        if (grounded){
            gravityTimer = 0;
            currentGravity = 0;
        }else{
            gravityTimer += Time.deltaTime;
            currentGravity = Mathf.Lerp(0, maxGravity, gravityTimer/1.5f) * Time.deltaTime;
        }
    }
}
