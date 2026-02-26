using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PunEnemy : MonoBehaviourPun, IDamageable
{
    public float startingHealth;
    protected float health;
    protected bool dead;

    NavMeshAgent pathfinder;
    Transform target;
    public GameObject[] _playerList;

    protected void Start()
    {
        health = startingHealth;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                pathfinder = GetComponent<NavMeshAgent>();
                _playerList = GameObject.FindGameObjectsWithTag("Player");
                target = FindNearTarget(_playerList);

                StartCoroutine(UpdatePath());
            }
        }
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    public void TakeHit(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    protected void Die()
    {
        dead = true;
        GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!photonView.IsMine)
            return;

        PhotonNetwork.Destroy(this.gameObject);
    }

    Transform FindNearTarget(GameObject[] _list)
    {
        Transform _current = null;
        float minimumLeaght = 100;
        foreach (GameObject objTrans in _list)
        {
            float currentLeaght = Vector3.Magnitude(this.transform.position - objTrans.transform.position);
            Debug.Log("currentLeaght : " + currentLeaght);
            if (currentLeaght < minimumLeaght)
            {
                _current = objTrans.transform;
                minimumLeaght = currentLeaght;
            }
        }

        return _current;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;

        while (target != null)
        {
            Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
            if (!dead)
            {
                pathfinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }

        if (target == null)
        {
            _playerList = GameObject.FindGameObjectsWithTag("Player");

            target = FindNearTarget(_playerList);
        }
    }
}
