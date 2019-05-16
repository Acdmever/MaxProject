using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanesController : MonoBehaviour
{
    public List<GameObject> children; //All the planes that are the children of this "Planes" object
    private int c, curr,length,t; //c: frame count  curr: current plane being shown     length: amount of planes in list    t:Counter for when a plane is being seen, but no control of MIDI CC is happening
    private GameObject hands; //Gloves
    private SensoHandExample handData;//Script run in the gloves object
    private List<Material> mats; // Different materials (easier for the user to see what is the current interaction with a plane)
    // Start is called before the first frame update
    void Start()
    {
        mats = new List<Material>();
        mats.Add(GameObject.Find("Materials/C1").GetComponent<Renderer>().material); // Default material of the plane
        mats.Add(GameObject.Find("Materials/C2").GetComponent<Renderer>().material); // When the plane is at the center of the HMD's view
        mats.Add(GameObject.Find("Materials/C3").GetComponent<Renderer>().material); // When the plane is also being used to control MIDI CC effects


        children = new List<GameObject>();
        hands = GameObject.Find("[CameraRig]/Right Hand Container");
        handData = hands.GetComponent<SensoHandExample>();
        c = 0; //Timer for switching planes
        t = 0; //Counter for when a plane is being seen, but no control of MIDI CC is happening
        curr = 0; //Current plane being shown
        Transform[] tr = gameObject.GetComponentsInChildren<Transform>();

        //Adding planes to children list
        foreach (Transform c in tr) {
            if (c.name.StartsWith("Plane") && c!=tr[0]) { // We ignore the first object, because it is the parent object

                children.Add(c.gameObject); // Add object if it has "Plane" in the name

                // Make all planes invisible
                c.gameObject.GetComponent<Renderer>().enabled = false;
                c.gameObject.GetComponentInChildren<Canvas>().enabled = false;


            }
        }
        length=children.Count; //Length of list
    }

    // Update is called once per frame
    void Update()
    {

        if (c % 10 == 0) // We do this just so we don't update the look of the plane every frame but every 10
        {
            if (!planeBeingSeen() && !ccControlling()) // If plane is not in center of view and not being using gloves to control MIDI CC
            {
                foreach (GameObject child in children)
                    child.GetComponent<Renderer>().material = mats[0];
            }
            else if (ccControlling() && planeBeingSeen()) // If plane in view, and controlling MIDI CC
            {
                foreach (GameObject child in children)
                    child.GetComponent<Renderer>().material = mats[2];
            }
            else if (planeBeingSeen()) //If plane is only being seen
            {
                foreach (GameObject child in children)
                    child.GetComponent<Renderer>().material = mats[1];

            }
            

        }
        if (c == 0) { // Every 3 seconds (HMD fps is 160)
            if (planeBeingSeen() && !ccControlling()) { // When plane is being seen, but there is no control, it can take twice as much time until the planes are switched
                
                t++; 
            }
            
            if ((t >= 2 && !ccControlling()) || !planeBeingSeen()) //If plane is not being seen OR timer has already gone twice, reset timers and switch planes
            {
                updatePlanes();
                t = 0;
            }
            

        }
        c = (c + 1) % 450; // Every ~3 seconds (HMD fps is 186)
    }

    //Function that switches planes
    private void updatePlanes()
    {
        //Disable current plane and its canvas
        children[curr].GetComponent<Renderer>().enabled = false;
        children[curr].GetComponentInChildren<Canvas>().enabled = false;
        curr = (curr + 1) % length;

        //Enable next plane and its canvas
        children[curr].GetComponent<Renderer>().enabled = true;
        children[curr].GetComponentInChildren<Canvas>().enabled = true;
    }

    //Check is a planes is being seen by the HMD
    private bool planeBeingSeen() {
        
        return children[curr].GetComponent<TestVision>().IsInView(); ;
    }

    //Check if user is currently controlling MIDI CC effects
    private bool ccControlling() {
        return handData.control;
    }
}
