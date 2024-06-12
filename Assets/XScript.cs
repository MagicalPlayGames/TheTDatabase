using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {
        StartCoroutine(disappear());
    }

    public IEnumerator disappear()
    {
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }
}
