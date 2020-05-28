using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public Transform firePoint;
    public Projectile projectile;

    [SerializeField] private float shootAngle = 0f;
    [SerializeField] private float shootAngleTurnAmount = 0f;
    [SerializeField] private float shootTimeOutDuration = 1f;
    

    // Start is called before the first frame update
    void Start()
    {
        ShootProjectile(shootAngle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShootProjectile(float angle)
    {
        Instantiate(projectile, firePoint.position, Quaternion.Euler(0, 0, angle));

        shootAngle += shootAngleTurnAmount;
        StartCoroutine(ShootTimeOut(shootTimeOutDuration));
    }

    IEnumerator ShootTimeOut(float duration)
    {
        yield return new WaitForSeconds(duration);
        ShootProjectile(shootAngle);
    }
}
