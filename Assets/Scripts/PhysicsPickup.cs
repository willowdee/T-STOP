using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPickup : MonoBehaviour
{
    public float rotationSpeed = 15;

    //distance from the camera from which the item is carried
    public float Distance = 2.5f;

    //the distance from which the item can be picked up
    public float GrabDistance = 5f;

    //the object being held
    private GameObject curObject;
    private Rigidbody curBody;

    //the rotation of the curObject at pickup relative to the camera
    private Quaternion relRot;
   
    // Use this for initialization
    void Start ()
    {
       
    }
	
	// Update is called once per frame
    void Update ()
    {
        //on key press, either pickup or drop an item
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(curObject == null)
            {
                PickupItem();
            }
            else
            {
                DropItem();
            }
        }

        if(curObject != null & Input.GetKeyDown(KeyCode.R))
        {
            float xAxis = Input.GetAxis("Mouse X") * rotationSpeed;
            float yAxis = Input.GetAxis("Mouse Y") * rotationSpeed;

            curObject.transform.Rotate(Vector3.up, -xAxis);
            curObject.transform.Rotate(Vector3.right, -yAxis);
        }
    }

    void FixedUpdate()
    {
        if(curObject != null)
        {
            //keep the object in front of the camera
            ReposObject();
        }
    }

    //calculates the new rotation and position of the curObject
    void ReposObject()
    {
        //calculate the target position and rotation of the curbody
        Vector3 targetPos = transform.position + transform.forward * Distance;
        Quaternion targetRot = transform.rotation * relRot;

        //interpolate to the target position using velocity
        curBody.velocity = (targetPos - curBody.position) * 9;

        //keep the relative rotation the same
        curBody.rotation = targetRot;

        //no spinning around
        curBody.angularVelocity = Vector3.zero;
    }

    //attempts to pick up an item straigth ahead
    void PickupItem()
    {
        //raycast to find an item
        RaycastHit hitInfo;
        Physics.Raycast(transform.position, transform.forward, out hitInfo, GrabDistance, 1);

        if(hitInfo.rigidbody == null)
            return;


        curBody = hitInfo.rigidbody;
        curBody.useGravity = false;

        curObject = hitInfo.rigidbody.gameObject;

        
        //hack w/ parenting & unparenting to get the relative rotation
        curObject.transform.parent = transform;
        relRot = curObject.transform.localRotation;
        curObject.transform.parent = null;

       
    }

    //drops the current item
    void DropItem()
    {
        curBody.useGravity = true;
        curBody = null;
        curObject = null;
    }
}