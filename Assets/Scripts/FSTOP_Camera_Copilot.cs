using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
public class FSTOP_Camera_Copilot: MonoBehaviour {
  // Start is called before the first frame update
  public GameObject captured = null;
  public bool isCaptured = false;
    public AudioSource soundSource;
    public AudioClip captureSound;
    public AudioClip releaseSound;
    public GameObject thumbCam;
    public GameObject usedObject = null;
    public bool isUsing = false;
    public RawImage rawImage;
    public Texture2D capturedThumb = null;

    public GameObject uiCaptureText = null;
  void Start() {

    // Lock mouse to the center of screen
    Cursor.lockState = CursorLockMode.Locked;
  }


    // Function that takes a material and sets its _Color alpha to the specified float
    void ChangeAlpha(Material mat, float alphaVal)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaVal);
        mat.SetColor("_Color", newColor);

    }
    private void HandleNewSnapshotTexture (Texture2D texture)
{
    // IMPORTANT! Textures are not automatically GC collected. 
    // So in order to not allocate more and more memory consider actively destroying
    // a texture as soon as you don't need it anymore
    // if(capturedThumb != null) Destroy (capturedThumb);     
    // capturedThumb = null;
    capturedThumb = texture;
    rawImage.texture = texture;
}
  void doGhostEffect(GameObject obj)
  {
            // make the hit object transparent and set its render mode
            obj.GetComponent<MeshRenderer>().material.ToFadeMode();
            ChangeAlpha(obj.GetComponent<MeshRenderer>().material, 0.4f);
            // set its collider to trigger
            obj.GetComponent<Collider>().isTrigger = true;
  }
  IEnumerator GhostCoroutine(GameObject fizzy)
    {
        yield return new WaitForSeconds(0.1f);
        doGhostEffect(fizzy);
    }
  // Update is called once per frame
  void Update() {
    // If E is pressed, raycast forward from the camera, if the hit object has the Capturable tag, parent it to the camera and set its position 2f away forward from the camera
    if (Input.GetKeyDown(KeyCode.E))
    {
      if(!isUsing){
      RaycastHit hit;
      if (Physics.Raycast(transform.position, transform.forward, out hit, 10))
      {
        if (hit.collider.gameObject.tag == "Capturable")
        {
          isUsing = true;
          usedObject = hit.collider.gameObject;
          usedObject.transform.parent = transform;
          usedObject.transform.position = transform.position + transform.forward * 2;
          if (usedObject.GetComponent<Rigidbody>()) {
            usedObject.GetComponent<Rigidbody>().isKinematic = true;
          }
        }
      }
      }else{
        // Drop the object
        usedObject.transform.parent = null;
        isUsing = false;
          if (usedObject.GetComponent<Rigidbody>()) {
            usedObject.GetComponent<Rigidbody>().isKinematic = false;
          }
      }
    }
    if (!isCaptured && !isUsing) {
      // Capturing mode
      // if mouse down pressed, raycast forward from the camera, then see if the hit object has the "Capturable" tag, and print "hey!" to the console if it does
      if (Input.GetMouseButtonUp(0)) {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
          if (hit.transform.gameObject.tag == "Capturable") {
            // make the hit object captured, also set isCaptured to true
            captured = hit.transform.gameObject;
            captured.layer = 2;
            
            // Here comes the thumb camera part
            thumbCam.transform.position = Camera.main.transform.position;
            thumbCam.transform.LookAt(captured.transform);

            // Snapshot to RenderTexture
            //GetComponent<SnapshotController>().Snapshot(HandleNewSnapshotTexture);

            // End thumb camera part

            // check if the usedobj has a rigidbody, and if it does, freeze it
            if (captured.GetComponent<Rigidbody>()) {
              captured.GetComponent<Rigidbody>().isKinematic = true;
            }
            // apply ghost effects
            StartCoroutine(GhostCoroutine(captured));
            // play the capture sound using the soundSource and playoneshot
            soundSource.PlayOneShot(captureSound);
            // uiCaptureText is a textmeshpro object that we want to change to "placement"
            uiCaptureText.GetComponent<TextMeshProUGUI>().text = "place";
            // enable thumbcam rawimage
            rawImage.enabled = true;
            isCaptured = true;
          }
        }
      }
    }else if (isCaptured && !isUsing) {
      // Placement mode
      // When we press mouse down again, we need to set isCaptured to false, and set the captured object to non-kinematic and back to the default layer

      // Raycast forward from this gameobject, then move the captured object to the hit point
      RaycastHit hit;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out hit)) {
        // captured.transform.position = hit.point;
        // Rotate the object to fit the surface we hit
        //captured.transform.rotation = Quaternion.LookRotation(hit.normal);
        // This works, but it's rotated incorrectly. The correct way is: 
        // 1. Get the hit normal as a vector
        // 2. Create a rotation from the hit normal to the world's up vector
        // 3. Apply the rotation to the captured object
        captured.transform.rotation = Quaternion.FromToRotation(hit.normal, Vector3.up);
        
        // Instead of just putting the object at the hit point, we need to align it at whatever surface we hit
        // We can use the hit normal to align the object
        captured.transform.position = hit.point + hit.normal * captured.GetComponent < MeshRenderer > ().bounds.extents.y;


      }

      // When we scroll our mouse wheel up, increase the size of the captured object slightly. When we scroll it down, decrease the size of the object slightly
       /* if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            // increase the size of the object
            captured.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            // decrease the size of the object
            captured.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        } */
        // The code above works, but it doesn't increase the size smoothly, it increases in steps
        // Here is the code to increase the size of the object smoothly when you scroll
        Vector3 newScale = captured.transform.localScale;
        /*newScale.x += Input.GetAxis("Mouse ScrollWheel") * 1.4f;
        newScale.y += Input.GetAxis("Mouse ScrollWheel") * 1.4f;
        newScale.z += Input.GetAxis("Mouse ScrollWheel") * 1.4f;*/
        // Instead of just increasing the size of the object, we want to increase the size of the object smoothly
        // We can use the Lerp function to make the size change smoothly
        newScale.x = Mathf.Lerp(newScale.x, newScale.x + Input.GetAxis("Mouse ScrollWheel") * 5f, Time.deltaTime * 80f);
        newScale.y = Mathf.Lerp(newScale.y, newScale.y + Input.GetAxis("Mouse ScrollWheel") * 5f, Time.deltaTime * 80f);
        newScale.z = Mathf.Lerp(newScale.z, newScale.z + Input.GetAxis("Mouse ScrollWheel") * 5f, Time.deltaTime * 80f);
        captured.transform.localScale = newScale;
        // also increase the mass of the object when scaling it, make sure to lerp it the same way
        if (captured.GetComponent<Rigidbody>()) {
          captured.GetComponent<Rigidbody>().mass = Mathf.Lerp(captured.GetComponent<Rigidbody>().mass, captured.GetComponent<Rigidbody>().mass + Input.GetAxis("Mouse ScrollWheel") * 30f, Time.deltaTime * 80f);
        }
      if (Input.GetMouseButtonUp(0)) {
        // check if the captured has a rigidbody, and if it does, unfreeze it
        if (captured.GetComponent<Rigidbody>()) {
          captured.GetComponent<Rigidbody>().isKinematic = false;
        }
        // unset its render mode, set its alpha to 1, and set its collider to not trigger
        // also set the layer back to 0
        captured.GetComponent<MeshRenderer>().material.ToOpaqueMode();
        ChangeAlpha(captured.GetComponent<MeshRenderer>().material, 1f);
        captured.GetComponent<Collider>().isTrigger = false;
        captured.layer = 0;
        // play release sound using audiosource
        soundSource.PlayOneShot(releaseSound);
        uiCaptureText.GetComponent<TextMeshProUGUI>().text = "capture";
        // hide rawimage
        rawImage.enabled = false;
        isCaptured = false;
      }
    }

  }
}

// So, as seen in the code above, this is a game where you can capture and place objects.
// The objects are all made of cubes.
// The game has a "capture" mode and a "place" mode.
// When the game starts in the "capture" mode, you can click on any cube and it will be captured.
// When you click on a captured cube, the game will switch to the "place" mode.
// During the "place" mode, you can drag your captured cube to wherever you want.
// When you release the mouse button, the captured cube will be released, and the game will switch back to the "capture" mode.
// You can also scroll the mouse wheel up or down to change the size of the captured cube.