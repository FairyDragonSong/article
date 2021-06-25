using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle
{
    public Vector3 curPos;
    public Vector3 prePos;
    public Vector3 speed;
    public float mass = 1;
    public Vector3 initPos;
}

public class Spring : MonoBehaviour
{
    private Vector3 hookPos1;
    private Vector3 hookPos2;

    CoroutineQueue queue;

    public int width = 2;
    public int height = 2;

    public GameObject[] gos;

    public List<particle> infos = new List<particle>();

    /// <summary>
    /// 阻尼
    /// </summary>
    public float damp = 1;

    /// <summary>
    /// 弹力
    /// </summary>
    public float elastic = 0.5f;

    /// <summary>
    /// 惯性
    /// </summary>
    public float inertia = 1;

    /// <summary>
    /// 弹簧初始长度
    /// </summary>
    public float L0 = 3;

    /// <summary>
    /// 弹簧的形变范围
    /// </summary>
    public float Lmin = 1f;
    public float Lmam = 5f;

    /// <summary>
    /// 临界值
    /// </summary>
    public float Lcritical = 0.5f;

    /// <summary>
    /// 重力加速度
    /// </summary>
    public float acceleration = 9.8f;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < gos.Length; i++)
        {
            particle p = new particle();
            p.curPos = gos[i].transform.position;
            p.prePos = gos[i].transform.position;
            p.initPos = gos[i].transform.localPosition;
            infos.Add(p);
            if (i == gos.Length - 1)
            {
                hookPos2 = gos[i].transform.position;
            }
            else if (i == gos.Length - width)
            {
                hookPos1 = gos[i].transform.position;
            }
        }

        queue = new CoroutineQueue(2, StartCoroutine);
        queue.Run(DelayAddForce());
    }

    IEnumerator DelayAddForce()
    {
        yield return new WaitForSeconds(1);

        gos[0].transform.Translate(-Vector3.right * 0.1f);
        gos[1].transform.Translate(Vector3.right * 0.1f);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "弹"))
        {
            queue.Run(DelayAddForce());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ResetPos()
    {
        for (int i = 0; i < gos.Length; i++)
        {
            gos[i].transform.localPosition = infos[i].initPos;
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < gos.Length; i++)
        {
            List<Tuple<int, double>> aroundList = new List<Tuple<int, double>>();

            // 把当前点四周围的点点当做弹簧一一链接。
            SingleAdd(aroundList, new Tuple<int, double>(i - width, L0));
            if ((i + 1) % width != 1)
            {
                SingleAdd(aroundList, new Tuple<int, double>(i - width - 1, L0 * Math.Sqrt(2)));
                SingleAdd(aroundList, new Tuple<int, double>(i + width - 1, L0 * Math.Sqrt(2)));
                SingleAdd(aroundList, new Tuple<int, double>(i - 1, L0));
            }
            if ((i + 1) % width != 0)
            {
                SingleAdd(aroundList, new Tuple<int, double>(i - width + 1, L0 * Math.Sqrt(2)));
                SingleAdd(aroundList, new Tuple<int, double>(i + width + 1, L0 * Math.Sqrt(2)));
                SingleAdd(aroundList, new Tuple<int, double>(i + 1, L0));
            }
            SingleAdd(aroundList, new Tuple<int, double>(i + width, L0));

            for (int j = 0; j < aroundList.Count; j++)
            {
                if (aroundList[j].Item1 >= 0 && aroundList[j].Item1 < gos.Length)
                {
                    GameObject go1 = gos[i];
                    particle info1 = infos[i];
                    GameObject go2 = gos[aroundList[j].Item1];
                    particle info2 = infos[aroundList[j].Item1];
                    SpringSimulation(go1, info1, go2, info2, aroundList[j].Item2);
                    
                }
            }
        }

        HookPoints();
    }

    public void HookPoints()
    {
        infos[gos.Length - 1].prePos = infos[gos.Length - 1].curPos = hookPos2;
        gos[gos.Length - 1].transform.position = hookPos2;
        infos[gos.Length - width].prePos = infos[gos.Length - width].curPos = hookPos1;
        gos[gos.Length - width].transform.position = hookPos1;
    }

    public void SingleAdd<T>(List<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
        }
    }

    void SpringSimulation(GameObject go1, particle info1, GameObject go2, particle info2, double iNormalLen)
    {
        CheckInertia(go1, info1);
        CheckInertia(go2, info2);
        CheckSpring(go1, info1, go2, info2, iNormalLen);
    }

    private void CheckInertia(GameObject go, particle info)
    {
        Vector3 realMoveDis = go.transform.position - info.prePos;
        Vector3 offsetVector3 = realMoveDis * inertia * 0.9f;

        // go.transform.position = offsetVector3 + info.curPos;
        info.curPos = offsetVector3 + info.curPos;
    }

    private void CheckSpring(GameObject go1, particle info1, GameObject go2, particle info2, double iNormalLen)
    {
        Vector3 delLen = go1.transform.position - go2.transform.position;

        Vector3 speed1 = (go1.transform.position - info1.prePos) / Time.deltaTime;
        Vector3 speed2 = (go2.transform.position - info2.prePos) / Time.deltaTime;

        if (delLen.magnitude < Lmin)
        {
            // // 弹簧异常 简单当做反弹处理
            speed1 = -speed1;
            speed2 = -speed2;

        }
        else
        {
            // 阻尼力和弹力计算
            Vector3 elasF1;
            Vector3 elasF2;

            elasF1 = -(delLen.magnitude - (float)iNormalLen) * elastic * delLen / delLen.magnitude - damp * (speed1 - speed2);
            elasF2 = -elasF1;

            // 自身重力
            elasF1 = elasF1 + acceleration * info1.mass * Vector3.down;
            elasF2 = elasF2 + acceleration * info2.mass * Vector3.down;

            // 加速度
            Vector3 a1 = elasF1 / info1.mass;
            Vector3 a2 = elasF2 / info2.mass;

            speed1 += a1 * Time.deltaTime;
            speed2 += a2 * Time.deltaTime;

            info1.speed = speed1;
            info2.speed = speed2;
        }

        Vector3 offset1 = speed1 * Time.deltaTime;
        Vector3 offset2 = speed2 * Time.deltaTime;


        go1.transform.position = info1.curPos + offset1;
        go2.transform.position = info2.curPos + offset2;

        // 数据保存
        info1.prePos = info1.curPos;
        info2.prePos = info2.curPos;
        info1.curPos = go1.transform.position;
        info2.curPos = go2.transform.position;
    }
}
