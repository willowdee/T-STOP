using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSTOP_Fizzler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Corountine
    /*IEnumerator FizzleObject(GameObject fizzy)
    {
        yield return new WaitForSeconds(2);
        Destroy(fizzy);
    }*/
    // When we enter this trigger, if an object with the Capturable tag passes through it, we destroy it
    // Only do this if we are not in capture mode
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Capturable" && Camera.main.GetComponent<FSTOP_Camera>().captureMode == false)
        {
            /*other.gameObject.GetComponent<ParticleSystem>().Play();
            other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            other.gameObject.GetComponent<Collider>().enabled = false;
            FizzleObject(other.gameObject);*/
            Destroy(other.gameObject);
        }
    }
}
