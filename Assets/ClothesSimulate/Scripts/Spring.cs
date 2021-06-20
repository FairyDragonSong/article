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
    public float L0 = 1;

    private Vector3 oriCurPos;

    // Start is called before the first frame update
    void Start()
    {
        info1.curPos = go1.transform.position;
        info1.prePos = go1.transform.position;
        info2.curPos = go2.transform.position;
        info2.prePos = go2.transform.position;
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
        oriCurPos = info.curPos;
        info.curPos = offsetVector3 + info.curPos;
    }

    private void CheckSpring(GameObject go1, particle info1, GameObject go2, particle info2)
    {
        Vector3 delLen = go1.transform.position - go2.transform.position;

        // 乘上 0.9
        Vector3 speed1 = (go1.transform.position - info1.prePos) * 0.99f / Time.deltaTime;
        Vector3 speed2 = (go2.transform.position - info2.prePos) / Time.deltaTime;

        Vector3 elasF1 = -(delLen.magnitude - L0) * elastic * delLen / delLen.magnitude - damp * (speed1 - speed2);

        elasF1 = elasF1 * 0.1f;

        Vector3 elasF2 = - elasF1;

        float mass1 = 1f;
        float mass2 = 1f;

        Vector3 a1 = new Vector3(elasF1.x / mass1, elasF1.y / mass1, elasF1.z / mass1);
        Vector3 a2 = new Vector3(elasF2.x / mass2, elasF2.y / mass2, elasF2.z / mass2);

        Debug.Log(a1);
        Debug.Log(a2);

        speed1 += a1 * Time.deltaTime;
        speed2 += a2 * Time.deltaTime;

        Debug.Log(a1 * Time.deltaTime);
        Debug.Log(a2 * Time.deltaTime);
        Debug.Log("=========================");

        Vector3 offset1 = speed1 * Time.deltaTime;
        Vector3 offset2 = speed2 * Time.deltaTime;


        go1.transform.position = info1.curPos + offset1;
        go2.transform.position = info2.curPos + offset2;

        info1.prePos = info1.curPos;
        info2.prePos = info2.curPos;
        info1.curPos = go1.transform.position;
        info2.curPos = go2.transform.position;
    }
}
