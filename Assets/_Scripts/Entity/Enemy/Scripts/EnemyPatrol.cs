using System.Collections.Generic;
using NaughtyAttributes;
using Pathfinding;
using UnityEngine;
public class EnemyPatrol : MonoBehaviour
{
  /* -----------------------------------------------
   * DESCRIPTION: 
   *   Script for generic patrol behavior
   *   I have some stuff in here for chase behavior, but that is not implimented and I am realizing 
   *   that I should probably create a state machine for enemy behavior if I want to swap from patrol to chase...
   * -----------------------------------------------
  */

  [SerializeField] private List<Transform> patrolPoints;
  [SerializeField, ReadOnly] private Transform player;
  [SerializeField] private Transform EnemyGFXTransform;

  [Header("Attributes Data")]

  [SerializeField, Expandable] private EnemyAttributesDataSO _enemyAttributesData;

  [SerializeField] private float nextWaypointDistance = 1f;

  [Header("Debug")]

  [SerializeField, ReadOnly] private float _speed = 2f;
  [SerializeField, ReadOnly] private Vector2 _normalizedDirectionFromPath = Vector2.zero;
  [SerializeField, ReadOnly] private float _directionX = 0f;
  [SerializeField, ReadOnly] private float _targetSpeed = 0f;
  [SerializeField, ReadOnly] private float _distanceFromTarget = -1f;
  [SerializeField, ReadOnly] private float _distanceFromPlayer = -1f;




  private Path _path;
  private Transform _currentTarget;

  private int _currentWaypoint = 0;
  private int _currentPatrolPoint = 0;
  private bool _reachedTarget = false;
  private bool _isPatrolling = false;
  private bool _isChasing = false;
  private bool _isDead = false;
  private Seeker _seeker;
  private Rigidbody2D _rb;
  private Transform _originalTransform;
  private Vector3 _originalLocation;
  private EnemyAnimatorController _enemyAnimatorController;


  private void Awake()
  {
    if (_enemyAttributesData == null)
    {
      Debug.LogError(name + " does not have a reference to EnemyAttributesDataSO in the inspector. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }
    if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    if (player != null) _enemyAttributesData.PlayerTransform = player;
    if (_enemyAnimatorController == null) _enemyAnimatorController = GetComponentInChildren<EnemyAnimatorController>();
  }

  private void Start()
  {
    _seeker = GetComponent<Seeker>();
    _rb = GetComponent<Rigidbody2D>();
    _originalLocation = _rb.transform.position;
    _originalTransform = _rb.transform;
    _currentTarget = patrolPoints.Count > 0 ? patrolPoints[_currentPatrolPoint] : _originalTransform;
    _distanceFromTarget = Vector2.Distance(_rb.position, _currentTarget.position);
    _distanceFromPlayer = Vector2.Distance(_rb.position, player.position);
    _speed = _enemyAttributesData.PatrolSpeed;
  }

  // Update is called once per frame
  private void FixedUpdate()
  {
    _distanceFromTarget = Vector2.Distance(_rb.position, _currentTarget.position);
    _distanceFromPlayer = Vector2.Distance(_rb.position, player.position);
    _isPatrolling = _distanceFromPlayer <= _enemyAttributesData.RangeAwakeDistance;
    _reachedTarget = _distanceFromTarget < 1f;

    if (_path == null) Debug.Log("Path is null");

    if (_isDead)
    {
      DisposeTheBody(_distanceFromPlayer > _enemyAttributesData.RangeAwakeDistance);
      return;
    }

    if (_distanceFromPlayer > _enemyAttributesData.RangeAwakeDistance)
    {
      ResetPositionAndGoToSleep();
      return;
    }

    if (!IsInvoking(nameof(UpdatePath)))
    {
      Debug.Log("!IsInvoking");
      InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
      return;
    }

    if (_path == null) return;

    if (_currentWaypoint >= _path.vectorPath.Count)
      return;

    if (_reachedTarget && _isPatrolling && !_isChasing)
    {
      UpdatePatrolTarget();
      return;
    }

    _normalizedDirectionFromPath = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
    _directionX = 0f;
    if (_normalizedDirectionFromPath.x > 0) _directionX = 1;
    else if (_normalizedDirectionFromPath.x < 0) _directionX = -1;

    _targetSpeed = _directionX * _speed;

    MoveEnemyOnPath();
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _isChasing = true;
      _isPatrolling = false;
      _speed = _enemyAttributesData.ChaseSpeed;
    }
  }

  private void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _isChasing = false;
      _isPatrolling = true;
      _reachedTarget = false;
      _speed = _enemyAttributesData.PatrolSpeed;
      UpdatePatrolTarget();
    }
  }

  // only call if is currently patrolling and has reached target
  // or is exiting chase behavior
  private void UpdatePatrolTarget()
  {
    if (patrolPoints.Count == 0)
    {
      _currentTarget = _originalTransform;
    }
    else
    {
      _currentPatrolPoint = _currentPatrolPoint == patrolPoints.Count - 1 ? 0 : _currentPatrolPoint + 1;
      _currentTarget = patrolPoints[_currentPatrolPoint];
    }
  }

  private void UpdatePath()
  {
    if (_seeker.IsDone())
    {
      if (_reachedTarget && _isPatrolling)
      {
        UpdatePatrolTarget();
      }
      if (_isChasing && _currentTarget != player)
      {
        _currentTarget = player;
      }
      _seeker.StartPath(_rb.position, _currentTarget.position, OnPathComplete);
    }
  }

  private void OnPathComplete(Path path)
  {
    if (path.error)
    {
      return;
    }
    _path = path;
    _currentWaypoint = 0;
  }



  private void MoveEnemyOnPath()
  {
    float accelRate;

    if (Mathf.Abs(_targetSpeed) > 0.01f)
    {
      accelRate = _enemyAttributesData.Acceleration;
    }
    else
    {
      accelRate = _enemyAttributesData.Deceleration;
    }

    float delta = _targetSpeed - _rb.linearVelocityX;

    float force = delta * accelRate;

    _rb.AddForce(force * Vector2.right, ForceMode2D.Force);

    float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

    if (distance < nextWaypointDistance)
    {
      _currentWaypoint++;
    }
  }

  private void ResetPositionAndGoToSleep()
  {
    if (IsInvoking(nameof(UpdatePath)))
    {
      CancelInvoke(nameof(UpdatePath));
      _seeker.CancelCurrentPathRequest();
    }
    if (_rb.transform.position != _originalLocation)
    {
      _rb.transform.position = _originalLocation;
    }
  }

  private void DisposeTheBody(bool playerOutOfRange)
  {
    _seeker.CancelCurrentPathRequest();
    if (IsInvoking(nameof(UpdatePath)))
    {
      CancelInvoke(nameof(UpdatePath));
    }

    if (playerOutOfRange)
    {
      Destroy(gameObject);
    }
  }

  // I'm making a method in case we need to do more later upon disposal
  public void KillYourself()
  {
    _isDead = true;
    _rb.constraints = RigidbodyConstraints2D.FreezeAll;
    _seeker.CancelCurrentPathRequest();
    if (IsInvoking(nameof(UpdatePath)))
    {
      CancelInvoke(nameof(UpdatePath));
    }
  }



}
