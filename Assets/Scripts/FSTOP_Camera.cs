using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialExtensions
{
    public static void ToOpaqueMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void ToFadeMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}

public class FSTOP_Camera : MonoBehaviour
{
    public GameObject captured;
    public bool captureMode = false;
    public AudioSource soundSource;
    public AudioClip captureSound;
    public AudioClip releaseSound;
    public float maxPlacementDistance;
    public float maxCaptureDistance;
    public Vector3 resetScaleMin;
    public Vector3 resetScaleMax;

    // Start is called before the first frame update
    void ChangeAlpha(Material mat, float alphaVal)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaVal);
        mat.SetColor("_Color", newColor);

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!captureMode)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 fwd = transform.TransformDirection(Vector3.forward);
                RaycastHit hit;
                if (Physics.Raycast(transform.position, fwd, out hit, maxCaptureDistance))
                {
                    if (hit.transform.tag == "Capturable")
                    {
                        print("We captured " + hit.transform.gameObject.name + "!");
                        // Now capture the object
                        GameObject capObject = hit.transform.gameObject;
                        // Make it transparent
                        capObject.GetComponent<MeshRenderer>().material.ToFadeMode();
                        ChangeAlpha(capObject.GetComponent<MeshRenderer>().material, 0.4f);
                        // Make the rigidbody kinematic and disable raycasts
                        if (capObject.GetComponent<Rigidbody>()) 
                         {
                            capObject.GetComponent<Rigidbody>().isKinematic = true;
                         }
                        capObject.layer = 2;
                        // Disable the collisions for the object by making it a trigger
                        capObject.GetComponent<Collider>().isTrigger = true;
                        // Now we can actually start placement mode
                        soundSource.clip = captureSound;
                        soundSource.Play();
                        captured = capObject;
                        captureMode = true;

                    }
                }
            }
            }
            else
            {
                // Placement mode!
                Vector3 fwd = transform.TransformDirection(Vector3.forward);
                RaycastHit hit;

            // Scale mechanic
            Vector3 newScale = captured.transform.localScale;
            new Vector3();
            newScale.x = Mathf.Lerp(newScale.x, newScale.x + Input.GetAxis("Mouse ScrollWheel") * 2f, Time.deltaTime * 80f);
            newScale.y = Mathf.Lerp(newScale.y, newScale.y + Input.GetAxis("Mouse ScrollWheel") * 2f, Time.deltaTime * 80f);
            newScale.z = Mathf.Lerp(newScale.z, newScale.z + Input.GetAxis("Mouse ScrollWheel") * 2f, Time.deltaTime * 80f);
            //max is 10.2, min is 0.2
            captured.transform.localScale = newScale;

            
            if (captured.GetComponent<Rigidbody>()) 
            {
                captured.GetComponent<Rigidbody>().mass = Mathf.Lerp(captured.GetComponent<Rigidbody>().mass, captured.GetComponent<Rigidbody>().mass + Input.GetAxis("Mouse ScrollWheel") * 30f, Time.deltaTime * 80f);
            }


            if (Physics.Raycast(transform.position, fwd, out hit, maxPlacementDistance, 1))
            {
                /*
                if(captured.activeSelf == false)
                {
                    captured.SetActive(true);
                }
                */
                if(hit.transform.gameObject.tag != "Capturable")
                {
                    if(hit.normal.y < 0.5)
                    {
                        captured.transform.position = hit.point + hit.normal * captured.GetComponent < Collider > ().bounds.extents.x;
                    }

                    if(hit.normal.z < -0.5 && !(hit.normal.z > 0.5))
                    {
                        captured.transform.position = hit.point + hit.normal * captured.GetComponent < Collider > ().bounds.extents.z;
                    }
                    if(hit.normal.z > 0.5)
                    {
                        captured.transform.position = hit.point + hit.normal * captured.GetComponent < Collider > ().bounds.extents.z;
                    }

                    if(!(hit.normal.y < 0.5) && !(hit.normal.z < -0.5) && !(hit.normal.z > 0.5))
                    {
                        captured.transform.position = hit.point + hit.normal * captured.GetComponent < Collider > ().bounds.extents.y;
                    }

                    captured.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }

            }
            /*
            else
            {
                captured.SetActive(false);
            }
            */
            if(Input.GetMouseButtonUp(0))
            {
                print("We placed " + captured.name + "!");
                //Enable the object first
                captured.SetActive(true);
                // Make it opaque
                captured.GetComponent<MeshRenderer>().material.ToOpaqueMode();
                ChangeAlpha(captured.GetComponent<MeshRenderer>().material, 1f);
                // Unmake the rigidbody kinematic and enable raycasts
                if (captured.GetComponent<Rigidbody>()) 
                {
                    captured.GetComponent<Rigidbody>().isKinematic = false;
                }
                captured.layer = 0;
                // Make the object collide again
                captured.GetComponent<Collider>().isTrigger = false;
                // Now we can actually disable placement mode
                soundSource.clip = releaseSound;
                soundSource.Play();
                captured = null;
                captureMode = false;
            }
        }
    }
}
