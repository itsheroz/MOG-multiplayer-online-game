using Photon.Pun;
using StarterAssets;
using UnityEngine;

public class PunStermina : MonoBehaviourPun, IPunObservable
{
    public float _currentStermina;
    public int currentStermina
    {
        get { return (int)_currentStermina; }
        set { _currentStermina = value; }
    }
    public FirstPersonController _fpc;

    [SerializeField]
    private StarterAssetsInputs _input;
    // Start is called before the first frame update
    void Start()
    {
        _fpc = GetComponent<FirstPersonController>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    public void OnGUI()
    {
        if (photonView.IsMine)
            GUI.Label(new Rect(0, 20, 300, 50), "Player Stermina : " + currentStermina);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

#if ENABLE_INPUT_SYSTEM
        if (_input != null)
        {
            if (_input.sprint && _input.move != Vector2.zero)
            {
                _currentStermina -= Time.deltaTime * 30f;

            }
            else
            {
                _currentStermina += Time.deltaTime * 15f;
            }

            _currentStermina = Mathf.Clamp(_currentStermina, 0, 100);
        }
        else
        {
            Debug.Log("StarterAssetsInputs is NULL.");
        }
#endif

        if (currentStermina > 30)
        {
            _fpc.SprintSpeed = 6f;
        }
        else if (currentStermina > 15)
        {
            _fpc.SprintSpeed = 3.5f;
        }
        else
        {
            _fpc.SprintSpeed = 2f;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentStermina);
        }
        else
        {
            currentStermina = (int)stream.ReceiveNext();
        }
    }
}
