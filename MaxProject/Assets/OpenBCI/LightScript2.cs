using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

//This is the script for the right lamp, it is different from the other lamp because different waves are used
public class LightScript2 : MonoBehaviour
{
    private int c, fps = 120; //Frame count
    private Color c1, c2; //Since it is color interpolation, we need 2 colors
    private float i1, i2; //We are also using interpolation between the 2 light intensities
    private GameObject bci;
    private OpenBCIData bcidata;
    // Start is called before the first frame update
    void Start()
    {
        //Get the OpenBCI object and its script
        bci = GameObject.Find("OpenBCI");
        bcidata = bci.GetComponent<OpenBCIData>();

        //Initialize color and intesity values
        c1 = Color.white;
        c2 = Color.white;

        i1 = 1f;
        i2 = 1f;

        c = 0;//Initilaize frame count
    }

    // Update is called once per frame
    void Update()
    {
        if (c == 0)
        {
            //We calculate every second
            changeLight();

        }
        updateLight();
        c = (c + 1) % fps;
    }

    
    //This function gets the new color and intesity values
    private void changeLight()
    {
        bcidata = bci.GetComponent<OpenBCIData>();

        //Scaling amplitude values to 0-1 for new color value
        float g = scale((float)bcidata.beta, 0.8f, 1.7f, 0f, 1f);
        float b = scale((float)bcidata.lowbeta, 0.8f, 1.9f, 0f, 1f);
        float r = scale((float)bcidata.highbeta, 0.8f, 1.7f, 0f, 1f);

        //Assigning new interpolation values
        c1 = c2;
        c2 = new Color(r, g, b, 1.0f);
        i1 = i2;
        i2 = scale((float)bcidata.betaR, 0f, 2f, 0f, 2.5f);//Scaling intesity value

    }
    //This function interpolates between the 2 values of color and intesity
    private void updateLight()
    {
        GetComponent<Light>().color = Color.Lerp(c1, c2, (float)c / (float)fps);
        GetComponent<Light>().intensity = i1 + (i2 - i1) * (float)c / (float)fps;
    }

    //Scale values to different range function
    private float scale(float n, float oldMin, float oldMax, float newMin, float newMax)
    {
        float r;
        r = ((n - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;
        r = Mathf.Max(newMin, r);
        r = Mathf.Min(newMax, r);
        return r;
    }
}