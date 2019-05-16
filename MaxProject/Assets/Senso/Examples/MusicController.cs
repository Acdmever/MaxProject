using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : Senso.Hand
{
    public Transform Palm;

    public Transform[] thumbBones;
    public Transform[] indexBones;
    public Transform[] middleBones;
    public Transform[] thirdBones;
    public Transform[] littleBones;

    //Vibration strength
    public byte strength = 5;
    public int x = 0;
    public int start = 0;
    public int frames = 0;
    public int h = 0;

    //Array of angles of each finger in the last prevFrames frames
    public static int prevFrames = 10;
    public float[,] angles = new float[5, prevFrames];
    

    //Standard deviation threshold
    public float std = 6.0f;

    //Which gloves will vibrate 1=right 2=left 3=both
    public int vibrateMode = 3; //0 to test other features

    //Mode 
    public int mode = 0;

    //What fingers will vibrate 0=thumb 1=index 2=middle 3=third 4=little
    public int vibrateFinger;

    //Duration of each vibration
    public ushort duration = 400;

    System.Random rnd = new System.Random();

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

        //My code



        vibrateFinger = rnd.Next(5);
        //Vibrate(1);
        //SensoHandsController c=base.m_sensoHandsController.Target as SensoHandsController;
        //c.SendVibro(base.HandType,Senso.EFingerType.Index, 2, 255);
    }

    //This is where all the controls for the vibration are
    public void Update()
    {
        if (frames == 0)
        {
            h = 0;
        }

        //Vibrate each finger
        /*if (x == 1) {
            VibrateAll();
            Debug.Log("Vibrating");
            System.Threading.Thread.Sleep(2000);
            Vibrate(1);
            Debug.Log("Vibrating");
            System.Threading.Thread.Sleep(700);
            Vibrate(2);
            Debug.Log("Vibrating");
            System.Threading.Thread.Sleep(700);
            Vibrate(3);
            Debug.Log("Vibrating");
            System.Threading.Thread.Sleep(700);
            Vibrate(4);
            Debug.Log("Vibrating");
            System.Threading.Thread.Sleep(700);
            Vibrate(0);
            Debug.Log("Vibrating");
            x = 0;
        }*/
        if (Input.GetKey(KeyCode.R))
        {
            x = 0;
        }

        if (x == 0 && h == 0 && mode != 0)
        {
            Vibrate((vibrateFinger));
            h = 1;
        }
        if (Input.GetKey(KeyCode.G))
        {
            vibrateFinger = 0;
            Vibrate(vibrateFinger);
            h = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            vibrateFinger = 1;
            Vibrate(vibrateFinger);
            h = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            vibrateFinger = 2;
            Vibrate(vibrateFinger);
            h = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            vibrateFinger = 3;
            Vibrate(vibrateFinger);
            h = 1;
        }
        if (Input.GetKey(KeyCode.F))
        {
            vibrateFinger = 4;
            Vibrate(vibrateFinger);
            h = 1;
        }

        //Vibrate all fingers
        if (Input.GetKey(KeyCode.Space))
        {
            VibrateAll();
            h = 1;
        }

        //Increase and Decrease Vibration Strength 0-255
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (strength < 10)
            {
                strength++;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (strength > 0)
            {
                strength--;
            }
        }


        //Change the mode
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (mode < 2)
            {
                mode++;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (mode > 0)
            {
                mode--;
            }
        }

        //Right Hand
        if (Input.GetKey(KeyCode.Q))
        {
            vibrateMode = 1;
        }
        //Left Hand
        if (Input.GetKey(KeyCode.W))
        {
            vibrateMode = 2;
        }
        //Both Hands
        if (Input.GetKey(KeyCode.E))
        {
            vibrateMode = 3;
        }
        frames++;
        frames = frames % 30;
    }

    public void OnGUI()
    {
        string s = "" + ((int)(1.0f / Time.smoothDeltaTime));
        GUI.Label(new Rect(0, 0, 100, 100), s);
    }

    //Vibrate one finger function
    public void Vibrate(int finger)
    {
        //Check the hand type (1=right 2=left)
        int h = (int)base.HandType;
        if (vibrateMode == 1 && h == 1)
        {
            switch (finger)
            {
                case 1: base.VibrateFinger(Senso.EFingerType.Thumb, duration, strength); break;
                case 2: base.VibrateFinger(Senso.EFingerType.Index, duration, strength); break;
                case 3: base.VibrateFinger(Senso.EFingerType.Middle, duration, strength); break;
                case 4: base.VibrateFinger(Senso.EFingerType.Third, duration, strength); break;
                default: base.VibrateFinger(Senso.EFingerType.Little, duration, strength); break;
            }
        }
        if (vibrateMode == 2 && h == 2)
        {
            switch (finger)
            {
                case 1: base.VibrateFinger(Senso.EFingerType.Thumb, duration, strength); break;
                case 2: base.VibrateFinger(Senso.EFingerType.Index, duration, strength); break;
                case 3: base.VibrateFinger(Senso.EFingerType.Middle, duration, strength); break;
                case 4: base.VibrateFinger(Senso.EFingerType.Third, duration, strength); break;
                default: base.VibrateFinger(Senso.EFingerType.Little, duration, strength); break;
            }
        }
        if (vibrateMode == 3)
        {
            switch (finger)
            {
                case 1: base.VibrateFinger(Senso.EFingerType.Thumb, duration, strength); break;
                case 2: base.VibrateFinger(Senso.EFingerType.Index, duration, strength); break;
                case 3: base.VibrateFinger(Senso.EFingerType.Middle, duration, strength); break;
                case 4: base.VibrateFinger(Senso.EFingerType.Third, duration, strength); break;
                default: base.VibrateFinger(Senso.EFingerType.Little, duration, strength); break;
            }
        }

    }

    //Vibrate all fingers in the chosen hands
    public void VibrateAll()
    {
        int h = (int)base.HandType;
        if (vibrateMode == 1 && h == 1)
        {

            base.VibrateFinger(Senso.EFingerType.Thumb, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Index, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Middle, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Third, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Little, duration, strength);

        }
        if (vibrateMode == 2 && h == 2)
        {

            base.VibrateFinger(Senso.EFingerType.Thumb, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Index, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Middle, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Third, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Little, duration, strength);

        }
        if (vibrateMode == 3)
        {

            base.VibrateFinger(Senso.EFingerType.Thumb, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Index, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Middle, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Third, duration, strength);
            base.VibrateFinger(Senso.EFingerType.Little, duration, strength);

        }

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


    //Calculate Standard Deviation
    private float stDev(int finger)
    {
        float r = 0.0f, mean = 0.0f;
        for (int i = 0; i < 10; ++i)
        {
            mean += angles[finger, i];

        }
        mean = mean / 10.0f;
        for (int i = 0; i < 10; ++i)
        {
            r += (float)System.Math.Pow(mean - angles[finger, i], 2.0);

        }
        r = r / 10.0f;

        r = (float)System.Math.Sqrt(r);
        Debug.Log(r);
        return r;

    }

   

    private void checkCommands(Senso.HandData aData)
    {
        //Track the angles from the last x frames, if program just started, the arrays are filled with the same starting angles

        updateAngles(aData);

        if (mode == 0) {
            raiseVolumeWFinger(aData, 2);
        }
        /*float dev = stDev(vibrateFinger);
        if ((dev >= std) /*|| ((dev >= std-5.0f) && (vibrateFinger==1) ))
        {
            x = 1;
            vibrateFinger = rnd.Next(5);
        }
        else if (dev >= std - 4.0f && vibrateFinger == 1)
        {
            x = 1;
            vibrateFinger = rnd.Next(5);

        }
        else
        {
            x = 0;
        }*/

    }

    private void updateAngles(Senso.HandData aData) {
        if (start == 0)
        {
            //When program just started
            start = 1;


            for (int i = 0; i < 5; i++)
            {
                float arr;
                switch (i)
                {
                    case 1: arr = aData.ThumbAngles.y; break;
                    case 2: arr = aData.IndexAngles.y; break;
                    case 3: arr = aData.MiddleAngles.y; break;
                    case 4: arr = aData.ThirdAngles.y; break;
                    default: arr = aData.LittleAngles.y; break;
                }

                for (int j = 0; j < prevFrames; ++j)
                    angles[i, j] = arr;
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                float arr;
                switch (i)
                {
                    case 1: arr = aData.ThumbAngles.y; break;
                    case 2: arr = aData.IndexAngles.y; break;
                    case 3: arr = aData.MiddleAngles.y; break;
                    case 4: arr = aData.ThirdAngles.y; break;
                    default: arr = aData.LittleAngles.y; break;
                }

                for (int j = 0; j < (prevFrames - 1); j++)
                {
                    angles[i, j] = angles[i, (j + 1)];

                }

                angles[i, (prevFrames - 1)] = arr;

            }
        }
    }

    private void raiseVolumeWFinger(Senso.HandData aData, int finger) {
        float fingerAngle;
        switch (finger)
        {
            case 1: fingerAngle = aData.ThumbAngles.x + 40.0f; break;
            case 2: fingerAngle = aData.IndexAngles.y; break;
            case 3: fingerAngle = aData.MiddleAngles.y; break;
            case 4: fingerAngle = aData.ThirdAngles.y; break;
            default: fingerAngle = aData.LittleAngles.y; break;
        }
        if (base.HandType == Senso.EPositionType.RightHand)
        {
            Debug.Log("Angle: " + fingerAngle);
        }
    }
}
