using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragBrush : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public Transform tranParent;
    public GameObject brush;
    public Text textShow;
    public RectTransform canvasRect;
    public Camera brushCamera;
    public RenderTexture renderTexture;
    public float radius = 0.5f;//半径
    GameObject brushClone;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (brushClone == null)
        {
            brushClone = Instantiate(brush);
        }
        brushClone.transform.parent = tranParent;
        brushClone.transform.localScale = Vector3.one* radius;
        SetPos(eventData);
    }

    private void SetPos(PointerEventData eventData)
    {
        Vector2 mouseUguiPos;
        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, brushCamera, out mouseUguiPos);
        brushClone.transform.localPosition = mouseUguiPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetPos(eventData);
        //throw new System.NotImplementedException();
    }


    IEnumerator CaptureScreenshot()
    {
        //只在每一帧渲染完成后才读取屏幕信息
        yield return new WaitForEndOfFrame();

        //Texture2D m_texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //// 读取Rect范围内的像素并存入纹理中
        //m_texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //// 实际应用纹理
        //m_texture.Apply();
        int width = renderTexture.width;
        int height = renderTexture.height;
        Debug.Log("宽：" + width + " 高：" + height);
        Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();

        int count = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color color = texture2D.GetPixel(i, j);
                if (color.a == 0)
                {
                    count++;
                }
            }
        }
        float percent = 1f - (float)count / (width * height);
        Debug.LogError("count:" + count + "percent:" + percent);
        textShow.text = "涂抹百分比：" + percent;

        Color color1 = texture2D.GetPixel((int)Input.mousePosition.x, (int)Input.mousePosition.y);

        Debug.LogError("Input.mousePosition.x:" + Input.mousePosition.x + "Input.mousePosition.y:" + Input.mousePosition.y + "color1.a:" + color1.a);

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(CaptureScreenshot());
        }
    }
}
