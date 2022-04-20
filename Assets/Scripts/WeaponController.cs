using System.Diagnostics;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject bulletProjectile;

    public GameObject bombProjectile;

    public float bulletSpeed;
    public int gunCooldownMS = 100;
    public int bombCooldownMS = 2000;
    private bool doShoot;
    private bool doBomb;
    private Stopwatch gunCooldown = new();
    private Stopwatch bombCooldown = new();
    void Awake()
    {
        gunCooldown.Restart();
        bombCooldown.Restart();
    }

    public void Shoot()
    {
        if (AirfieldManager.Paused() && AirfieldManager.StateChanged())
        {
            gunCooldown.Stop();
            bombCooldown.Stop();
            return;
        }
        else if(AirfieldManager.StateChanged())
        {
            gunCooldown.Start();
            bombCooldown.Start();
        }
        if (gunCooldown.ElapsedMilliseconds > gunCooldownMS)
        {
            GameObject bullet = Instantiate(bulletProjectile);
            bullet.GetComponent<Rigidbody>().velocity = GetComponentInParent<Rigidbody>().velocity + (GetComponentInParent<Rigidbody>().velocity.normalized * bulletSpeed);
            gunCooldown.Restart();
        }
    }

    public void Bomb()
    {
        if (bombCooldown.ElapsedMilliseconds > bombCooldownMS)
        {
            GameObject bomb = Instantiate(bombProjectile);
            bomb.GetComponent<Rigidbody>().velocity = GetComponentInParent<Rigidbody>().velocity;
        }
    }
}
