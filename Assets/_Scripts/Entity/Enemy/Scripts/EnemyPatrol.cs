using System.Collections;
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

  private Path _path;
  private Transform _currentTarget;
  private int _currentWaypoint = 0;
  private int _currentPatrolPoint = 0;
  private bool _reachedTarget = false;
  private bool _isPatrolling = false;
  private bool _isChasing = false;
  private bool _isDead = false;
  private float _distanceFromTarget = -1f;
  private float _distanceFromPlayer = -1f;
  private Seeker _seeker;
  private Rigidbody2D _rb;
  private Vector3 _originalLocation;



  private void Awake()
  {
    if (_enemyAttributesData == null)
    {
      Debug.LogError(name + " does not have a reference to EnemyAttributesDataSO in the inspector. Disabling gameobject to avoid null object errors.");
      gameObject.SetActive(false);
    }

    if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    if (player != null) _enemyAttributesData.PlayerTransform = player;
  }

  private void Start()
  {
    _seeker = GetComponent<Seeker>();
    _rb = GetComponent<Rigidbody2D>();
    _originalLocation = _rb.transform.position;
    _currentTarget = patrolPoints[_currentPatrolPoint];
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
      InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
      return;
    }

    if (_path == null) return;

    if (_currentWaypoint >= _path.vectorPath.Count)
      return;

    if (_reachedTarget && _isPatrolling)
    {
      UpdatePatrolTarget();
      return;
    }

    MoveEnemyOnPath();
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _isChasing = true;
      _isPatrolling = false;
      _speed = _enemyAttributesData.ChaseSpeed;
      Debug.Log("Player has ENTERED our chase range");
    }
  }

  private void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _isChasing = false;
      _isPatrolling = true;
      _speed = _enemyAttributesData.PatrolSpeed;
      UpdatePatrolTarget();
      Debug.Log("Player has EXITED our chase detection");
    }
  }

  // only call if is currently patrolling and has reached target
  // or is exiting chase behavior
  private void UpdatePatrolTarget()
  {
    _currentPatrolPoint = _currentPatrolPoint == patrolPoints.Count - 1 ? 0 : _currentPatrolPoint + 1;
    _currentTarget = patrolPoints[_currentPatrolPoint];
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
    Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
    Vector2 force = direction * _speed;

    _rb.AddForce(force);

    float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

    if (distance < nextWaypointDistance)
    {
      _currentWaypoint++;
    }

    ChangeSpriteDirection(force);
  }

  private void ChangeSpriteDirection(Vector2 force)
  {
    if (force.x >= 0.01f)
    {
      EnemyGFXTransform.localScale = new Vector3(-1f, 1f, 1f);
    }
    else if (force.x <= -0.01f)
    {
      EnemyGFXTransform.localScale = new Vector3(1f, 1f, 1f);
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
