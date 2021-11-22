using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSTOP_Fan : MonoBehaviour
{
    // The fan raycasts in the direction it is facing and checks for every object with the "Capturable" tag. It then loops over them, and applies a force to them in the direction of the fan.
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Fan code
        Vector3 fanDirection = transform.up;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, fanDirection, 100);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.tag == "Capturable")
            {
                hit.rigidbody.AddForce(fanDirection * 0.3f, ForceMode.VelocityChange);
            }
            // if its the player, we need to add fanDirection * 15 instead of 10
            if (hit.collider.gameObject.layer == 3)
            {
                hit.rigidbody.AddForce(fanDirection * 0.3f, ForceMode.VelocityChange);
            }
        }
    }

}