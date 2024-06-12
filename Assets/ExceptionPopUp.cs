using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExceptionPopUp : MonoBehaviour
{
    //Throws An X when something is wrong
    public GameObject X;
    public Transform location;

    public void Start()
    {
        location = GameObject.FindGameObjectWithTag("XTag").transform;
        X = location.GetComponent<ExceptionPopUp>().X;
        popup();
    }
    public void popup()
    {
        GameObject.Instantiate(X,location.position,location.rotation,location);
        StartCoroutine(destroy());
    }

    private IEnumerator destroy()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(this);
    }
}
