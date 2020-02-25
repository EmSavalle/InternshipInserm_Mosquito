using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public float angleNumber;
    public MovementManager mm;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        angleNumber =mm.angleNumber;
        for (int i = 0; i < angleNumber; i++)
        {
            Debug.DrawRay(transform.position, Quaternion.Euler(0,i * (360f / angleNumber),0) *(-1*transform.right) *(mm.maxDist),Color.red);
        }
    }



}
