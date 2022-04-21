using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(WeaponController))]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public class Enemy : MonoBehaviour
{
    private int hp, maxhp;
    public float oneShotDistance;
    public GameObject enemy;
    
    public GameObject player;
    private bool canSeePlayer;
    public float maxTurnRate; // Degrees / second

    private WeaponController enemyWeapons;
    
    
    [CanBeNull] public GameObject damagedParticles;
    [CanBeNull] public GameObject husk;
    [CanBeNull] public GameObject deathParticles;
    private bool _isdamagedParticlesNotNull;

    private void Awake()
    {
        _isdamagedParticlesNotNull = damagedParticles != null;
    }

    void Start()
    { // When it spawns in
        enemyWeapons = GetComponent<WeaponController>();//enemy.AddComponent<WeaponController>();
        enemyWeapons.playerTeam = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!AirfieldManager.Paused())
        {
            if (hp < 0) Dead();
            else if (hp < maxhp / 2) Damaged();


            Vector3 directionToPlayer = Vector3.Normalize(player.transform.position - enemy.transform.position);
            float angle = Vector3.Angle(directionToPlayer, enemy.transform.forward);
            canSeePlayer = angle < 90f;
            seekPlayer(directionToPlayer, angle);
            shootPlayer(angle);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void Dead()
    {
        Instantiate(husk);
        Instantiate(deathParticles).GetComponent<ParticleSystem>().Play();
        Destroy(GetComponentInParent<GameObject>());
    }

    void Damaged()
    {
        if (_isdamagedParticlesNotNull) damagedParticles!.SetActive(true);
    }
    [UsedImplicitly]
    void nearbyExplosion(float distance)
    {
        if (distance < oneShotDistance)
        {
            hp = 0;
        }
        else
        {
            hp -= 50;
        }
    }

    void seekPlayer(Vector3 directionToPlayer, float angle)
    {
        if (canSeePlayer)
        { // turn towards player but not TOO quickly
            angle = angle < maxTurnRate ? angle : angle / 2f;
            enemy.transform.Rotate(directionToPlayer, angle * Time.deltaTime);
        }
        else
        { // 45d/s turn until player sighted
            enemy.transform.Rotate(Vector3.up, maxTurnRate * Time.deltaTime);
        }
    }

    void shootPlayer(float angle)
    {
        if (angle < 10)
        {
            enemyWeapons.Shoot();
        }
    }

    [UsedImplicitly]
    void BulletHit()
    {
        hp -= 50;
    }
}
