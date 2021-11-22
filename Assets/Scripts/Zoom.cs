using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    public bool enableZoom = true;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;
    private bool isZoomed = false;

    public GameObject playerCamera;

    public float fov = 60f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enableZoom)
        {
            if(Input.GetKeyDown(zoomKey))
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                  isZoomed = false;
                }
            }

            if(isZoomed)
            {
                playerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCamera.GetComponent<Camera>().fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else
            {
                playerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(playerCamera.GetComponent<Camera>().fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }
        
    }
}
