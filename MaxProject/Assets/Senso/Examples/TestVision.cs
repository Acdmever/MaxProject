using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for planes 
public class TestVision : MonoBehaviour
{   

    public int cc;//MIDI CC value of the plane
    private Camera cam; //HMD camera
    // Start is called before the first frame update
    void Start() // Get camera and send script
    {
        cam = Camera.main;
      
    }

    // Update is called once per frame
    void Update()
    {}

    //Check visibility
    public bool IsInView()
    {

        //We find the center point of the plane
        Vector3 rend = gameObject.GetComponent<Renderer>().bounds.center;

        //Transform the center point to a location (x,y,z) in the viewport, if it is in view of the camera, all these values would be between 0-1
        Vector3 pointOnScreen = cam.WorldToViewportPoint(rend);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            //Debug.Log("Behind: " + gameObject.name);
            return false;
        }

        //Is not in FOV
        if ((pointOnScreen.x < 0.35) || (pointOnScreen.x > 0.65) ||
                (pointOnScreen.y < 0.35) || (pointOnScreen.y > 0.65))
        {
            //Debug.Log("OutOfBounds: " + gameObject.name);
            return false;
        }
        //Debug.Log("Is in view: " + gameObject.name);

        //If value is rendered
        if (GetComponent<Renderer>().enabled)
        {
            return true;
        }
        return false;


        
        

    }
}
//TODO Update manual
