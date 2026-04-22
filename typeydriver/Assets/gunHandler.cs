using System.Collections;
using UnityEngine;

public class gunHandler : MonoBehaviour
{
    public followTarget followTargetScript;
    public Transform gunTip;
    public LineRenderer lineRenderer;
    public Animator handAnimator;
    public Material shotMaterial;

    public GunType currentGun;

    private float nextFireTime = 0f;

    // Gatling tuning
    public float minFireRate = 0.5f;
    public float maxFireRate = 0.08f;
    public float gatlingRampSpeed = 0.3f;
    private float currentFireRate;

    //damage values
    public float scatterDamage = 7;
    public float gatlingDamage = 3;
    public float pistolDamage = 25;
    public float rocketDamage = 75;

    // General
    public float lineDuration = 0.05f;

    void Start()
    {
        lineRenderer.enabled = false;
        currentFireRate = minFireRate;
    }

    void Update()
    {
        if (followTargetScript == null)
            return;

        if (followTargetScript.target != null)
            return;

        switch (currentGun)
        {
            case GunType.pistol:
                if (Input.GetMouseButton(0))
                    TryShoot(0.5f, () => ShootSingle(pistolDamage));
                break;

            case GunType.scatter:
                if (Input.GetMouseButtonDown(0))
                    TryShoot(0.8f, ShootScatter);
                break;

            case GunType.gatling:
                HandleGatling();
                break;

            case GunType.rocket:
                if (Input.GetMouseButtonDown(0))
                    TryShoot(1.5f, ShootRocket);
                break;

            case GunType.beam:
                HandleBeam();
                break;
        }
    }

    void TryShoot(float rate, System.Action shootMethod)
    {
        if (Time.time >= nextFireTime)
        {
            shootMethod();
            nextFireTime = Time.time + rate;
        }
    }

    // ------------------------
    // Gun Behaviors
    // ------------------------

    void ShootSingle(float shotDamage)
    {
        PlayRecoil();
        FireRay(Camera.main.transform.forward, shotDamage);
    }

    void ShootScatter()
    {
        for (int i = 0; i < 6; i++)
        {
            PlayRecoil();
            Vector3 dir = Camera.main.transform.forward + Random.insideUnitSphere * 0.04f;
            FireRay(dir.normalized, scatterDamage);
        }
    }

    void HandleGatling()
    {
        if (Input.GetMouseButton(0))
        {
            // Smooth ramp (linear)
            currentFireRate = Mathf.MoveTowards(currentFireRate, maxFireRate, Time.deltaTime * gatlingRampSpeed);

            TryShoot(currentFireRate, () => ShootSingle(gatlingDamage));
        }
        else
        {
            currentFireRate = minFireRate;
        }
    }

    void ShootRocket()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f))
        {
            PlayRecoil();
            StartCoroutine(ShowShot(gunTip.position, hit.point));

            float radius = 5f;

            StartCoroutine(ShowExplosion(hit.point, radius));

            Collider[] cols = Physics.OverlapSphere(hit.point, radius);

            foreach (Collider col in cols)
            {
                Rigidbody rb = col.attachedRigidbody;
                healthHandler hpScript = col.GetComponent<healthHandler>();
                if (!hpScript)
                continue;

                Debug.Log("Hit in explosion: " + col.name);

                if (rb == null)
                    rb = col.GetComponentInParent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(2500f, hit.point, radius);
                    if ( !hpScript.isPlayer)
                    hpScript.hitPoints -= rocketDamage;
                }
            }
        }
    }

    void HandleBeam()
    {
        if (Input.GetMouseButton(0))
        {
            ShootBeam();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void ShootBeam()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, gunTip.position);
            lineRenderer.SetPosition(1, hit.point);

            Rigidbody rb = hit.rigidbody;
            if (rb == null)
                rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(Vector3.up * 500f * Time.deltaTime, ForceMode.Force);
            }
        }
        else
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, gunTip.position);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * 100f);
        }
    }

    // ------------------------
    // Core Ray Logic
    // ------------------------

    void FireRay(Vector3 direction, float damage)
    {
        if (Physics.Raycast(Camera.main.transform.position, direction, out RaycastHit hit, 100f))
        {
            StartCoroutine(ShowShot(gunTip.position, hit.point));
            healthHandler hpScript = hit.collider.GetComponent<healthHandler>();
            if (!hpScript)
            return;

            if (hit.rigidbody != null)
                hit.rigidbody.AddForce(Vector3.up * 100f);
                hpScript.hitPoints -= damage;
        }
        else
        {
            // Show line even if nothing hit
            StartCoroutine(ShowShot(gunTip.position, Camera.main.transform.position + direction * 100f));
        }
    }

    void PlayRecoil()
    {
        if (handAnimator != null)
            handAnimator.SetTrigger("Fire");
    }

    // ------------------------
    // Visual Effects
    // ------------------------

    IEnumerator ShowShot(Vector3 start, Vector3 end)
    {
        LineRenderer lr = Instantiate(lineRenderer, transform);
        lr.enabled = true;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(lineDuration);

        Destroy(lr.gameObject);

        // Only disable if not using beam
        if (currentGun != GunType.beam)
            lineRenderer.enabled = false;
    }

    IEnumerator ShowExplosion(Vector3 center, float radius)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = center;
        sphere.transform.localScale = Vector3.one * radius * 2f;

        Renderer r = sphere.GetComponent<Renderer>();
        r.material = shotMaterial;

        Destroy(sphere.GetComponent<Collider>());

        yield return new WaitForSeconds(0.2f);
        Destroy(sphere);
    }

    // ------------------------
    // Gun Types Enum
    // ------------------------

}


    public enum GunType
    {
        pistol,
        scatter,
        gatling,
        rocket,
        beam
    }

