using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSTOP_Dollhouse : MonoBehaviour
{
    //The dollhouse is a small house, that can be captured and scaled up. 
    //When an object enters the dollhouse, it retains it's position relative to the dollhouse and scales with it

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Capturable")
        {
            other.gameObject.transform.SetParent(this.gameObject.transform);
            other.gameObject.layer = 3;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Capturable")
        {
            other.gameObject.transform.SetParent(null);
            other.gameObject.layer = 1;
        }
    }
}
