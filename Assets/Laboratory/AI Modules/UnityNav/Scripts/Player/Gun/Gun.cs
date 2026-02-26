using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	public Transform muzzle;
	public TopDown_Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35;

	float nextShotTime;

	public void Shoot() {

		if (Time.time > nextShotTime) {
			nextShotTime = Time.time + msBetweenShots / 1000;
			TopDown_Projectile newProjectile = Instantiate (projectile, muzzle.position, muzzle.rotation) as TopDown_Projectile;
			newProjectile.SetSpeed (muzzleVelocity);
		}
	}
}
