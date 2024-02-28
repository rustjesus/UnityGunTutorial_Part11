using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool useExplosive = false;
    [SerializeField] private float damageToDo = 25f;
    [SerializeField] private float returnTime = 3f;
    private float timer;
    [SerializeField] private bool destroyOnAnything = true;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float startReturnTime;
    [SerializeField] private bool isPlayers = false;
    private TrailRenderer trailRenderer;
    [HideInInspector] public ExplosiveDamagePool explosiveDamagePool;

    private void Awake()
    {
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        startReturnTime = returnTime;    
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }


    void Update()
    {
        timer += Time.deltaTime;

        if (timer > returnTime)
        {
            gameObject.SetActive(false);    
            timer = 0;
        }
    }
    private void OnEnable()
    {
        trailRenderer.enabled = false;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        StartCoroutine(StartTimeDelay());
    }
    IEnumerator StartTimeDelay()
    {
        yield return 0;//wait one frame
        trailRenderer.enabled = true;
        trailRenderer.emitting = true;
    }
    private void OnDisable()
    {
        returnTime = startReturnTime;
        timer = 0;
        transform.position = Vector3.zero;
        gameObject.transform.localScale = originalScale;
        transform.rotation = originalRotation;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(useExplosive == false)
        {

            if (isPlayers)
            {
                if (collision.collider.GetComponent<EnemyHealth>() != null)
                {
                    EnemyHealth eh = collision.collider.GetComponent<EnemyHealth>();
                    eh.health -= damageToDo;
                    gameObject.SetActive(false);
                }

            }
            else//is not players (enemys)
            {
                if (collision.collider.GetComponent<PlayerHealth>() != null)
                {
                    PlayerHealth eh = collision.collider.GetComponent<PlayerHealth>();
                    eh.curentHealth -= damageToDo;
                    eh.playerIsHit = true;
                    gameObject.SetActive(false);
                }
            }
        }



        if (destroyOnAnything)
        {
            if (useExplosive == true)
            {

                //get objs from pool
                GameObject explosiveShot = explosiveDamagePool.GetDmgObjectFromPool();
                explosiveShot.transform.position = transform.position;
                explosiveShot.transform.rotation = transform.rotation;

                GameObject effect = explosiveDamagePool.GetEffectObjectFromPool();
                effect.transform.position = transform.position;
                effect.transform.rotation = transform.rotation;
            }
            gameObject.SetActive(false);
        }

    }
}
