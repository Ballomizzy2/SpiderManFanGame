using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderManController : MonoBehaviour
{
    PlayerController parent;

    private void Start()
    {
        parent = transform.parent.parent.GetComponent<PlayerController>();
    }

    private void Jump()
    {
        parent.JumpTrigger();
    }

}
