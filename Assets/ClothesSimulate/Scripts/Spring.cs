using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle
{
    public Vector3 curPos;
    public Vector3 prePos;
    public Vector3 speed;
}

public class Spring : MonoBehaviour
{

    public GameObject go1;
    public GameObject go2;

    public particle info1 = new particle();
    public particle info2 = new particle();

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

    // Start is called before the first frame update
    void Start()
    {
        info1.curPos = go1.transform.position;
        info1.prePos = go1.transform.position;
        info2.curPos = go2.transform.position;
        info2.prePos = go2.transform.position;

        var queue = new CoroutineQueue(2, StartCoroutine);
        queue.Run(DelayAddForce());
    }

    IEnumerator DelayAddForce()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("===============");
        go1.transform.Translate(-go1.transform.right * 0.1f);
        go2.transform.Translate(go2.transform.right * 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        info1.curPos = go1.transform.position;
        info2.curPos = go2.transform.position;

    }

    void LateUpdate()
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

    public bool rever = false;

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
                Debug.Log(iExtentOfCritical);

                rever = true;
                iExtentOfCritical = iExtentOfCritical - 0.01f;
            }

            speed1 = info1.speed * (1 - iExtentOfCritical) * (rever ? -1 : 1);
            speed2 = info2.speed * (1 - iExtentOfCritical) * (rever ? -1 : 1);

        }
        else
        {
            rever = false;
            // 阻尼力和弹力计算
            Vector3 elasF1;
            Vector3 elasF2;

            elasF1 = -(delLen.magnitude - L0) * elastic * delLen / delLen.magnitude - damp * (speed1 - speed2);
            elasF2 = -elasF1;

            // 质量
            float mass1 = 1f;
            float mass2 = 1f;
            // 加速度
            Vector3 a1 = elasF1 / mass1;
            Vector3 a2 = elasF2 / mass2;

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
