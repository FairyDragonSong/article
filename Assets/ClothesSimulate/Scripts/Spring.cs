﻿using System;
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

        gos[0].transform.Translate(-Vector3.right);
        gos[1].transform.Translate(Vector3.right);
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
        // ResetPos();
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

        // HookPoints();
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
        // CheckInertia(go1, info1);
        // CheckInertia(go2, info2);

        Vector3 delLen = go1.transform.localPosition - go2.transform.localPosition;
        Vector3 prePrePos1 = transform.InverseTransformPoint(info1.prePos);
        Vector3 prePrePos2 = transform.InverseTransformPoint(info2.prePos);
        Vector3 speed1 = (go1.transform.localPosition - prePrePos1) / Time.deltaTime;
        Vector3 speed2 = (go2.transform.localPosition - prePrePos2) / Time.deltaTime;
        CheckSpring(go1, info1, iNormalLen, delLen, speed1, speed2);
        CheckSpring(go2, info2, iNormalLen, -delLen, speed2, speed1);
    }

    private void CheckInertia(GameObject go, particle info)
    {
        Vector3 realMoveDis = go.transform.position - info.prePos;
        Vector3 offsetVector3 = realMoveDis * inertia * 0.9f;

        info.curPos = offsetVector3 + info.curPos;
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
            // elasF1 = elasF1 + acceleration * info.mass * Vector3.down;

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
