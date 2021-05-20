using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TachiRotate : MonoBehaviour
{
    public float Speed = 1f;
   
    Vector3 defaultR;
    float angel;

    private void Start()
    {
        defaultR = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
     
    }
    void Update()
    {
        angel+= Time.deltaTime * Speed;
        
        transform.rotation = Quaternion.Euler(defaultR.x, defaultR.y+angel, defaultR.z );
        
    }
}
