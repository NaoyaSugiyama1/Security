using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// https://commons.wikimedia.org/wiki/File:Go_sign.svg
// https://commons.wikimedia.org/wiki/File:Bi-lingual_Canadian_stop_sign.svg
// https://commons.wikimedia.org/wiki/File:Attention_Sign.svg

public class Signal : MonoBehaviour
{
    public Image imageGo;
    public Image imageSlow;
    public Image imageStop;
    public Text durationText;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GestureManager gm = gameObject.GetComponentInParent<GestureManager>();
        imageGo.enabled = false;
        imageSlow.enabled = false;
        imageStop.enabled = false;
        switch( gm.currentGesture )
        {
            case GestureManager.GestureType.GO:
                imageGo.enabled = true;
                break;
            case GestureManager.GestureType.SLOW:
                imageSlow.enabled = true;
                break;
            case GestureManager.GestureType.STOP:
                imageStop.enabled = true;
                break;
        }
        durationText.text = String.Format("{0:0.0}", gm.duration);
    }
}
