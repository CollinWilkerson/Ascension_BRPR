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
    public float bulletSpeed;
    public float shootRate;

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

        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);
    }

    [PunRPC]
    private void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rig.AddForce(dir * bulletSpeed, ForceMode.VelocityChange);
    }

    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        GameUI.instance.UpdateAmmoText();
    }
}
