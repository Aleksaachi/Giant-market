using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickOnPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    public void StickOnObject(Transform target){
        transform.SetParent(target);
        transform.localPosition = new Vector3(0, 0, transform.position.z);
    }
}
