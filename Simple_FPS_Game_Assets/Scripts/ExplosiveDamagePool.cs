using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveDamagePool : MonoBehaviour
{
    public GameObject dmgPrefab; // The blood prefab to be pooled
    private int dmgPoolSize = 1; // The initial size of the blood pool

    private List<GameObject> dmgPool; // The list to store blood instances
    private GameObject dmgPoolParent; // The parent GameObject for the blood pool


    public GameObject effectPrefab; // The blood prefab to be pooled
    private int effectPoolSize = 1; // The initial size of the blood pool

    private List<GameObject> effectPool; // The list to store blood instances
    private GameObject effectPoolParent; // The parent GameObject for the blood pool
    private Gun gun;
    private void Awake()
    {
        gun = GetComponentInParent<Gun>();

        InitializeDmgPool();
        InitializeEffectPool();
    }

    // Function to initialize the blood pool
    private void InitializeDmgPool()
    {
        // Create an empty GameObject to serve as the parent for the blood pool
        dmgPoolParent = new GameObject(gun.gunName + " ExplosiveRoundPool");
        //poolParent.AddComponent<FindAoePool>();

        // Initialize the bloodPool list
        dmgPool = new List<GameObject>();

        // Create a pool of blood instances
        for (int i = 0; i < dmgPoolSize; i++)
        {
            GameObject blood = Instantiate(dmgPrefab);
            blood.SetActive(false);
            blood.transform.parent = dmgPoolParent.transform;
            dmgPool.Add(blood);
        }
    }

    // Function to get a pooled blood object
    public GameObject GetDmgObjectFromPool()
    {
        //Debug.Log("pooling explosive round");
        // Find the first inactive object in the pool and return it
        foreach (GameObject obj in dmgPool)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // If no inactive objects are found, create a new one and add it to the pool
        GameObject newObj = Instantiate(dmgPrefab, transform);
        Debug.Log("adding new explosive round");
        // Set the scale to match the prefab
        newObj.transform.parent = dmgPoolParent.transform;
        newObj.transform.localScale = dmgPrefab.transform.localScale;
        dmgPool.Add(newObj);
        return newObj;
    }

    private void InitializeEffectPool()
    {
        // Create an empty GameObject to serve as the parent for the blood pool
        effectPoolParent = new GameObject(gun.gunName + " ExplosiveEffectPool");

        // Initialize the bloodPool list
        effectPool = new List<GameObject>();

        // Create a pool of blood instances
        for (int i = 0; i < effectPoolSize; i++)
        {
            GameObject blood = Instantiate(effectPrefab);
            blood.SetActive(false);
            blood.transform.parent = effectPoolParent.transform;
            effectPool.Add(blood);
        }
    }

    // Function to get a pooled blood object
    public GameObject GetEffectObjectFromPool()
    {
        // Find the first inactive object in the pool and return it
        foreach (GameObject obj in effectPool)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // If no inactive objects are found, create a new one and add it to the pool
        GameObject newObj = Instantiate(effectPrefab, transform);
        Debug.Log("adding new explosive effect");
        // Set the scale to match the prefab
        newObj.transform.parent = effectPoolParent.transform;
        newObj.transform.localScale = effectPrefab.transform.localScale;
        effectPool.Add(newObj);
        return newObj;
    }
}
