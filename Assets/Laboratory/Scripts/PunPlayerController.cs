using UnityEngine;
using Photon.Pun;
using StarterAssets;
using ExitGames.Client.Photon;

public class PunPlayerController : MonoBehaviourPun
{
    public GameObject bulletPrefab;
    public GameObject firstaidPrefab;
    public Camera m_playerCam;
    [SerializeField]
    private StarterAssetsInputs _input;

    void Start()
    {
        if (m_playerCam == null)
            m_playerCam = Camera.main;

        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

#if ENABLE_INPUT_SYSTEM
        if (_input != null)
        {
            /*
            if (_input.fire)
            {
                _input.fire = false;
                CmdFire(this.bulletPrefab.name,
                    m_playerCam.transform.position,
                    m_playerCam.transform.rotation,
                    m_playerCam.transform.forward);
            }else if (_input.heal)
            {
                _input.heal = false;
                CmdFire(this.firstaidPrefab.name,
                    m_playerCam.transform.position,
                    m_playerCam.transform.rotation,
                    m_playerCam.transform.forward);
            }
            else if (_input.change)
            {
                _input.change = false;
                ChangeColorProperties();
            }
            */
        }
        else
        {
            Debug.Log("StarterAssetsInputs is NULL.");
        }
#endif
    }

    void CmdFire(string prefabName,Vector3 position, Quaternion rotation, Vector3 forward)
    {
        Debug.Log("CmdFire.");
        //Keep Data when Instantiate.
        object[] data = { photonView.ViewID };

        // Spawn the bullet on the Clients and Create the Bullet from the Bullet Prefab
        PhotonNetwork.Instantiate(prefabName
                                , position + (forward * 1.5f)
                                , rotation
                                , 0
                                , data);
    }

    private void ChangeColorProperties()
    {
        Hashtable props = new Hashtable
        {
            {PunGameSetting.PLAYER_COLOR, Random.Range(0,7)}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
