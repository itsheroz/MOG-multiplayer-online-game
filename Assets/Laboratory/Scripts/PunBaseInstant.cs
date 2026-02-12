using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunBaseInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public int OwnerViewID = -1;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // e.g. store this gameobject as this player's charater in Player.TagObject
        info.Sender.TagObject = this.gameObject;
        OwnerViewID = info.photonView.OwnerActorNr;

        PunInstantiateObject(info);
    }

    protected virtual void PunInstantiateObject(PhotonMessageInfo info)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
            return;
        
        PunTriggerEnter(other);

        Destroy(this.gameObject);
    }

    protected virtual void PunTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Bullet Collision to other player.");

            PunUserNetControl tempOther = other.gameObject.GetComponent<PunUserNetControl>();
            if (tempOther != null)
                Debug.Log("Attack to Other ViewID : " + tempOther.photonView.ViewID);
            else Debug.Log("Empty Component.");

            TriggerWithPlayer(other);

            Destroy(this.gameObject, 0.5f);
        }
    }

    protected virtual void TriggerWithPlayer(Collider other)
    {

    }

    private void OnDestroy()
    {
        if (!photonView.IsMine)
            return;

        PhotonNetwork.Destroy(this.gameObject);
    }
}
