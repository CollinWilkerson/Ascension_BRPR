using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletRange;
    public float shootRate;
    public LayerMask targetMask;
    public LayerMask obstructionMask;

    private float lastShootTime;
    
    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    public PlayerController player;

    private void Awake()
    {
        player.GetComponent<PlayerController>();
    }

    public void TryShoot()
    {
        //must have bullets and not exceed fire rate to shoot
        if(curAmmo <= 0 || Time.time - lastShootTime < shootRate)
        {
            return;
        }

        curAmmo--;
        lastShootTime = Time.time;

        GameUI.instance.UpdateAmmoText();

        //if a ray does not hit an obstruction
        if (!Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, bulletRange, obstructionMask))
        {
            //if the Raycast hits a player
            if (Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, bulletRange, targetMask))
            {
                //gets the shot player
                RaycastHit hitPlayer;
                Ray shotRay = new Ray(Camera.main.transform.forward, bulletSpawnPos.position);
                Physics.Raycast(shotRay, out hitPlayer);

                //this vomit deals damage to the player we shot.
                player.photonView.RPC("TakeDamage", hitPlayer.collider.gameObject.GetComponent<PlayerController>().photonPlayer,
                    player.id, damage);

                //displays the shot
                Debug.DrawLine(shotRay.origin, hitPlayer.point, Color.white, 0.5f);
            }
            else
            {
                //draws line to end of range
                Debug.DrawLine(bulletSpawnPos.position, bulletSpawnPos.TransformDirection(Camera.main.transform.forward) * bulletRange, Color.white, 0.5f);
            }
        }

        //player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);
    }

    /*
    [PunRPC]
    private void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rig.AddForce(dir * bulletSpeed, ForceMode.VelocityChange);
    }
    */

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        GameUI.instance.UpdateAmmoText();
    }

    public void getGun(string gunName, int newDamage, int newMax, int newRate)
    {

    }
}
