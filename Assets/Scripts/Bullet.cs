using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public int despawnTimeMS;
	private Stopwatch despawn = new();
	private GameObject bullet;

	public GameObject explosion;
	// Start is called before the first frame update
	void Start()
	{
		bullet = GetComponentInParent<GameObject>();
		despawn.Restart();
	}

	// Update is called once per frame
	void Update()
	{
		if (despawn.ElapsedMilliseconds > despawnTimeMS)
		{
			Destroy(bullet);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Floor"))
		{
			BulletCollision(collision.collider.gameObject);
		}
	}

	private void BulletCollision(GameObject collider)
	{
		Instantiate(explosion).GetComponent<ParticleSystem>().Play();
		collider.SendMessage("BulletHit");
		List<GameObject> enemies = new();
		Destroy(bullet);
	}
}