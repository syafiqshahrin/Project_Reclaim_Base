using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordRaycast : MonoBehaviour
{
    bool planeGenerated;
    GameObject selected;
    [SerializeField] GameObject cuttingPlane;
    string previousPlaneName;

    void Start()
    {
        planeGenerated = false;
        if (cuttingPlane == null) cuttingPlane = GameObject.Find("CuttingPlane");
        if (cuttingPlane != null && cuttingPlane.activeInHierarchy) cuttingPlane.SetActive(false);
    }

    void Update()
    {
        Swordcast();
        if (Input.GetMouseButtonDown(0)) Slash();
    }

    private void OnDrawGizmos()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, forward);
    }

    void Swordcast()
    {
        RaycastHit hit;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        
        if (Physics.Raycast(transform.position, forward, out hit, 15.0f))
        {
            //Debug.Log("Raycast has detected " + hit.collider.name);
            GeneratePlane(hit);
        }
        else
        {
            if (cuttingPlane.activeInHierarchy)
            {
                cuttingPlane.SetActive(false);
                selected = null;
                planeGenerated = false;
            }
        }
    }

    void GeneratePlane(RaycastHit hit)
    {
        if(cuttingPlane != null)
        {
            if (cuttingPlane.activeInHierarchy)
            {
                cuttingPlane.SetActive(false);
                selected = null;
            }

            if (hit.collider.GetComponent<SlicingJoints>() != null)
            {
                selected = hit.collider.gameObject;
                GameObject joint = hit.collider.GetComponent<SlicingJoints>().connectedJoint;
                //cuttingPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                planeGenerated = true;
                //cuttingPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
                cuttingPlane.transform.position = joint.transform.position;
                cuttingPlane.transform.rotation = joint.transform.rotation;
                //cuttingPlane.transform.localScale = new Vector3(0.5f, 0.05f, 0.05f);
                cuttingPlane.SetActive(true);
            }
            else
            {
                selected = null;
                planeGenerated = false;
            }
        }
    }

    void Slash()
    {
        if(selected != null)
        {
            foreach (Transform child in selected.gameObject.transform)
            {
                child.gameObject.SetActive(false);
            }
            selected.gameObject.SetActive(false);
        }
    }
}
