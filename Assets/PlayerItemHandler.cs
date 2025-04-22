using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [Header("Settings")]
    public float grabDistance;
    public Vector2 offsetFromPlayer;
    public Material materialTarget;
    public Material materialTargetable;
    public Material baseMaterial;
    Carriable currentHeldBox;
    Carriable currentTarget;
    int checkFrames = 15;
    int currentFrame;
    GameObject[] allCarriables;


    void Update()
    {
        // Find all carriable objects
        // TODO: replace this to trigger when a box spawns/despawns, and not on a timer
        if (currentFrame == 0){
            allCarriables = GetCarriableObjects();
            currentFrame = checkFrames;
        }else{
            currentFrame -= 1;
        }

        // Get all grabbable objects (carriables in grabDistance to the player)
        GetGrabbableCarriables();

        // Get player inputs
        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null){
            if (currentHeldBox != null) DropBox();

            GrabBox();
        }

        if (Input.GetKeyDown(KeyCode.E) && currentHeldBox != null){
            DropBox();
        }
    }

    GameObject[] GetCarriableObjects(){
        // Get all carriables
        GameObject[] allCarriables = GameObject.FindGameObjectsWithTag("Carriable");
        return allCarriables;
    }
    List<Carriable> GetGrabbableCarriables(){
        // Find and mark all carriables within grabDistance
        List<Carriable> grabbableCarriables = new List<Carriable>();
        float closestDistance = -1;
        for (int i = 0; i < allCarriables.Length; i++){
            // Skip the box the player is holding
            if (currentHeldBox != null && allCarriables[i] == currentHeldBox.gameObject) continue;

            // Find the distance between the player and the box
            float distance = Vector3.Distance(allCarriables[i].transform.position, transform.position); 
            if (distance < grabDistance){
                // If the box is within grabDistance mark it
                grabbableCarriables.Add(allCarriables[i].GetComponent<Carriable>());

                if (closestDistance > distance || closestDistance < 0){
                    // If this is the closest box mark it as such
                    closestDistance = distance;
                    if (currentTarget != null) currentTarget.SetMaterial(materialTargetable);
                    currentTarget = grabbableCarriables[grabbableCarriables.Count-1];
                    currentTarget.SetMaterial(materialTarget);
                }else{
                    // If this isn't the closest target mark it normaly
                    grabbableCarriables[grabbableCarriables.Count-1].SetMaterial(materialTargetable);
                }   
            }else{
                // Reset material on targets that are far away
                allCarriables[i].GetComponent<Carriable>().SetMaterial(baseMaterial);
            }
        }

        // Return all boxes within grabDistance of the player
        return grabbableCarriables;
    }
    void GrabBox(){

    }
    void DropBox(){

    }
}
