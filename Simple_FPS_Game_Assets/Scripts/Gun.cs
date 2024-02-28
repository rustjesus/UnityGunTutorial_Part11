using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class Gun : MonoBehaviour
{
    public string gunName;
    [Header("Shared stuff")]
    [SerializeField] private bool isAuto = true;
    [Header("Hitscan stuff")]
    [SerializeField] private bool useHitscan = true;
    [SerializeField] private bool useInstantHitscan = true;
    [SerializeField] private float hitscanBulletSpeed = 20f;
    [SerializeField] private float hitscanDamage = 25f;
    [SerializeField] private float hitscanRange = 200f;
    [SerializeField] private float fireRate = 0.25f;

    [Header("Projectile stuff")]
    [SerializeField] private GameObject proj;
    [SerializeField] private bool useProjectileGavity = true;
    [SerializeField] private float rangeProjectileSpeed = 100f;

    private bool canShoot = true;
    private float shootTimer;

    private GameObject ammoPool;
    private ObjectPool ammoObjectPool;
    private GameObject trailPool;
    private ObjectPool trailObjectPool;
    [SerializeField] private int ammoCount = 15;
    private int maxGunAmmo;
    private int curGunAmmo;

    [SerializeField] private float reloadTime = 2f;
    private bool canReload = false;
    private bool isReloading = false;
    private float rTimer;
    [SerializeField] private Transform barrelCheckPos;

    private AimDownSights aimDownSights;
    private FPS_Controller fpsController;
    private AimGunAtRaycast aimGunAtRaycast;
    [SerializeField] private bool canADS = true;
    [Header("Lower is higer zoom")]
    [SerializeField] private float adsZoom = 60f;
    [SerializeField] private Vector3 adsOffset = Vector3.zero;
    private bool isAimingDownSight = false;
    private Vector3 posToShootFrom;
    private PlayerManager playerManager;
    private ExplosiveDamagePool explosiveDamagePool;
    [Header("Spray, Kick & Recoil")]
    [SerializeField] private float sprayAmount = 0.25f; //control the max spray
    [SerializeField] private float sprayDivideRate = 100f; //control the max inc rate
    [SerializeField] private float kickbackStrength = 2f; //control kickback
    [SerializeField] private float recoilAmount = 10f; //control kickback
    [SerializeField] private float recoilSpeed = 0.1f; // Speed of recoil effect
    [Header("ADS reduction")]
    [SerializeField] private float adsSprayDivider = 3;
    [SerializeField] private float adsKickDivder = 3;
    [SerializeField] private float adsRecoilDivder = 3;
    [Header("100% initally accurate bullets")]
    [SerializeField] private int accurateBulletOverrideCount = 2;
    [Header("time to reset accurate bullets & spray")]
    [SerializeField] private float accurateBulletResetTime = 1f;
    private float startSprayAmount;
    private float startKick;
    private int shotsFired = 0;
    private float accurateTimer = 0;
    private Vector3 sprayOffset;
    [HideInInspector] public bool isShooting = false;
    private GunRoot gunRoot;
    private Quaternion originalRotation; // To store the original rotation of the gun

    private void Awake()
    {
        if(GetComponent<ExplosiveDamagePool>() != null)
        {
            explosiveDamagePool = GetComponent<ExplosiveDamagePool>();
        }
        playerManager = GetComponentInParent<PlayerManager>();
        aimGunAtRaycast = FindObjectOfType<AimGunAtRaycast>();
        fpsController = GetComponentInParent<FPS_Controller>();
        aimDownSights = GetComponentInParent<AimDownSights>();
        gunRoot = GetComponentInParent<GunRoot>();

        //set ammo
        maxGunAmmo = ammoCount;
        curGunAmmo = maxGunAmmo;

        //ammo pool
        ammoPool = new GameObject();//spawns
        ammoObjectPool = ammoPool.AddComponent<ObjectPool>();//adds script
        ammoObjectPool.prefab = proj;//set pool prefab

        //set explosive damage pool
        if(explosiveDamagePool != null )
        {
            Projectile projectile = proj.GetComponent<Projectile>();
            projectile.explosiveDamagePool = explosiveDamagePool;
        }

        ammoObjectPool.poolSize = ammoCount; //set size of pool
        ammoObjectPool.gameObject.name = gunName + " AmmoPool";

        if(useHitscan == true)
        {
            //trail pool 
            trailPool = new GameObject();//spawns
            trailObjectPool = trailPool.AddComponent<ObjectPool>();//adds script
            trailObjectPool.prefab = playerManager.bulletTrail;//set pool prefab
            trailObjectPool.poolSize = ammoCount; //set size of pool
            trailObjectPool.gameObject.name = gunName + "TrailPool";
        }
    }
    private void Start()
    {
        startSprayAmount = sprayAmount;
        startKick = kickbackStrength;
        originalRotation = gunRoot.transform.localRotation;
    }
    private void OnEnable()
    {
        isAimingDownSight = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * hitscanRange);
    }
    private void TriggerKickback()
    {
        if (fpsController != null)
        {
            fpsController.ApplyKickback(kickbackStrength);
        }
    }
    void Update()
    {    
        if(PauseMenu.gameIsPaused == true)
        {
            return;
        }
        //Debug.Log(sprayOffset);             
        //check if shooting 
        if (Input.GetMouseButton(0))
        {
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }
        Debug.Log(isShooting);
        //accuracy override
        if (shotsFired > 0)
        {
            //Debug.Log("accuracy timer started");
            accurateTimer += Time.deltaTime;
            if (isShooting == false)
            {
                if (accurateTimer > accurateBulletResetTime)
                {
                    //Debug.Log("accuracy timer finished");
                    shotsFired = 0;
                    accurateTimer = 0;
                }
            }
            else
            {
                accurateTimer = 0;
            }

        }
        PlayerManager.aimingDownSights = isAimingDownSight;
        PlayerManager.currentAmmo = curGunAmmo;
        PlayerManager.currentMaxAmmo = ammoCount;
        PlayerManager.isReloading = isReloading;

        if (canADS)
        {
            if (Input.GetMouseButton(1) && isReloading == false)
            {
                if (isAimingDownSight == false)
                {
                    float result = Mathf.InverseLerp(0, 60, adsZoom);
                    fpsController.lookSpeed = result * (fpsController.lookSpeed);

                    aimDownSights.mainCamera.fieldOfView = adsZoom;
                    aimDownSights.gunCamera.fieldOfView = adsZoom;
                    aimGunAtRaycast.enabled = false;
                    aimDownSights.gunRoot.transform.position = aimDownSights.adsPos.transform.position + adsOffset;
                    aimDownSights.gunRoot.transform.rotation = Quaternion.LookRotation(aimDownSights.adsPos.transform.forward);

                    sprayAmount = sprayAmount / adsSprayDivider;
                    kickbackStrength = kickbackStrength / adsKickDivder;

                    isAimingDownSight = true;
                }
            }
            else
            {
                DisableAds();
            }
        }
        else
        {
            DisableAds();
        }


        if (canShoot == false)
        {
            shootTimer += Time.deltaTime;

            if (shootTimer > fireRate)
            {
                canShoot = true;
                shootTimer = 0;
            }
        }

        //reloading
        if (isReloading == true)
        {
            shotsFired = 0;
            accurateTimer = 0;

            rTimer += Time.deltaTime;
            //while reloading
            aimGunAtRaycast.enabled = false;
            aimDownSights.gunRoot.transform.position = aimDownSights.reloadPos.transform.position + adsOffset;
            aimDownSights.gunRoot.transform.rotation = Quaternion.LookRotation(aimDownSights.reloadPos.transform.forward);

            //when reloading is done
            if (rTimer > reloadTime)
            {
                isReloading = false;

                aimDownSights.gunRoot.transform.position = aimDownSights.gunPos.transform.position;
                aimGunAtRaycast.enabled = true;

                curGunAmmo = maxGunAmmo;
                rTimer = 0;
            }
        }

        if (canReload == true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isReloading = true;
                canReload = false;
            }
        }

        if (curGunAmmo > 0)
        {
            if (Physics.Linecast(barrelCheckPos.position, transform.position, playerManager.blockingLayer))
            {
                Debug.Log("blocked");
            }
            else
            {
                ShootGun();
            }
        }

        if (curGunAmmo < maxGunAmmo)
        {
            canReload = true;
        }
    }
    private void ShootGun()
    {
        if (isAuto)
        {

            if (canShoot && isReloading == false && Input.GetMouseButton(0))
            {
                if (useHitscan)
                {
                    RangeAttack(true);
                }
                else
                {
                    RangeAttack(false);
                }
                canShoot = false;
            }
        }
        else// single fire
        {

            if (canShoot && isReloading == false && Input.GetMouseButtonDown(0))
            {
                if (useHitscan)
                {
                    RangeAttack(true);
                }
                else
                {
                    RangeAttack(false);
                }
                canShoot = false;
            }
        }
    }
    private void RangeAttack(bool useHitscan)
    {
        if (isAimingDownSight == false)
        {
            StartCoroutine(RecoilShake());
        }

        posToShootFrom.Normalize();

        shotsFired++;
        curGunAmmo--;

        TriggerKickback();

        float currentSpray = 0;
        float shotsFiredSince = 0;

        //wait for accurate override
        if (shotsFired >= accurateBulletOverrideCount)
        {
            //use remaining shots to increase spray over shots rate
            shotsFiredSince = shotsFired - accurateBulletOverrideCount;
            for (int i = 0; i < shotsFiredSince; i++)
            {
                currentSpray += shotsFiredSince / sprayDivideRate;
            }
            if (currentSpray > sprayAmount)
            {
                currentSpray = sprayAmount;
            }
            // Generate spray offset
            sprayOffset = Random.insideUnitSphere * currentSpray;
        }

        if (useHitscan)
        {
            FireHitscan();
        }
        else
        {
            CreateProjectile();
        }
    }
    private void CreateProjectile()
    {
        if (isAimingDownSight)
        {
            posToShootFrom = Camera.main.transform.forward + sprayOffset;
        }
        else
        {
            posToShootFrom = transform.forward + sprayOffset;
        }

        GameObject proj = ammoObjectPool.GetObject();
        proj.transform.position = transform.position;
        proj.transform.rotation = Quaternion.LookRotation(posToShootFrom);


        //GameObject proj = Instantiate(proj, transform.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (useProjectileGavity)
        {
            rb.useGravity = true;
        }
        else
        {
            rb.useGravity = false;
        }

        rb.AddForce(posToShootFrom * rangeProjectileSpeed, ForceMode.Force);

    }
    private void FireHitscan()
    {

        if (isAimingDownSight)
        {
            posToShootFrom = Camera.main.transform.position + sprayOffset;
        }
        else
        {
            posToShootFrom = transform.position + sprayOffset;
        }

        if (Physics.Raycast(posToShootFrom, transform.forward, out RaycastHit hit, hitscanRange, playerManager.hitscanlayers))
        {
            if(useInstantHitscan)
            {
                GameObject trail = trailObjectPool.GetObject();
                trail.transform.position = transform.position;
                trail.transform.rotation = Quaternion.LookRotation(posToShootFrom);

                StartCoroutine(SpawnInstantTrail(trail.GetComponent<TrailRenderer>(), hit.point)); // Start the coroutine

                if (hit.collider.GetComponent<EnemyHealth>() != null)
                {
                    EnemyHealth eh = hit.collider.GetComponent<EnemyHealth>();
                    eh.health -= hitscanDamage;
                }
            }
            else //hitscan over travel time
            {
                GameObject trail = trailObjectPool.GetObject();
                trail.transform.position = transform.position;
                trail.transform.rotation = Quaternion.LookRotation(posToShootFrom);

                StartCoroutine(SpawnTravelTrail(trail.GetComponent<TrailRenderer>(), hit)); // Start the coroutine

            }
        }
        else//miss raycast
        {
            Vector3 misseverything = transform.position + transform.forward * hitscanRange;
            GameObject trail = trailObjectPool.GetObject();
            trail.transform.position = transform.position;
            trail.transform.rotation = transform.rotation;

            StartCoroutine(SpawnMissTrail(trail.GetComponent<TrailRenderer>(), misseverything)); // Start the coroutine
        }

    }

    private void DisableAds()
    {
        //reset look speed
        fpsController.lookSpeed = fpsController.startLookSpeed;

        kickbackStrength = startKick;
        sprayAmount = startSprayAmount;

        //reset the fov
        aimDownSights.mainCamera.fieldOfView = 60f;
        aimDownSights.gunCamera.fieldOfView = 60f;
        //reset gun pos
        aimDownSights.gunRoot.transform.position = aimDownSights.gunPos.transform.position;
        aimGunAtRaycast.enabled = true;

        isAimingDownSight = false;

    }

    private IEnumerator SpawnInstantTrail(TrailRenderer trail, Vector3 miss)
    {
        float time = 0;
        Vector3 startpos = trail.transform.position;

        while (time < 0.05f)
        {
            trail.transform.position = Vector3.Lerp(startpos, miss, time);

            time += Time.deltaTime;

            yield return null;
        }
        trail.transform.position = miss;

        //Destroy(trail.gameObject, trail.time);
    }
    private IEnumerator SpawnMissTrail(TrailRenderer trail, Vector3 miss)
    {
        float time = 0;
        Vector3 startpos = trail.transform.position;

        while (time < 0.15f)
        {
            trail.transform.position = Vector3.Lerp(startpos, miss, time);

            time += Time.deltaTime;

            yield return null;
        }
        trail.transform.position = miss;

        //Destroy(trail.gameObject, trail.time);
    }
    private IEnumerator SpawnTravelTrail(TrailRenderer Trail, RaycastHit hit)
    {
        Vector3 startPosition;

        startPosition = Trail.transform.position;

        Vector3 direction = (hit.point - Trail.transform.position).normalized;

        float distance = Vector3.Distance(Trail.transform.position, hit.point);
        float startingDistance = distance;

        while (distance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, hit.point, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * hitscanBulletSpeed;

            yield return null;
        }

        Trail.transform.position = hit.point;

        //hitscan dmg
        if (hit.point != null)
        {
            if (hit.collider.GetComponent<EnemyHealth>() != null)
            {
                EnemyHealth eh = hit.collider.GetComponent<EnemyHealth>();
                eh.health -= hitscanDamage;
            }
        }
        //Destroy(Trail.gameObject, Trail.time);
    }
    private IEnumerator RecoilShake()
    {
        float time = 0f;

        while (time < recoilSpeed)
        {
            // Apply recoil
            gunRoot.gameObject.transform.localRotation = Quaternion.Lerp(gunRoot.gameObject.transform.localRotation, originalRotation * Quaternion.Euler(-recoilAmount, 0, 0), time * recoilSpeed);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;
        while (time < recoilSpeed)
        {
            // Return to original position
            gunRoot.gameObject.transform.localRotation = Quaternion.Lerp(gunRoot.gameObject.transform.localRotation, originalRotation, time * recoilSpeed);
            time += Time.deltaTime;
            yield return null;
        }
    }
}
