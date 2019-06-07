using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanesController : MonoBehaviour
{
    public List<GameObject> children; //All the planes that are the children of this "Planes" object
    public List<GameObject> planes; //Other "Planes" objects
    private int c, curr,length,t; //c: frame count  curr: current plane being shown     length: amount of planes in list    t:Counter for when a plane is being seen, but no control of MIDI CC is happening
    private GameObject hands, max; //Gloves;    Max sender
    private SensoHandExample handData;//Script run in the gloves object
    private SendMax sendmax;// SendMax script
    private List<Material> mats; // Different materials (easier for the user to see what is the current interaction with a plane)
    public bool seen, active, sent; // Whether the user is looking at this specific group of planes;  If the user is controlling the effects with the Senso Gloves; If MIDI CC number was already sent to SendMax
    
    
    // Start is called before the first frame update
    void Start()
    {
        //Load up materials for planes
        mats = new List<Material>();
        mats.Add(GameObject.Find("Materials/C1").GetComponent<Renderer>().material); // Default material of the plane
        mats.Add(GameObject.Find("Materials/C2").GetComponent<Renderer>().material); // When the plane is at the center of the HMD's view
        mats.Add(GameObject.Find("Materials/C3").GetComponent<Renderer>().material); // When the plane is also being used to control MIDI CC effects

        //Get SendMax
        max = GameObject.Find("MaxSender");
        sendmax=max.GetComponent<SendMax>();


        active = true; //Start with using OpenBCI data to change 

        children = new List<GameObject>();
        hands = GameObject.Find("[CameraRig]/Right Hand Container");
        handData = hands.GetComponent<SensoHandExample>();
        c = 0; //Timer for switching planes
        t = 0; //Counter for when a plane is being seen, but no control of MIDI CC is happening
        curr = 0; //Current plane being shown


        //Adding planes to children list
        Transform[] tr = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform c in tr) {
            if (c.name.StartsWith("Plane") && c!=tr[0]) { // We ignore the first object, because it is the parent object

                children.Add(c.gameObject); // Add object if it has "Plane" in the name

                // Make all planes invisible
                c.gameObject.GetComponent<Renderer>().enabled = false;
                c.gameObject.GetComponentInChildren<Canvas>().enabled = false;


            }
        }
        length=children.Count; //Length of list

        //Adding the other plane groups
        foreach (GameObject obj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (obj.name.Contains("Planes"))
            {
                planes.Add(obj);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (active)
        {
            if (c % 10 == 0) // We do this just so we don't update the look of the plane every frame but every 10
            {

                updateMaterials();
                sendMax();

            }
            if (c == 0)// Every 3 seconds (HMD fps is 160)
            {
                planeLoop();

            }
            c = (c + 1) % 450; // Every ~3 seconds (HMD fps is 160)
        }
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
    public bool planeBeingSeen() {
         if (!children[curr].GetComponent<TestVision>().IsInView()) //If the plane is not being seen, it will always be false
            return false;
        foreach (GameObject plane in planes) { // If there is another Planes object being seen,both will be false, in order not to send different MIDI CC messages at the same time
            if (plane.GetComponent<PlanesController>().seen) {
                return false;
            }
        }
        return true; //If plane is the only being seen, then it returns true
    }

    //Check if user is currently controlling MIDI CC effects
    private bool ccControlling() {
        return handData.control;
    }

    public void switchPlanes() //Deactivate/Activate the rendering of planes
    {
        active = !active;
        children[curr].GetComponent<Renderer>().enabled = active;
        children[curr].GetComponentInChildren<Canvas>().enabled = active;
    }

    private void updateMaterials()
    {
        if (ccControlling() && planeBeingSeen()) // If plane in view, and controlling MIDI CC
        {
            foreach (GameObject child in children)
                child.GetComponent<Renderer>().material = mats[2];
        }
        else if (planeBeingSeen()) //If plane is only being seen
        {
            foreach (GameObject child in children)
                child.GetComponent<Renderer>().material = mats[1];

        }
        else //Else, it has default material 
        {
            foreach (GameObject child in children)
                child.GetComponent<Renderer>().material = mats[0];
        }
    }

    private void planeLoop() {
        
        if (planeBeingSeen() && !ccControlling())
        { // When plane is being seen, but there is no control, it can take twice as much time until the planes are switched

            t++;
        }

        if ((t >= 2 && !ccControlling()) || !planeBeingSeen()) //If plane is not being seen OR timer has already gone twice, reset timers and switch planes
        {
            updatePlanes();
            t = 0;
        }

    }

    private void sendMax() {
        if (ccControlling() && !sent && planeBeingSeen()) { //If user is controlling with the Gloves, and looking at a plane for the first frame
            TestVision plane = children[curr].GetComponent<TestVision>();
            int cc = plane.cc;
            if (cc == 100) //If MIDI CC value is 100, we are controlling the key of the tune, so we send SendMax that we are controlling the key. This is because there is no MIDI CC value to change key, so we have to change it differently
            {
                sendmax.key = true;

            }
            else if (cc == 101) // Key 2
            {
                sendmax.key2 = true;
            }
            else if (cc == 102)
            { // Speed
                sendmax.speed = true;
            }
            else // If not, it is the default MIDI CC change
            {
                sendmax.MIDICC = cc;
            }
            sent = true;// This helps us so we send our data to SendMax only once
        }
        if ((!ccControlling() || !planeBeingSeen()) && sent ) // If user stops looking at a plane, or controlling with gloves for the first frame
        {
            TestVision plane = children[curr].GetComponent<TestVision>();
            int cc = plane.cc;
            if (cc == 100) //If MIDI CC value is 100, we are controlling the key of the tune, so we send SendMax that we are controlling the key. This is because there is no MIDI CC value to change key, so we have to change it differently
            {
                sendmax.key = false;

            }
            else if (cc == 101) // Key 2
            {
                sendmax.key2 = false;
            }
            else if (cc == 102)
            { // Speed
                sendmax.speed = false;
            }
            else // If not, it is the default MIDI CC change
            {
                sendmax.MIDICC = 0;
            }
            sent = false;
        }

    }
}
