using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    public float shrinkWaitTime;
    public float shrinkAmount;
    public float shrinkDuration;
    public float minShrinkAmount;

    public int playerDamage;

    private float lastShrinkEndTime;
    private bool shrinking;
    private float targetDiameter;
    private float lastPlayerCheckTime;

    private void Start()
    {
        lastShrinkEndTime = Time.time;
        targetDiameter = transform.localScale.x;
    }

    private void Update()
    {
        if (shrinking)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, (shrinkAmount / shrinkDuration) * Time.deltaTime);
            if(transform.localScale.x <= targetDiameter)
            {
                shrinking = false;
            }
        }
        else
        {
            //shring if enough time has elapsed and the feild isn't too small
            if(Time.time - lastShrinkEndTime >= shrinkWaitTime && transform.localScale.x > minShrinkAmount)
            {
                Shrink();
            }
        }

        CheckPlayers();
    }

    private void Shrink()
    {
        shrinking = true;

        if(transform.localScale.x - shrinkAmount > minShrinkAmount)
        {
            targetDiameter -= shrinkAmount;
        }
        else
        {
            targetDiameter = minShrinkAmount;
        }

        lastShrinkEndTime = Time.time + shrinkDuration;
    }

    private void CheckPlayers()
    {
        if(Time.time - lastPlayerCheckTime > 1.0f)
        {
            lastPlayerCheckTime = Time.time;

            foreach (PlayerController player in GameManager.instance.players)
            {
                if(player.dead || !player)
                {
                    continue;
                }
                //CAUTION: measures from the center of the world, not the center of the object
                if(Vector3.Distance(Vector3.zero, player.transform.position) >= transform.localScale.x)
                {
                    player.photonView.RPC("TakeDamage", player.photonPlayer, 0, playerDamage);
                }
            }
        }
    }
}
