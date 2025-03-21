using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class CodeMonitor : MonoBehaviour
{
    public TextMeshProUGUI codeText;
    bool foundCode = false;
    void Update(){
        if (foundCode) return;
        
        GameObject playerInformationHolder = GameObject.Find("PlayerInformationHolder" + NetworkManager.Singleton.LocalClientId);
        if (playerInformationHolder != null){
            string code = playerInformationHolder.GetComponent<PlayerInformationHolder>().gameCode;
            codeText.text = code;
            foundCode = true;
        }
    }
}
