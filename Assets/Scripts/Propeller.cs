using UnityEngine;

public class Propeller : MonoBehaviour
{
    private GameObject prop;

    public float rpm = 60f;
    // Start is called before the first frame update
    void Start()
    {
        prop = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 axle = Vector3.forward;
        prop.transform.Rotate(axle, RPMtoDeg(rpm));
    }

    private static float RPMtoDeg(float rpm)
    {
        return rpm * 6f;
    }
}
