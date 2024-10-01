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

    [Header("Ray Masks")]
    public LayerMask targetMask;
    public LayerMask obstructionMask;

    [Header("Guns")]
    private GameObject currentGun;
    private Vector3 hipPosition = new Vector3(0.5f,-0.45f,1f);
    private Vector3 sightPositionHG = new Vector3(0f, -0.22f, 0.65f);
    private Vector3 sightPositionS = new Vector3(0f, -0.25f, 1f);
    public GameObject Handgun;
    public GameObject Sniper;

    private float lastShootTime;
    
    //public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    public PlayerController player;

    private LineRenderer bulletTrail;

    private void Awake()
    {
        bulletTrail = player.GetComponent<LineRenderer>();
        bulletTrail.positionCount = 2;
        bulletTrail.enabled = false;
        currentGun = Handgun;
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


        RaycastHit rayHit;

        //if a ray does not hit an obstruction
        //change to if we hit the player then if we hit an obstruction
        if (Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, out rayHit, bulletRange, targetMask))
        {
            //Debug.Log("Start: " + bulletSpawnPos.position + " direction: " + Camera.main.transform.forward + " range: " + bulletRange);
            Debug.Log("Player hit");
            //if the Raycast hits a player
            if (!Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, Vector3.Distance(bulletSpawnPos.position, rayHit.point), obstructionMask))
            {
                PlayerController hitPlayer = rayHit.collider.gameObject.GetComponent<PlayerController>();
                Debug.Log("No Obstruction");
                //this vomit deals damage to the player we shot.
                hitPlayer.photonView.RPC("TakeDamage", hitPlayer.photonPlayer, player.id, damage);

                //this only works on the editor screen
                //Debug.DrawLine(bulletSpawnPos.position, rayHit.point, Color.red, 0.5f);
                Vector3[] linePostitions = new Vector3[2];
                linePostitions[0] = bulletSpawnPos.position;
                linePostitions[1] = rayHit.point;
                bulletTrail.enabled = true;
                bulletTrail.SetPositions(linePostitions);
                Invoke("DisableBulletTrail", 0.2f);
            }
            else
            {
                Debug.Log("Obstruction Hit");
            }
        }
        else if(Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, out rayHit, bulletRange, obstructionMask))
        {
            Debug.Log("Obstruction hit at: " + rayHit.point);

            Vector3[] linePostitions = new Vector3[2];
            linePostitions[0] = bulletSpawnPos.position;
            linePostitions[1] = rayHit.point;
            bulletTrail.enabled = true;
            bulletTrail.SetPositions(linePostitions);
            Invoke("DisableBulletTrail", 0.2f);
        }
        else
        {
            Vector3[] linePostitions = new Vector3[2];
            linePostitions[0] = bulletSpawnPos.position;
            linePostitions[1] = Camera.main.transform.forward * bulletRange;
            bulletTrail.enabled = true;
            bulletTrail.SetPositions(linePostitions);
            Invoke("DisableBulletTrail", 0.2f);
            Debug.Log("No Hit");
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

    [PunRPC]
    public void GetGun(string gunName = "Handgun", int newDamage = 5, int newMax = 100, float newRate = 0.1f, int newRange = 10)
    {
        currentGun.SetActive(false);
        if (gunName == "Sniper")
        {
            Sniper.SetActive(true);
            currentGun = Sniper;
        }

        damage = newDamage;
        maxAmmo = newMax;
        shootRate = newRate;
        bulletRange = newRange;
    }

    public void AimDownSights(bool sights)
    {
        if (sights)
        {
            if(currentGun == Handgun)
            {
                currentGun.transform.localPosition = sightPositionHG;
            }
            else if (currentGun == Sniper)
            {
                currentGun.transform.localPosition = sightPositionS;
            }
        }
        else
        {
            currentGun.transform.localPosition = hipPosition;
        }
    }

    private void DisableBulletTrail()
    {
        bulletTrail.enabled = false;
    }
}
