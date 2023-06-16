using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum AIStateEnum { Idle, Patrol ,Pursuit, Attack, Investigate}
public enum AITargetType {none , Enemy, Noise, Point}


/// <summary>
/// Struct Represent The target That the AI is seeing
/// </summary>
public struct AITarget
{
    public Transform targetTransform { get; private set; }
    public Vector3 targetPos { get; private set; }
    public float targetDistance { get; private set; }
    public AITargetType targetType { get; private set; }
    

    //Public Setters
    public void Settarget(Transform targetTr, Vector3 pos, float distance, AITargetType type)
    {
        targetTransform = targetTr;
        targetPos = pos;
        targetDistance = distance;
        targetType = type;
    }

    public void SetTargetTransform(Transform targetTr)
    {
        targetTransform = targetTr;
    }

    public void SetTargetPos(Vector3 pos)
    {
        targetPos = pos;
        Debug.Log("Target Pos is updated " + targetPos);
    }

    public void SetTargetDistance (float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetPos(AITargetType type)
    {
        targetType = type;
    }

    public void Cleartarget()
    {
        targetTransform = null;
    //    Debug.Log("AI: Target Cleared");
    }
}


public class AIZombieManager : NetworkBehaviour
{

    #region Variables

    //Inspector Assigne
    [Header("AI Settings")]
    [SerializeField] private AIState _startingState;
    [Header("Detection")]
    [SerializeField] private LayerMask _detectionLayer;
    [SerializeField] private LayerMask _ignoredLineOfSightLayer;
    [SerializeField] private float _detectionRadius;
    [SerializeField] [Range(-360, 0)] private float _minFieldOfViewAngle;
    [SerializeField] [Range(0, 360)] private float _maxFieldOfViewAngle;
    [SerializeField] [Range(-360, 0)] private float _minFieldOfViewAngleOnHavingTarget;
    [SerializeField] [Range(0, 360)] private float _maxFieldOfViewAngleOnHavingTarget;
    [SerializeField] private float _looseSightTime = 3f;

    [Header("Stats")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _attackDamage;

    [Header("References")]
    [SerializeField] private Transform _playerHeadTransform;

    public Collider leftDamageArea;
    public Collider rightDamageArea;

    [Header("Others")]
    [SerializeField] private bool _inverseForward;

    //Var's
    [SerializeField] private AIState _currentState;
    private AIState _nextState;
    private AITarget _currentTarget;
    private Animator _anim;
    private NavMeshAgent _navMeshAgent;
    private AI_Vitals _aiVitals;

    private List<AIState> _allAIStates = new List<AIState>();

    private float _minFOVAngle;
    private float _maxFOVAngle;

    private Vector2 _smoothDeltaPos;
    private Vector3 _velocity;

    private IEnumerator _clearTargetCoroutine;


    //Properties
    public float maxHealth => _maxHealth;
    public float attackDamage => _attackDamage;

    public AITarget currentTarget => _currentTarget;
    public Animator anim => _anim;
    public NavMeshAgent navMeshAgent => _navMeshAgent;

    #endregion Variables

    #region Unity

    private void Awake()
    {
        //Set Starter State
        _currentState = _startingState;
        _anim = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _aiVitals = GetComponentInChildren<AI_Vitals>();

        if (_anim != null)
            _anim.applyRootMotion = true;

        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = true;
    }

    private void Start()
    {
        //Get All AI States on this Gameobject And Store Them in a List
        var states = GetComponents<AIState>();
        foreach (var state in states)
        {
            if (!_allAIStates.Contains(state))
                _allAIStates.Add(state);
        }

        //Set AI Manager In All AIStates Scripts
        foreach (var state in _allAIStates)
        {
            state.SetAIManagerComponent(this);
        }
    }

    private void FixedUpdate()
    {
      
    }

    private void Update()
    {
        if (base.IsServer)
            HandleZombieState();
    }

    private void LateUpdate()
    {
        if (base.IsServer)
        {
            if (_currentTarget.targetPos != Vector3.zero)
                _currentTarget.SetTargetDistance(Vector3.Distance(transform.position, _currentTarget.targetPos));
        //    _currentTarget.Cleartarget();
        }
    }

    private void OnAnimatorMove()
    {
        //if (_anim == null)
        //    return;

        Vector3 rootPosition = _anim.rootPosition;
        rootPosition.y = _navMeshAgent.nextPosition.y;
        transform.position = rootPosition;
        _navMeshAgent.nextPosition = rootPosition;
    }

    private void OnDrawGizmos()
    {

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.red;
        float rayRange = 10.0f;
        float halfFOV = _maxFieldOfViewAngle / 2.0f;
        float coneDirection = 90;

        Quaternion upRayRotation = Quaternion.AngleAxis(-halfFOV + coneDirection, _inverseForward == false ? Vector3.up : Vector3.down);
        Quaternion downRayRotation = Quaternion.AngleAxis(halfFOV + coneDirection, _inverseForward == false ? Vector3.up : Vector3.down);

        Vector3 upRayDirection = upRayRotation * transform.right * rayRange;
        Vector3 downRayDirection = downRayRotation * transform.right * rayRange;

        Gizmos.DrawRay(transform.position + Vector3.up, upRayDirection + Vector3.up);
        Gizmos.DrawRay(transform.position + Vector3.up, downRayDirection + Vector3.up);
        // Gizmos.DrawLine(transform.position + Vector3.up + downRayDirection, transform.position + Vector3.up + upRayDirection);

        Gizmos.color = Color.yellow;
        //  if (_currentTarget.targetTransform != null)
        Gizmos.DrawSphere(currentTarget.targetPos, .5f);
    }

    #endregion Unity

    #region Functions

    //Handle All AI States On This Agent
    private void HandleZombieState()
    {

        if (_currentState != null)
        {
            if (_nextState != _currentState)
            {
                _currentState.OnEnterState();
                _nextState = _currentState;
                Debug.Log(_currentState.currentState);
            }

            //Run State Logic
            _currentState = _currentState.Tick();
        }
    }

    //Handle Move To Target Using Root Motion
    public void HandleMoveToTarget()
    {

        if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || _navMeshAgent.isPathStale)
        {
            if (_anim != null)
            {
                _anim.SetFloat("Vertical", 0f);
                _anim.SetFloat("Speed", 0f);
            }
            return;
        }

        Vector3 worldDeltaPosition = (_navMeshAgent.nextPosition.normalized) - transform.position;
        worldDeltaPosition.y = 0;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
        _smoothDeltaPos = Vector2.Lerp(_smoothDeltaPos, deltaPosition, smooth);
        _velocity = _smoothDeltaPos / Time.deltaTime;
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            _velocity = Vector2.Lerp(Vector2.zero, _velocity,
                _navMeshAgent.remainingDistance / _navMeshAgent.stoppingDistance);
        }

        if (_anim != null)
        {
            _anim.SetFloat("Vertical", _velocity.magnitude);
            //   _aiZombieManager.anim.SetFloat("Horizontal",  velocity.y);
            _anim.SetFloat("Speed", _navMeshAgent.speed);
        }
    }

    public void HandleAttack(int attackPercentage, bool isCloseAttack)
    {
        if (_anim != null)
        {
            _anim.SetInteger("AttackNumber", attackPercentage);
            if (isCloseAttack) _anim.SetTrigger("CloseAttack");
            else _anim.SetTrigger("LongRangeAttack");
        }
    }

    /// <summary>
    /// Used For Detection
    /// </summary>
    public void FindATargerViaLineOfSight()
    {
        //Detect All Enteties in the radius
       // Collider[] colliders = Physics.OverlapSphere(transform.position, _detectionRadius, _detectionLayer);

        ////for (int i = 0; i < colliders.Length; i++)
        ////{
        ////    //Check if we have a valid target
        ////    if (colliders[i].gameObject != null)
        ////    {
        ////        //Check if he is in our field of view
        ////        Vector3 targetDir = transform.position - colliders[i].transform.position;
        ////        float viewableAngele = Vector3.Angle(targetDir, _inverseForward == false ? transform.forward : transform.forward * -1f);

        ////        if (viewableAngele > _minFOVAngle && viewableAngele < _maxFOVAngle)
        ////        {
        ////            //Check if the target is Blocked by a wall ...
        ////            RaycastHit hit;
        ////            Vector3 startPoint = _playerHeadTransform.position;
        ////            Vector3 endPoint =  new Vector3(colliders[i].transform.position.x, colliders[i].transform.position.y + 1f, colliders[i].transform.position.z); 

        ////            if (!Physics.Linecast(startPoint, endPoint, out hit, _ignoredLineOfSightLayer))
        ////            {
        ////                //Set the AI Target
        ////                float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
        ////                _currentTarget.Settarget(colliders[i].transform, colliders[i].transform.position, distance, AITargetType.Enemy);
        ////                SetClearTargetAfterDelay();
        ////            }
                  
        ////        }
        ////    }
        ////}
    }
    private void SetClearTargetAfterDelay()
    {
        if(_clearTargetCoroutine != null)
        {
            StopCoroutine(_clearTargetCoroutine);
            _clearTargetCoroutine = null;
        }
        _clearTargetCoroutine = ClearTargetAfterDelay();
        StartCoroutine(_clearTargetCoroutine);
    }

    private IEnumerator ClearTargetAfterDelay()
    {
        yield return new WaitForSeconds(_looseSightTime);
        _currentTarget.Cleartarget();
    }

    public void SetRandomPointForPatrol()
    {
        Vector3 target = GetRandomPointOnNavMesh();
        float dist = Vector3.Distance(transform.position, target);
        _currentTarget.Settarget(null,target, dist, AITargetType.Point);
    }

    /// <summary>
    /// Used To Set Field Of View TO Normal
    /// </summary>
    public void SetFOVAngleToNormal()
    {
        _minFOVAngle = _minFieldOfViewAngle;
        _maxFOVAngle = _maxFieldOfViewAngle;
    }

    /// <summary>
    /// Used To Set Field OF View to a higher value when chasing a target
    /// </summary>
    public void SetFOVAngleToTheAlertedState()
    {
        _minFOVAngle = _minFieldOfViewAngleOnHavingTarget;
        _maxFOVAngle = _maxFieldOfViewAngleOnHavingTarget;
    }

    #endregion Functions


    #region Getters

    private Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 10f, 1);
        Vector3 finalPosition = hit.position;
        return finalPosition;
    }

    /// <summary>
    /// Get AIState Component Reference By a state enum,
    /// Eazy to find AIStates References
    /// </summary>
    /// <param name="stateEnum"></param>
    /// <returns></returns>
    public AIState GetAIState(AIStateEnum stateEnum)
    {
        foreach (var state in _allAIStates)
        {
            if (state.currentState == stateEnum) return state;
        }

        return null;
    }

    public AI_Vitals GetAIVitalsComponent()
    {
        return _aiVitals;
    }


    #endregion Getters


}
