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
    private float defaultZoom = 60f;
    private float sightsZoom = 40f;
    private float sniperZoom = 10f;
    private float initialSensitivity;
    private float sniperSensitivity;
    private CameraController mainCamController;
    private Vector3 hipPosition = new Vector3(0.5f, -0.45f, 1f);
    private Vector3 hipPositionSg = new Vector3(0.5f, -0.267f, 1.6f);
    private Vector3 sightPositionHG = new Vector3(0f, -0.22f, 0.65f);
    private Vector3 sightPositionS = new Vector3(0f, -0.33f, 1f);
    private Vector3 sightPositionSMG = new Vector3(0f, -0.08f, 1f);
    private Vector3 sightPositionSg = new Vector3(0f, -0.04f, 1.25f);
    public GameObject Handgun;
    public GameObject Sniper;
    public GameObject Smg;
    public GameObject Shotgun;

    private float lastShootTime;

    //public GameObject bulletPrefab;
    private Transform bulletSpawnPos;
    public Transform bulletSpawnPosHG;
    public Transform bulletSpawnPosS;
    public Transform bulletSpawnPosSMG;
    public Transform bulletSpawnPosSg;

    public PlayerController player;

    private LineRenderer bulletTrail;
    private float sightSpeed = 10f;

    private void Awake()
    {
        bulletTrail = player.GetComponent<LineRenderer>();
        bulletTrail.positionCount = 2;
        bulletTrail.enabled = false;
        currentGun = Handgun;
        bulletSpawnPos = bulletSpawnPosHG;
        mainCamController = Camera.main.GetComponent<CameraController>();
        initialSensitivity = mainCamController.sensX;
        sniperSensitivity = initialSensitivity / 4;
    }

    public void TryShoot()
    {
        //must have bullets and not exceed fire rate to shoot
        if(curAmmo <= 0)
        {
            player.photonView.RPC("GetGun", player.photonPlayer, "Handgun", 1, 999, 0.5f, 30);
            return;
        }
        else if (Time.time - lastShootTime < shootRate)
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
            //Debug.Log("Player hit");
            //if the Raycast hits a player
            if (!Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, Vector3.Distance(bulletSpawnPos.position, rayHit.point), obstructionMask))
            {
                int scaledDamage;
                PlayerController hitPlayer = rayHit.collider.gameObject.GetComponent<PlayerController>();
                //Debug.Log("No Obstruction");

                if(currentGun == Shotgun)
                {
                    scaledDamage = (int)(damage / Vector3.Distance(bulletSpawnPos.position, rayHit.point));
                }
                else
                {
                    scaledDamage = damage;
                }
                hitPlayer.photonView.RPC("TakeDamage", hitPlayer.photonPlayer, player.id, scaledDamage);

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
                //Debug.Log("Obstruction Hit");
            }
        }
        else if(Physics.Raycast(bulletSpawnPos.position, Camera.main.transform.forward, out rayHit, bulletRange, obstructionMask))
        {
            //Debug.Log("Obstruction hit at: " + rayHit.point);

            Vector3[] linePostitions = new Vector3[2];
            linePostitions[0] = bulletSpawnPos.position;
            linePostitions[1] = rayHit.point;
            bulletTrail.enabled = true;
            bulletTrail.SetPositions(linePostitions);
            Invoke("DisableBulletTrail", 0.2f);
        }
        else
        {
            /*
            Vector3[] linePostitions = new Vector3[2];
            linePostitions[0] = bulletSpawnPos.position;
            linePostitions[1] = Camera.main.transform.forward * bulletRange;
            bulletTrail.enabled = true;
            bulletTrail.SetPositions(linePostitions);
            Invoke("DisableBulletTrail", 0.2f);
            */
           // Debug.Log("No Hit");
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
        //curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);
        curAmmo = maxAmmo;

        GameUI.instance.UpdateAmmoText();
    }

    [PunRPC]
    public void GetGun(string gunName = "Handgun", int newDamage = 3, int newMax = 999, float newRate = 0.5f, int newRange = 30)
    {
        currentGun.SetActive(false);
        if (gunName == "Handgun")
        {
            Handgun.SetActive(true);
            currentGun = Handgun;
            bulletSpawnPos = bulletSpawnPosHG;
        }
        else if (gunName == "Sniper")
        {
            Sniper.SetActive(true);
            currentGun = Sniper;
            bulletSpawnPos = bulletSpawnPosS;
        }
        else if (gunName == "SMG")
        {
            Smg.SetActive(true);
            currentGun = Smg;
            bulletSpawnPos = bulletSpawnPosSMG;
        }
        else if (gunName == "Shotgun")
        {
            Shotgun.SetActive(true);
            currentGun = Shotgun;
            bulletSpawnPos = bulletSpawnPosSg;
        }

        damage = newDamage;
        maxAmmo = newMax;
        curAmmo = maxAmmo;
        shootRate = newRate;
        bulletRange = newRange;

        GameUI.instance.UpdateAmmoText();
    }

    public void AimDownSights(bool sights)
    {
        if (sights)
        {
            if(currentGun == Handgun)
            {
                //Lerp allows the motion to be smoothed
                currentGun.transform.localPosition = V3Lerp(currentGun.transform.localPosition, sightPositionHG, Time.deltaTime * sightSpeed);
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sightsZoom, Time.deltaTime * sightSpeed);
            }
            else if (currentGun == Sniper)
            {
                currentGun.transform.localPosition = V3Lerp(currentGun.transform.localPosition, sightPositionS, Time.deltaTime * sightSpeed);
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sniperZoom, Time.deltaTime * sightSpeed);
                mainCamController.sensX = sniperSensitivity;
                mainCamController.sensY = sniperSensitivity;
            }
            else if (currentGun == Smg)
            {
                currentGun.transform.localPosition = V3Lerp(currentGun.transform.localPosition, sightPositionSMG, Time.deltaTime * sightSpeed);
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sightsZoom, Time.deltaTime * sightSpeed);
            }
            else if(currentGun == Shotgun)
            {
                currentGun.transform.localPosition = V3Lerp(currentGun.transform.localPosition, sightPositionSg, Time.deltaTime * sightSpeed);
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, sightsZoom, Time.deltaTime * sightSpeed);
            }
        }
        else
        {
            if (currentGun == Shotgun)
            {
                currentGun.transform.localPosition = V3Lerp(currentGun.transform.localPosition, hipPositionSg, Time.deltaTime * sightSpeed);
            }
            else
            {
                currentGun.transform.localPosition = V3Lerp(currentGun.transform.localPosition, hipPosition, Time.deltaTime * sightSpeed);
            }

            if(Camera.main.fieldOfView != defaultZoom)
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defaultZoom, Time.deltaTime * sightSpeed);
            }
            if(mainCamController.sensX != initialSensitivity)
            {
                mainCamController.sensX = initialSensitivity;
                mainCamController.sensY = initialSensitivity;
            }
        }
    }

    private void DisableBulletTrail()
    {
        bulletTrail.enabled = false;
    }

    private Vector3 V3Lerp(Vector3 currentPos, Vector3 targetPos, float t)
    {
        return new Vector3(Mathf.Lerp(currentPos.x, targetPos.x, t),
                    Mathf.Lerp(currentPos.y, targetPos.y, t),
                    Mathf.Lerp(currentPos.z, targetPos.z, t));
    }
}
