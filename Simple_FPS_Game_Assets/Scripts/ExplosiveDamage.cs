using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveDamage : MonoBehaviour
{
    [HideInInspector] public float explosionDamage;
    [SerializeField] private float explosiveRadius = 3f;
    // Start is called before the first frame update
    void OnEnable()
    {
        ExplosionDamage(transform.position, explosiveRadius);
        gameObject.SetActive(false);
    }
    void ExplosionDamage(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<EnemyHealth>() != null)
            {
                EnemyHealth eh = hitCollider.GetComponent<EnemyHealth>();
                eh.health -= explosionDamage;
            }
        }
    }
}
