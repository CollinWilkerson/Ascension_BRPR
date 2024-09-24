using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;

    [Header("GameStats")]
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;

    public int id;
    public Player photonPlayer;

    private int curAttackerId;
    public PlayerWeapon weapon;

    private void Start()
    {
        rig = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //only operate on live client player
        if(!photonView.IsMine || dead)
        {
            return;
        }

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            weapon.TryShoot();
        }
    }

    private void Move()
    {
        //read key inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //compencates for object rotation
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        //physics based movement, no frame rate compensation needed
        rig.velocity = dir;
    }

    private void TryJump()
    {
        //raycast down and trigger if it hits an object
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 1.5f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        //removes camera for non client players
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
        {
            return;
        }

        curHp -= damage;
        curAttackerId = attackerId;
        photonView.RPC("DamageFlash", RpcTarget.All);

        if(curHp < 0)
        {
            photonView.RPC("Die", RpcTarget.All);
        }

        GameUI.instance.UpdateHealthBar();
    }

    [PunRPC]
    private void DamageFlash()
    {
        if (flashingDamage)
        {
            return;
        }

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;
            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    private void Die()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }

        if (photonView.IsMine)
        {
            if(curAttackerId != 0)
            {
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);
            }
            GetComponentInChildren<CameraController>().SetAsSpectator();

            //make the player useless
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;

        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int healAmount)
    {
        curHp = Mathf.Clamp(curHp + healAmount, 0, maxHp);

        GameUI.instance.UpdateHealthBar();
    }
}
