using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PunGameTimer : MonoBehaviourPunCallbacks {
    /// <summary>
    /// OnCountdownTimerHasExpired delegate.
    /// </summary>
    public delegate void CountdownTimerHasExpired();

    /// <summary>
    /// Called when the timer has expired.
    /// </summary>
    public static event CountdownTimerHasExpired OnCountdownTimerHasExpired;

    public bool isTimerRunning;

    public float startTime;

    [Header("Reference to a Text component for visualizing the countdown")]
    public TextMeshProUGUI Text;

    [Header("Countdown time in seconds")]
    public float Countdown = 120f;
    public float currentCountDown;

    public void Start() {
        if (Text == null) {
            Debug.LogError("Reference to 'Text' is not set. Please set a valid reference.", this);
            return;
        }else
        {
            Text.text = "Wait to Start.";
        }
        //Add Delegate Function
        PunNetworkManager.OnGameStart += StartTime;
        PunNetworkManager.OnGameOver += OverTime;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;
    }

    public void Update() {

        if (!isTimerRunning)
            return;

        float timer = (float)PhotonNetwork.Time - startTime;
        currentCountDown = Countdown - timer;

        Text.text = "Time : " + CovertformatTime(currentCountDown);

        //Timeout Logic
        if (currentCountDown > 0.0f)
            return;

        isTimerRunning = false;

        Text.text = string.Empty;

        if (OnCountdownTimerHasExpired != null)  {
            OnCountdownTimerHasExpired();
        }
    }

    private void OnCountdownTimerIsExpired()
    {
        Debug.Log("Game is Over? or TimeOut : " + currentCountDown);

        if(PhotonNetwork.IsMasterClient)
            PunNetworkManager.singleton.CurrentGamestate = PunNetworkManager.gamestate.GameOver;
    }

    /// <summary>
    /// Static Method to call Start Game Time
    /// </summary>
    public void StartTime() {
        Hashtable props = new Hashtable {
            {PunGameSetting.START_GAMETIME, (float) PhotonNetwork.Time}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public void OverTime()
    {
        Text.text = "Time Up!!!";
    }

    public void GetStartTime(Hashtable propertiesThatChanged) {
        object startTimeFromProps;

        if (propertiesThatChanged.TryGetValue(PunGameSetting.START_GAMETIME, out startTimeFromProps)) {
            Debug.Log("GetStartTime Prop is : " + startTimeFromProps);
            isTimerRunning = true;
            startTime = (float)startTimeFromProps;
        }
    }

    #region Photon CallBack

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if(!PhotonNetwork.IsMasterClient)
            GetStartTime(PhotonNetwork.CurrentRoom.CustomProperties);
    }

    /// <summary>
    /// Photon Room Properties Update
    /// </summary>
    /// <param name="propertiesThatChanged"></param>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        GetStartTime(propertiesThatChanged);
    }

    #endregion

    //Uility Method
    string CovertformatTime(float seconds)
    {
        double hh = Math.Floor(seconds / 3600),
          mm = Math.Floor(seconds / 60) % 60,
          ss = Math.Floor(seconds) % 60;
        return hh.ToString("00") + ":" + mm.ToString("00") + ":" + ss.ToString("00");
    }
}
