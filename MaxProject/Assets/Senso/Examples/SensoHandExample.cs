using UnityEngine;
using System.Collections;
using System;
using System.Linq;
public class SensoHandExample : Senso.Hand {

    public Transform Palm;

    public Transform[] thumbBones;
    public Transform[] indexBones;
    public Transform[] middleBones;
    public Transform[] thirdBones;
    public Transform[] littleBones;

    
    public byte strength = 5;//Vibration strength
    public int start = 0;//When script was just initialized


    
    public static int prevFrames = 30;
    public float[,] angles = new float[5, prevFrames];//Array of angles of each finger in the last prevFrames frames



    public int value = 127, t=0;//Value to change in MIDICC Message     Timer value, this helps so that there can be a command every second.
    public bool control,timer=false,change=false,stop=false;//Whether the value is being changed
    public float min=0f,max=127f;//Min,max value range of MIDICC values


    private Quaternion[][] fingerInitialRotations;

    public new void Start()
    {
        base.Start();
        fingerInitialRotations = new Quaternion[5][];
        for (int i = 0; i < 5; i++)
        {
            Transform[] arr;
            switch (i)
            {
                case 1: arr = indexBones; break;
                case 2: arr = middleBones; break;
                case 3: arr = thirdBones; break;
                case 4: arr = littleBones; break;
                default: arr = thumbBones; break;
            }
            fingerInitialRotations[i] = new Quaternion[arr.Length];
            for (int j = 0; j < arr.Length; ++j)
                fingerInitialRotations[i][j] = arr[j].localRotation;
        }

        
    }

    public void Update()
    {
        if (timer) { //If timer is on, add to t 
            t++;
        }
        if (t > 160) { // When timer reaches 1 second, stop timer
            timer = false;
            t = 0;
        }

        
    }

    public void OnGUI()
    {
        string s = "" + ((int)(1.0f / Time.smoothDeltaTime));
        GUI.Label(new Rect(0, 0, 100, 100), s);
    }

    




    //Functions from sample project, update hand positions
    public override void SetSensoPose(Senso.HandData aData)
    {
        Palm.localRotation = /*(Quaternion.Inverse(wq) */ aData.PalmRotation;
        Palm.localPosition = aData.PalmPosition;
        //base.VibrateFinger(Senso.EFingerType.Thumb, 50, 1);
        //Fingers
        if (aData.AdvancedThumb)
        {
            //Quaternion thumbQ = new Quaternion(aData.ThumbQuaternion.y / 3.0f, aData.ThumbQuaternion.x, -aData.ThumbQuaternion.z, aData.ThumbQuaternion.w);
            Quaternion thumbQ = new Quaternion(aData.ThumbQuaternion.z, aData.ThumbQuaternion.y, -aData.ThumbQuaternion.x / 3.0f, aData.ThumbQuaternion.w);
            thumbBones[0].localRotation = fingerInitialRotations[0][0] * thumbQ;
            thumbBones[1].localRotation = fingerInitialRotations[0][1] * Quaternion.Euler(0.0f, 0.0f, -aData.ThumbQuaternion.x / 3.0f);
            thumbBones[2].localRotation = fingerInitialRotations[0][2] * Quaternion.Euler(0.0f, 0.0f, aData.ThumbBend * Mathf.Rad2Deg);



        }
        else // old method
            setFingerBones(ref thumbBones, aData.ThumbAngles, Senso.EFingerType.Thumb);
        setFingerBones(ref indexBones, aData.IndexAngles, Senso.EFingerType.Index);
        setFingerBones(ref middleBones, aData.MiddleAngles, Senso.EFingerType.Middle);
        setFingerBones(ref thirdBones, aData.ThirdAngles, Senso.EFingerType.Third);
        setFingerBones(ref littleBones, aData.LittleAngles, Senso.EFingerType.Little);


        //MIDI CC function
        checkCommands(aData);
    }

    private void setFingerBones(ref Transform[] bones, Vector2 angles, Senso.EFingerType fingerType)
    {
        if (fingerType == Senso.EFingerType.Thumb) setThumbBones(ref bones, ref angles);
        else
        {
            var fingerIdx = (int)fingerType;
            var vec = new Vector3(-angles.y, 0, angles.x);
            if (vec.x > 0.0f)
            {
                vec.z += fingerInitialRotations[fingerIdx][0].eulerAngles.z;
                bones[0].localEulerAngles = vec;
            }
            else
            {
                var v = vec;
                v.x /= 3.0f;
                bones[0].localRotation = fingerInitialRotations[fingerIdx][0];
                bones[0].Rotate(v);
                vec.z = 0.0f;

                vec.x /= 1.5f;
                for (int j = 1; j < bones.Length; ++j)
                {
                    bones[j].localRotation = fingerInitialRotations[fingerIdx][j];
                    bones[j].Rotate(vec);
                }
            }
        }
    }

    private static void setThumbBones(ref Transform[] bones, ref Vector2 angles)
    {
        bones[0].localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        float t = angles.x;
        angles.x = -angles.y;
        angles.y = t;

        angles.y += 30.0f;
        bones[0].Rotate(angles);
    }


    //Calculate Standard Deviation of the angles array of a given finger
    private float stDev(int finger)
    {
        float r = 0.0f, mean = 0.0f,f=(float)prevFrames;
        for (int i = 0; i < prevFrames; ++i)
        {
            mean += angles[finger, i];

        }
        mean = mean / f;
        for (int i = 0; i < prevFrames; ++i)
        {
            r += (float)System.Math.Pow(mean - angles[finger, i], 2.0);

        }
        r = r / f;
        
        r = (float)System.Math.Sqrt(r);
        return r;

    }


    //Update angles, transform finger angles to MIDICC range values to send to Max patch
    private void checkCommands(Senso.HandData aData)
    {
        updateAngles(aData);
        changeValWHand(aData);//Use hand rotation and finger angles
        //changeValWFinger(aData,2);//Uses angle of one finger
        changeSettingWHand(aData); // Swipe movement to change song
       
    }


    //Updates angles array with current finger angles
    //Track the angles from the last x frames, if program just started, the arrays are filled with the same starting angles
    private void updateAngles(Senso.HandData aData)
    {
        if (start == 0)//When program just started
        {
            
            start = 1;

            //We make all values of each finger equal
            for (int i = 0; i < 5; i++)
            {
                float arr;
                switch (i) //Get the latest value
                {
                    case 1: arr = aData.ThumbAngles.y; break;
                    case 2: arr = aData.IndexAngles.y; break;
                    case 3: arr = aData.MiddleAngles.y; break;
                    case 4: arr = aData.ThirdAngles.y; break;
                    default: arr = aData.LittleAngles.y; break;
                }

                for (int j = 0; j < prevFrames; ++j) //Copy it to all array spaces of the current finger
                    angles[i, j] = arr;
            }
        }
        else
        {

            //Afterwards, we only dispose the oldest value in the array, move all value back by 1, and add the newest value at the end
            for (int i = 0; i < 5; i++)
            {
                float arr;
                switch (i) //Get the latest value
                {
                    case 1: arr = aData.ThumbAngles.y; break; //angles[1]
                    case 2: arr = aData.IndexAngles.y; break;//angles[2]
                    case 3: arr = aData.MiddleAngles.y; break;//angles[3]
                    case 4: arr = aData.ThirdAngles.y; break;//angles[4]
                    default: arr = aData.LittleAngles.y; break;//angles[0]
                }

                for (int j = 0; j < (prevFrames - 1); j++) //Move values back
                {
                    angles[i, j] = angles[i, (j + 1)];

                }

                angles[i, (prevFrames - 1)] = arr; //Add newest value

            }
        }
    }

    //Change MIDI CC Values by the angle of one finger
    private void changeValWFinger(Senso.HandData aData, int finger)
    {
        float fingerAngle,angMin=50,angMax=-20;//Min and max depending on the angle of the finger

        switch (finger) // Get the angle value dependin on the finger chosen
        {
            case 1: fingerAngle = aData.ThumbAngles.x + 40.0f; break;
            case 2: fingerAngle = aData.IndexAngles.y; break;
            case 3: fingerAngle = aData.MiddleAngles.y; break;
            case 4: fingerAngle = aData.ThirdAngles.y; break;
            default: fingerAngle = aData.LittleAngles.y; break;
        }

        //Scale the values to be 0-127 (MIDI CC range)
        value = (int)scale(fingerAngle, angMin, angMax, min, max);
        
    }

    //Change MIDI CC Values by the angle of all fingers except thumbs
    private void changeValWHand(Senso.HandData aData) {
        float angMin = 0f, angMax = 70f; //Min and max finger angle values
        float palm = aData.PalmRotation.eulerAngles.z;//Palm angle rotation 
        float[] fingers = new float[4];//Finger angles

        
        //Get the current angles of fingers, thumbs are ignored
        fingers[0] = aData.IndexAngles.y;
        fingers[1] = aData.MiddleAngles.y;
        fingers[2] = aData.ThirdAngles.y;
        fingers[3] = aData.LittleAngles.y;

        float fingerMean = fingers.Average(); //Average the angles
        
        if (palm > 160f && palm < 230f) // Angle range of the palm
        {
            value = (int)scale(fingerMean, angMin, angMax, min, max); // Scale the angle mean to be 0-127 (MIDI CC range)
            //Debug.Log(value);
            control = true; // Toggle control variable, which SendMax will read to know that the user is controlling the MIDI CC values

        }
        else {
            //Debug.Log("Not in control");
            if (control) {
                timer = true; // So that there are no changes by mistake after just leaving control hand position, we toggle the timer
            }
            control = false; //Make control variable false, so that SendMax knows that the user is no longer controlling MIDI CC values
        }

    }

    //Swipe movement to change song
    private void changeSettingWHand(Senso.HandData aData)
    {
        float palm = aData.PalmRotation.eulerAngles.z;//Palm angle rotation 
        float[] stdFingers = new float[4];//Finger deviations
        int[] fingers = { 0, 2, 3, 4 };//Thumbs are ignored (1)


        for (int i=0; i<4;i++) { //Calculate the standard deviations from all fingers 
            stdFingers[i] = stDev(fingers[i]);
        }
        


        float fingerMean = stdFingers.Average(); //Get the mean of all standard deviations

        //Change song
        if (fingerMean > 19f && !timer && palm > 250f && palm < 310 /*&& constant(fingers)*/) { // If standard deviation mean meets a threshold, the timer is off, and the palm angle is between a range

            change = true; //Toggle change so that SendMax sends bang to change song
            timer = true; //Toggle timer
            //Debug.Log("Change");

        }

        //Stop motion, currently toggling changing effects with values on/off
        if (fingerMean > 9f && !timer && (palm > 350f || palm < 30f) /*&& constant(fingers)*/) // If standard deviation mean meets a threshold, the timer is off, and the palm angle is between a range
        {
            stop = true; //Toggle change so that SendMax sends bang to toggle MIDI CC changing by the wave values
            timer = true; //Toggle timer
            //Debug.Log("Stop");

        }
        
        

    }

    //Scale values to different range function
    private float scale(float n, float oldMin, float oldMax, float newMin, float newMax) {
        float r;
        r = ((n - oldMin) * (newMax-newMin) / (oldMax - oldMin))+newMin;
        r = Mathf.Max(newMin, r);
        r = Mathf.Min(newMax, r);
        return r;
    }

    //Check whether movement is on one direction (not working properly, angle decalibration in gloves when moving makes it unreliable)
    private bool constant(int[] fingers, int sign) {

        int rowLength = angles.GetLength(0);
        int colLength = angles.GetLength(1);

        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                Debug.Log(string.Format("{0} ", angles[i, j]));
            }
            Debug.Log(Environment.NewLine + Environment.NewLine);
        }
        foreach (int finger in fingers) {
            float curr = angles[finger, 0]+100f;
            int tol = 0;
            for (int i = 2; i < prevFrames; i=i+2) {
                float next = angles[finger,i] + 100f;
                if (Math.Sign(next - curr) == -(sign)) {
                    tol++;

                    if (tol > 3)
                    {
                        Debug.Log((next - curr));
                        Debug.Log(-sign);

                        return false;
                    }
                }
                curr = next;
            }
        }
        Debug.Log(sign);
        return true;
    }
}
