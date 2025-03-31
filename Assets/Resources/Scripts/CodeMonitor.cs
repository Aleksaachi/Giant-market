using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class CodeMonitor : MonoBehaviour
{
    public TextMeshProUGUI codeText;
    public ParticleSystem onClickParticles;
    public string code;
    bool foundCode = false;

    void Update(){
        if (foundCode) return;
        
        GameObject playerInformationHolder = GameObject.Find("PlayerInformationHolder" + NetworkManager.Singleton.LocalClientId);
        if (playerInformationHolder != null){
            code = playerInformationHolder.GetComponent<PlayerInformationHolder>().gameCode;
            code = code.ToUpper();
            codeText.text = code;
            foundCode = true;
        }
    }

    void OnMouseUp()
    {
        CopyToClipboard(code);
        if(codeText.text == code){
            ChangeText();
            Invoke(nameof(ChangeText), 2);
        }
    }

    public void CopyToClipboard(string text){
        GUIUtility.systemCopyBuffer = text;
    }

    public void ChangeText(){
        onClickParticles.Play();
        if(codeText.text != code){
            codeText.text = code;
        }else{
            codeText.text = "Copied!";
        }
    }
}
