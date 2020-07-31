using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickTest : MonoBehaviour
{
    public StickRecognizer sr;
    Texture2D tex1, tex2, tex3;
    // Start is called before the first frame update
    void Start()
    {
        tex1 = Resources.Load("stick1.JPG") as Texture2D;
        tex2 = Resources.Load("stick2.JPG") as Texture2D;
        tex3 = Resources.Load("stick3.JPG") as Texture2D;
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyDown(KeyCode.Y) )
            sr.GetStickPosition(tex1);
        if( Input.GetKeyDown(KeyCode.H) )
            sr.GetStickPosition(tex2);
        if( Input.GetKeyDown(KeyCode.N) )
            sr.GetStickPosition(tex3);
    }
}
