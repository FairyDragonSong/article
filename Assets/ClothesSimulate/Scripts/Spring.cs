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

    private Vector3 objMove;
    private Vector3 objPrePosition;

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
    /// 阻尼
    /// </summary>
    public float parti_damp = 1;

    /// <summary>
    /// 弹力
    /// </summary>
    public float elastic = 0.5f;


    public float parti_elastic = 0.5f;

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
        objPrePosition = transform.position;

        for (int i = 0; i < gos.Length; i++)
        {
            particle p = new particle();
            p.curPos = gos[i].transform.position;
            p.prePos = gos[i].transform.position;
            p.initPos = gos[i].transform.localPosition;
            infos.Add(p);
            if (i == gos.Length - 1)
            {
                hookPos2 = gos[i].transform.localPosition;
            }
            else if (i == gos.Length - width)
            {
                hookPos1 = gos[i].transform.localPosition;
            }
        }

        queue = new CoroutineQueue(2, StartCoroutine);
        // queue.Run(DelayAddForce());
    }

    IEnumerator DelayAddForce()
    {
        yield return new WaitForSeconds(1);

        gos[0].transform.Translate(-Vector3.right);
        gos[1].transform.Translate(Vector3.right);

        infos[0].curPos = gos[0].transform.position;
        infos[1].curPos = gos[1].transform.position;
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
        objMove = transform.position - objPrePosition;
        objPrePosition = transform.position;
    }

    void LateUpdate()
    {
        for (int i = 0; i < gos.Length; i++)
        {
            //惯性
            CheckInertia(gos[i], infos[i]);
        }


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

        // HookPoints();

    }

    public void HookPoints()
    {
        gos[gos.Length - 1].transform.localPosition = hookPos2;
        gos[gos.Length - width].transform.localPosition = hookPos1;
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
        Vector3 delLen = transform.InverseTransformPoint(info1.curPos) - transform.InverseTransformPoint(info2.curPos); //go1.transform.localPosition - go2.transform.localPosition;
        Vector3 prePrePos1 = transform.InverseTransformPoint(info1.prePos);
        Vector3 prePrePos2 = transform.InverseTransformPoint(info2.prePos);

        Vector3 curPos1 = transform.InverseTransformPoint(info1.curPos);
        Vector3 curPos2 = transform.InverseTransformPoint(info2.curPos);

        Vector3 speed1 = (curPos1 - prePrePos1) / Time.deltaTime;
        Vector3 speed2 = (curPos2 - prePrePos2) / Time.deltaTime;
        CheckSpring(go1, info1, iNormalLen, delLen, speed1, speed2);
        CheckSpring(go2, info2, iNormalLen, -delLen, speed2, speed1);
    }

    private void CheckInertia(GameObject go, particle info)
    {
        if (objMove.magnitude > 0)
        {
            
        }

        Vector3 v = info.curPos - info.prePos;
        Vector3 offsetVector3 = objMove * inertia;
        // info.prePos = offsetVector3 + info.curPos;
        info.curPos += v * (1 - parti_damp) + offsetVector3;


        Whipping(go, info);
    }

    private void Whipping(GameObject go, particle info)
    {
        Vector3 resPos = transform.TransformPoint(info.initPos);
        Vector3 d = resPos - info.curPos;
        info.curPos += d * parti_elastic;

    }

    private void CheckSpring(GameObject go, particle info, double iNormalLen, Vector3 delLen, Vector3 speed1, Vector3 speed2)
    {
        Vector3 curPrePos = transform.InverseTransformPoint(info.curPos);

        if (delLen.magnitude < Lmin)
        {
            // 弹簧异常 简单当做反弹处理
            speed1 = -speed1;

        }
        else
        {
            // 阻尼力和弹力计算
            Vector3 elasF;

            elasF = -(delLen.magnitude - (float)iNormalLen) * elastic * delLen / delLen.magnitude - damp * (speed1 - speed2);

            // 自身重力
            // elasF = elasF + acceleration * info.mass * Vector3.down;

            // 加速度
            Vector3 a1 = elasF / info.mass;

            speed1 += a1 * Time.deltaTime;

            info.speed = speed1;
        }

        Vector3 offset = speed1 * Time.deltaTime;

        go.transform.localPosition = curPrePos + offset;

        // 数据保存
        info.prePos = info.curPos;
        info.curPos = go.transform.position;
    }

}
