using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPortotype : NetworkBehaviour
{
    NavMeshAgent _navAgent;
    Vector3 _currentTarget;
    [SerializeField] float _distance;


    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!base.IsServer) return;
        SetRandomPointForPatrol();
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.IsServer) return;

        _distance = Vector3.Distance(transform.position, _currentTarget);

        if (_distance <= _navAgent.stoppingDistance || _distance > 30f || _navAgent.isPathStale || !_navAgent.hasPath)
        {
            SetRandomPointForPatrol();
        }
    }



    public void SetRandomPointForPatrol()
    {
        _currentTarget = GetRandomPointOnNavMesh();
        _navAgent.SetDestination(_currentTarget);
    }

    private Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 10f, 1);
        Vector3 finalPosition = hit.position;
        return finalPosition;
    }
}
