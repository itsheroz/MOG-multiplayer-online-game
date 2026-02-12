using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerListEntry : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Text PlayerNameText;

    public Button PlayerColorButton;
    public Button PlayerReadyButton;
    public Image PlayerReadyImage;

    protected int ownerId;
    private bool isPlayerReady;

    #region UNITY

    public virtual void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
        {
            PlayerReadyButton.gameObject.SetActive(false);
        }
        else
        {
            SettingPlayerProperties();

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
            {
                { PunGameSetting.PLAYER_READY, isPlayerReady}
            });

            PlayerReadyButton.onClick.AddListener(() =>
            {
                isPlayerReady = !isPlayerReady;
                SetPlayerReady(isPlayerReady);

                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
                {
                    { PunGameSetting.PLAYER_READY, isPlayerReady}
                });

                if (PhotonNetwork.IsMasterClient)
                {
                    FindFirstObjectByType<InsideRoomPanel>().LocalPlayerPropertiesUpdated();
                }
            });

            PlayerColorButton.onClick.AddListener(() =>
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                {
                    { PunGameSetting.PLAYER_COLOR, UnityEngine.Random.Range(0,7) }
                });
            });
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayerNumbering.OnPlayerNumberingChanged -= OnPlayerNumberingChanged;
    }

    #endregion

    public virtual void Initialize(int playerId, string playerName)
    {
        ownerId = playerId;
        PlayerNameText.text = playerName;
    }

    public static void SettingPlayerProperties()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
        {
            { PunGameSetting.PLAYER_LIVES, PunGameSetting.PLAYER_MAX_LIVES }
        });
        PhotonNetwork.LocalPlayer.SetScore(0);
    }

    /// <summary>
    /// Setting FirstTime when Player Joined Room
    /// </summary>
    private void OnPlayerNumberingChanged()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == ownerId)
            {
                PlayerColorButton.GetComponent<Image>().color = PunGameSetting.GetColor(p.GetPlayerNumber());
            }
        }
    }

    public void SetPlayerReady(bool playerReady)
    {
        PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Ready?";
        PlayerReadyImage.enabled = playerReady;
        PlayerColorButton.interactable = !playerReady;
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);

        if (changedProps.ContainsKey(PunGameSetting.PLAYER_COLOR) &&
            target.ActorNumber == ownerId)
        {
            object colors;
            if (changedProps.TryGetValue(PunGameSetting.PLAYER_COLOR, out colors))
            {
                PlayerColorButton.GetComponent<Image>().color = PunGameSetting.GetColor((int)colors);
            }

            return;
        }
    }
}
