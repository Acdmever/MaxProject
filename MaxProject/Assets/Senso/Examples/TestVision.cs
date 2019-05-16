using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for planes 
public class TestVision : MonoBehaviour
{   

    public int cc;//MIDI CC value related to the plane
    private int c; //Frame count
    private bool seen,sent=false;//Plane is being seen by camera; Whether the value has been sent to SendMax, sent ensures us to send MIDI CC 0 only once, so that there is no problems with other planes
    private Camera cam; //HMD camera
    private GameObject max; //Max sender object
    private SendMax send; // Script
    // Start is called before the first frame update
    void Start() // Get camera and send script
    {
        cam = Camera.main;
        c = 0;
        max = GameObject.Find("MaxSender");
        send = max.GetComponent<SendMax>(); 
       
    }

    // Update is called once per frame
    void Update()
    {
        //Check if plane is being seen every 10 frames
        if (c % 10 == 0)
        {
            seen=IsInView();
        }


        if (seen) { // If plane is seen
            if (cc == 100) //If MIDI CC value is 100, we are controlling the key of the tune, so we send SendMax that we are controlling the key. This is because there is no MIDI CC value to change key, so we have to change it differently
            {
                send.key = true;

            }
            else if (cc == 101) // Key 2
            {
                send.key2 = true;
            }
            else if (cc == 102) { // Speed
                send.speed = true;
            }
            else // If not, it is the default MIDI CC change
            {
                send.MIDICC = cc;
            }
            sent = true;// This helps us so we send our data to SendMax only once
            
        }
        if (!seen && sent) { // We update everything when the plane just stopped being seen
            send.MIDICC = 0;
            sent = false;
        }
        c = (c + 1) % 160; //Every second
        
    }

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
