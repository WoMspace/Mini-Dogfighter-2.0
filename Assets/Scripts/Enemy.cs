using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update

    private int hp, maxhp;
    public float oneShotDistance;
    [CanBeNull] public GameObject damagedParticles;
    [CanBeNull] public GameObject husk;
    [CanBeNull] public GameObject deathParticles;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(hp < 0) Dead();
        else if (hp < maxhp / 2) Damaged();
    }

    void Dead()
    {
        Instantiate(husk);
        Instantiate(deathParticles).GetComponent<ParticleSystem>().Play();
        Destroy(GetComponentInParent<GameObject>());
    }

    void Damaged()
    {
        if (damagedParticles != null) damagedParticles.SetActive(true);
    }
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

    void BulletHit()
    {
        hp -= 50;
    }
}
