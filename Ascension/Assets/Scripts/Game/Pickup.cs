using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickupType
{
    Health,
    Ammo,
    Sniper,
    SMG,
    Shotgun
}


public class Pickup : MonoBehaviourPunCallbacks
{
    public PickupType type;
    public int value;

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if(type == PickupType.Health)
            {
                player.photonView.RPC("Heal", player.photonPlayer, value);
            }
            else if(type == PickupType.Ammo)
            {
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);
            }
            else if(type == PickupType.Sniper)
            {
                player.photonView.RPC("GetGun", player.photonPlayer, "Sniper", 20, 10, 1f, 100);
            }
            else if(type == PickupType.SMG)
            {
                player.photonView.RPC("GetGun", player.photonPlayer, "SMG", 1, 300, 0.05f, 20);
            }
            else if(type == PickupType.Shotgun)
            {
                player.photonView.RPC("GetGun", player.photonPlayer, "Shotgun", 50, 50, 0.3f, 20);
            }

            StartCoroutine(GameManager.instance.TempDestroy(gameObject));
        }
    }
}
