using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle
{
    public Vector3 curPos;
    public Vector3 prePos;
    public Vector3 speed;
    public float mass = 1;
}

public class Spring : MonoBehaviour
{
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

    private bool reverse = false;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < gos.Length; i++)
        {
            particle p = new particle();
            p.curPos = gos[i].transform.position;
            infos.Add(p);
        }

        var queue = new CoroutineQueue(2, StartCoroutine);
        queue.Run(DelayAddForce());
    }

    IEnumerator DelayAddForce()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("===============");

        // gos[0].transform.Translate(-Vector3.right * 0.0001f);
        // gos[1].transform.Translate(Vector3.right * 0.0001f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        // for (int i = 0; i < gos.Length; i++)
        // {
        //     List<int> aroundList = new List<int>();
        //     SingleAdd(aroundList, i - width - 1);
        //     SingleAdd(aroundList, i - width);
        //     SingleAdd(aroundList, i - width + 1);
        //     SingleAdd(aroundList, i - width + 1);
        //     SingleAdd(aroundList, i - 1);
        //     SingleAdd(aroundList, i + 1);
        //     SingleAdd(aroundList, i + width - 1);
        //     SingleAdd(aroundList, i + width);
        //     SingleAdd(aroundList, i + width + 1);
        //
        //     for (int j = 0; j < aroundList.Count; j++)
        //     {
        //         if (aroundList[j] >= 0 && aroundList[j] < gos.Length)
        //         {
        //             GameObject go1 = gos[i];
        //             particle info1 = infos[i];
        //             GameObject go2 = gos[aroundList[j]];
        //             particle info2 = infos[aroundList[j]];
        //             SpringSimulation(go1, info1, go2, info2);
        //             
        //         }
        //     }
        //     break;
        // }
        GameObject go1 = gos[0];
        particle info1 = infos[0];
        GameObject go2 = gos[1];
        particle info2 = infos[1];
        SpringSimulation(go1, info1, go2, info2);
    }

    public void SingleAdd<T>(List<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
        }
    }

    void SpringSimulation(GameObject go1, particle info1, GameObject go2, particle info2)
    {
        CheckInertia(go1, info1);
        CheckInertia(go2, info2);
        CheckSpring(go1, info1, go2, info2);
    }

    private void CheckInertia(GameObject go, particle info)
    {
        Vector3 realMoveDis = go.transform.position - info.prePos;
        Vector3 offsetVector3 = realMoveDis * inertia * 0.9f;

        // go.transform.position = offsetVector3 + info.curPos;
        info.curPos = offsetVector3 + info.curPos;
    }

    private void CheckSpring(GameObject go1, particle info1, GameObject go2, particle info2)
    {
        Vector3 delLen = go1.transform.position - go2.transform.position;

        Vector3 speed1 = (go1.transform.position - info1.prePos) / Time.deltaTime;
        Vector3 speed2 = (go2.transform.position - info2.prePos) / Time.deltaTime;

        if (delLen.magnitude < Lmin)
        {
            // 弹簧异常
            float iExtentOfCritical = (Lmin - delLen.magnitude) / (Lmin - Lcritical);

            if (iExtentOfCritical <= 0.99)
            {
                reverse = true;
                iExtentOfCritical = iExtentOfCritical - 0.01f;
            }

            speed1 = info1.speed * (1 - iExtentOfCritical) * (reverse ? -1 : 1);
            speed2 = info2.speed * (1 - iExtentOfCritical) * (reverse ? -1 : 1);

        }
        else
        {
            reverse = false;
            // 阻尼力和弹力计算
            Vector3 elasF1;
            Vector3 elasF2;

            elasF1 = -(delLen.magnitude - L0) * elastic * delLen / delLen.magnitude - damp * (speed1 - speed2);
            elasF2 = -elasF1;

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
