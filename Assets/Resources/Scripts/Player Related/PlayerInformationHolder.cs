using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInformationHolder : NetworkBehaviour
{
    public string gameCode;
    public GameObject playerPrefab;
    [HideInInspector]
    public GameObject playerObject;
    public string targetSceneName;
    bool gameStarted;
    int nextObjectSuffix;
    
    // Start is called before the first frame update
    void Start()
    {   
        name = "PlayerInformationHolder" + OwnerClientId;
        DontDestroyOnLoad(gameObject);

        if (IsOwner){
             gameCode = PlayModeMenu.instance.code;
        }
        
        if (IsHost && IsOwner){
            SceneManager.LoadScene(targetSceneName);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!gameStarted && SceneManager.GetActiveScene() == SceneManager.GetSceneByName(targetSceneName)){
            SpawnNetworkObjectWithOwnerServerRpc(playerPrefab.name, new Vector3(-11, 7, 0), OwnerClientId);

            gameStarted = true;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    // Use this to spawn objects
    public void SpawnObjectServerRpc(string objectToSpawn, Vector3 position)
    {
        GameObject newObject = Instantiate(Resources.Load<GameObject>("Objects/Prefabs/" + objectToSpawn), position, Quaternion.identity);
        // Generate an indexed name
        string newName = GenerateObjectName(objectToSpawn);
        newObject.name = newName;
        AdvanceObjectSuffix();

        SpawnObjectClientRpc(objectToSpawn, position);
    }

    [ClientRpc]
    // Don't touch this
    void SpawnObjectClientRpc(string objectToSpawn, Vector3 position)
    {
        if (IsServer) return;
        GameObject newObject = Instantiate(Resources.Load<GameObject>("Objects/Prefabs/" + objectToSpawn), position, Quaternion.identity);

        string newName = GenerateObjectName(objectToSpawn);
        newObject.name = newName;
        AdvanceObjectSuffix();
    }

    [ServerRpc(RequireOwnership = false)]
    // Use this to spawn network objects
    public void SpawnNetworkObjectWithOwnerServerRpc(string objectToSpawn, Vector3 position, ulong ownerId)
    {
        GameObject newObject = Instantiate(Resources.Load<GameObject>("Objects/Prefabs/" + objectToSpawn), position, Quaternion.identity);

        // Fix name
        string newName = GenerateObjectName(objectToSpawn);
        AdvanceObjectSuffix();
        newObject.name = newName;

        // Update on server
        NetworkObject networkObject = newObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        networkObject.ChangeOwnership(ownerId);
    }

    public string GenerateObjectName(string oldName){
        string newName = oldName + "_" + OwnerClientId + "_"+ nextObjectSuffix;
        return newName;
    }

    public void AdvanceObjectSuffix(){
        nextObjectSuffix++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendPositionServerRpc(Vector3 position, string objectName, ulong ignoreClient){
        // Update the server transform
        GameObject target = GameObject.Find(objectName);
        try{
            target.transform.position = position;
            SendPositionClientRpc(position, objectName, ignoreClient);
        }catch{
            Debug.LogWarning("The object you are trying to move doesn't exist!");
        }
    }

    [ClientRpc]
    public void SendPositionClientRpc(Vector3 position, string objectName, ulong ignoreClient){
        // Update the client transform
        if (IsServer || (IsLocalPlayer && OwnerClientId == ignoreClient)) return;

        GameObject target = GameObject.Find(objectName);
        try{
            target.transform.position = position;
        }catch{
            Debug.LogWarning("The object you are trying to move doesn't exist!");
        }
    }
    
}
