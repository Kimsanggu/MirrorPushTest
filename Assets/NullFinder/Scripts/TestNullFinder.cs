using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNullFinder : MonoBehaviour
{
    public GameObject target;
    public void FindButtonEvent()
    {
        target.SetActive(true);
    }
    public void MissingMethod()
    {

    }
}
