using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    bool hit;
    float timer = 0.0f;
    float duration = 2.0f;
    public void DummyHit()
    {
        //Debug.Log("Targeted!");
        gameObject.GetComponent<Renderer>().material.SetInt("_Targeted", 1);
        hit = true;
        timer = 0.0f;
    }
    private void Update()
    {
        if(hit)
        {
            timer += Time.deltaTime;
            if(timer > duration)
            {
                gameObject.GetComponent<Renderer>().material.SetInt("_Targeted", 0);
                hit = false;
            }
        }
    }
}
