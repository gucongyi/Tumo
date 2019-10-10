using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasAndBlushCameraManager : MonoBehaviour
{
    public RectTransform RectCanvasShowUI;
    public RectTransform RectCanvasRender;
    public RectTransform RectCanavaseOrigin;
    public Camera CameraRenderer;
    float sizeOrthCamera;
    // Start is called before the first frame update
    void Start()
    {
        //用来做适配
        RectCanvasRender.localScale = RectCanvasShowUI.localScale;
        float currCanvasLocalScaleX = RectCanvasShowUI.localScale.x;
        float originCanvasLocalScaleX = RectCanavaseOrigin.localScale.x;
        float radioCanvas = currCanvasLocalScaleX / originCanvasLocalScaleX;
        sizeOrthCamera = CameraRenderer.orthographicSize;
        float currSizeOrthCamera = sizeOrthCamera * radioCanvas;
        CameraRenderer.orthographicSize = currSizeOrthCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
