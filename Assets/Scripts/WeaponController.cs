using System.Diagnostics;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Rigidbody bulletProjectile;

    public Rigidbody bombProjectile;

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

    // Update is called once per frame

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
            Rigidbody bullet = Instantiate(bulletProjectile);
            bullet.velocity = GetComponentInParent<Rigidbody>().velocity + (GetComponentInParent<Rigidbody>().velocity.normalized * bulletSpeed);
            gunCooldown.Restart();
        }
    }

    public void Bomb()
    {
        if (bombCooldown.ElapsedMilliseconds > bombCooldownMS)
        {
            Rigidbody bomb = Instantiate(bombProjectile);
            bomb.velocity = GetComponentInParent<Rigidbody>().velocity;
        }
    }
}
