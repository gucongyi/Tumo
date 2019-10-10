using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragBrush : MonoBehaviour, IPointerDownHandler, IDragHandler,IEndDragHandler
{
    public Transform tranParent;
    public GameObject brush;
    public Text textShow;
    public RectTransform canvasRect;
    public Camera brushCamera;
    public RenderTexture renderTexture;
    public float radius = 0.5f;//半径
    const float blushWidth= 50;

    bool twoPoints = false;
    Vector2 lastPos;//上一个点
    Vector2 beginPos;//开始点
    int countBrush = 0;

    //限制创建出来的数量
    List<Vector2> listHave = new List<Vector2>();
    private void Start()
    {
        listHave.Clear();
        listHave.Add(Vector2.zero);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        twoPoints = false;
        StartCoroutine(CaptureScreenshot());
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        
        //SetPos(eventData);
        beginPos = GetPos(eventData);
        InstanceBrush(beginPos);
    }

    public void InstanceBrush(Vector2 localPoint)
    {
        bool isCreate = true;
        foreach (var item in listHave)
        {
            if (Vector2.Distance(item, localPoint) < radius* blushWidth/4)
            {
                //附近已经有了，不创建
                isCreate=false;
                break;
            }
        }
        if (!isCreate)
        {
            return;
        }
        GameObject  brushClone = Instantiate(brush);
        brushClone.transform.parent = tranParent;
        brushClone.transform.localScale = Vector3.one * radius;
        brushClone.transform.localPosition = localPoint;
        listHave.Add(localPoint);
        countBrush++;
        Debug.LogError($"countBrush:{countBrush}");
    }

    private Vector2 GetPos(PointerEventData eventData)
    {
        Vector2 mouseUguiPos;
        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, brushCamera, out mouseUguiPos);
        return mouseUguiPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //SetPos(eventData);
        //throw new System.NotImplementedException();
        Vector2 mouseUguiPos = GetPos(eventData);
        if (twoPoints && Vector2.Distance(mouseUguiPos, lastPos) > 2*radius)//如果两次记录的鼠标坐标距离大于一定的距离，开始记录鼠标的点
        {
            Vector2 pos = mouseUguiPos;
            float dis = Vector2.Distance(lastPos, pos);
            int segments = (int)(dis / radius);//计算出平滑的段数
            segments = segments < 1 ? 1 : segments;
            Vector2[] points = Beizier(beginPos, lastPos, pos, segments);//进行贝塞尔平滑
            for (int i = 0; i < points.Length; i++)
            {
                InstanceBrush(points[i]);
            }
            lastPos = pos;
            beginPos = points[points.Length - 2];
        }
        else
        {
            twoPoints = true;
            lastPos = mouseUguiPos;
        }
    }

    public Vector2[] Beizier(Vector2 start, Vector2 mid, Vector2 end, int segments)
    {
        float d = 1f / segments;
        Vector2[] points = new Vector2[segments - 1];
        for (int i = 0; i < points.Length; i++)
        {
            float t = d * (i + 1);
            points[i] = (1 - t) * (1 - t) * mid + 2 * t * (1 - t) * start + t * t * end;
        }
        List<Vector2> rps = new List<Vector2>();
        rps.Add(mid);
        rps.AddRange(points);
        rps.Add(end);
        return rps.ToArray();
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
    
}
