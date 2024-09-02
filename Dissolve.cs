using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private bool dissolve = false;
    LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dissolve)
        {
            lineRenderer.material.color = new Color(225, 225, 225, 225 - Time.deltaTime);
        }
    }

    public void DissolveEnable()
    {
        dissolve = true;
    }
}
