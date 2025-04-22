using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    public float destroyAfterSeconds;
    void Update()
    {
        if (destroyAfterSeconds <= 0) {
            Destroy(gameObject);
        }
        destroyAfterSeconds -= Time.deltaTime; 
    }
}
