using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrailReturnTime : MonoBehaviour
{
    public float returnTime = 3;
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(WaitTime());
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(returnTime);
        gameObject.SetActive(false);

    }
}
