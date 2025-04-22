using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Carriable : NetworkBehaviour
{
    public Item thisItem;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = thisItem.boxSprite;   
    }

    public void SetMaterial(Material material){
        spriteRenderer.material = material;
    }


}
