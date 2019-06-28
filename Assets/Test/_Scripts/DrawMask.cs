using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawMask : MonoBehaviour
{
    public Text textShow;
    public float radius = 0.5f;//半径
    public GameObject brush;
    bool startDraw = false;
    bool twoPoints = false;
    Vector2 lastPos;//最后一个点
    Vector2 penultPos;//倒数第二个点
    List<GameObject> brushesPool = new List<GameObject>(), activeBrushes = new List<GameObject>();//笔刷对象池

    public delegate void DrawHandler(Vector2 pos);
    public event DrawHandler onStartDraw;
    public event DrawHandler onEndDraw;
    public event DrawHandler drawing;

    [SerializeField]
    RenderTexture renderTexture;
    // Use this for initialization
    void Start()
    {
        Debug.Log("宽度：" + Screen.width + " 高度：" + Screen.height);

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

    }

    void GetInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startDraw = true;
            if (onStartDraw != null)
            {
                onStartDraw(VectorTransfer(Input.mousePosition));
            }
            penultPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            if (twoPoints && Vector2.Distance(Input.mousePosition, lastPos) > 0.5f)//如果两次记录的鼠标坐标距离大于一定的距离，开始记录鼠标的点
            {
                Vector2 pos = Input.mousePosition;
                float dis = Vector2.Distance(lastPos, pos);
                int segments = (int)(dis / radius);//计算出平滑的段数
                segments = segments < 1 ? 1 : segments;
                Vector2[] points = Beizier(penultPos, lastPos, pos, segments);//进行贝塞尔平滑
                for (int i = 0; i < points.Length; i++)
                {
                    InstanceBrush(VectorTransfer(points[i]));
                }
                if (drawing != null)
                {
                    drawing(VectorTransfer(Input.mousePosition));
                }
                lastPos = pos;
                penultPos = points[points.Length - 2];
            }
            else
            {
                twoPoints = true;
                lastPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (onEndDraw != null)
            {
                onEndDraw(VectorTransfer(Input.mousePosition));
            }
            startDraw = false;
            twoPoints = false;

            StartCoroutine(CaptureScreenshot());
        }


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
        textShow.text = "涂抹百分比："+ percent;

        Color color1 = texture2D.GetPixel((int)Input.mousePosition.x, (int)Input.mousePosition.y);

        Debug.LogError("Input.mousePosition.x:" + Input.mousePosition.x + "Input.mousePosition.y:" + Input.mousePosition.y + "color1.a:" + color1.a);

    }


    private void OnPostRender()
    {
        InitBrushes();
    }

    void InitBrushes()
    {
        for (int i = 0; i < activeBrushes.Count; i++)
        {
            activeBrushes[i].SetActive(false);
            brushesPool.Add(activeBrushes[i]);
        }
        activeBrushes.Clear();
    }

    void InstanceBrush(Vector2 pos)
    {
        GameObject brushClone;
        if (brushesPool.Count > 0)
        {
            brushClone = brushesPool[brushesPool.Count - 1];
            brushesPool.RemoveAt(brushesPool.Count - 1);
        }
        else
        {
            brushClone = Instantiate(brush, pos, Quaternion.identity);
        }
        brushClone.transform.position = pos;

        brushClone.transform.localScale = Vector3.one * radius;
        brushClone.SetActive(true);
        activeBrushes.Add(brushClone);
    }

    /// <summary>
    /// 贝塞尔平滑
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="mid">中点</param>
    /// <param name="end">终点</param>
    /// <param name="segments">段数</param>
    /// <returns></returns>
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

    Vector2 VectorTransfer(Vector2 point)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(point.x, point.y, 0));
    }
}
