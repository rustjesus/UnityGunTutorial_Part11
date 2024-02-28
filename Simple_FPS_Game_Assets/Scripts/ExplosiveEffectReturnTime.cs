using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveEffectReturnTime : MonoBehaviour
{
    [SerializeField] private float returnTime = 3f;
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(ReturnDelay());
    }
    IEnumerator ReturnDelay()
    {
        yield return new WaitForSeconds(returnTime);
        gameObject.SetActive(false);    
        //Destroy(gameObject);
    }
}
