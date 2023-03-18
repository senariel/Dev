using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLetterBox : MonoBehaviour
{
    public Vector2 resolutionRate = new(16.0f, 9.0f);

    // Start is called before the first frame update
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;
        float scaleHeight = ((float)Screen.width / (float)Screen.height) / (resolutionRate.x / resolutionRate.y);
        float scaleWidth = 1.00f / scaleHeight;

        if (scaleHeight < 1.0f)
        {
            rect.height =scaleHeight;
            rect.y = (1.00f - scaleHeight) / 2.0f;
        }
        else
        {
            rect.width = scaleWidth;
            rect.x = (1.00f - scaleWidth) / 2.0f;
        }

        camera.rect = rect;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}
