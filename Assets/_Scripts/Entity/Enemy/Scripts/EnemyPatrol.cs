using System.Collections.Generic;
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
  [SerializeField] private Transform player;
  [SerializeField] private Transform EnemyGFXTransform;
  [SerializeField] private float _patrolSpeed = 2f;
  [SerializeField] private float _chaseSpeed = 4f;
  [SerializeField] private float rangeAwakeDistance = 100f;
  [SerializeField] private float nextWaypointDistance = 1f;

  private Path _path;
  private Transform _currentTarget;
  private float _speed = 2f;
  private int _currentWaypoint = 0;
  private int _currentPatrolPoint = 0;
  private bool _reachedTarget = false;
  private bool _isPatrolling = false;
  private bool _isChasing = false;
  private float _distanceFromTarget = -1f;
  private float _distanceFromPlayer = -1f;
  private Seeker _seeker;
  private Rigidbody2D _rb;

  private void Start()
  {
    _seeker = GetComponent<Seeker>();
    _rb = GetComponent<Rigidbody2D>();
    _currentTarget = patrolPoints[_currentPatrolPoint];
    _distanceFromTarget = Vector2.Distance(_rb.position, _currentTarget.position);
    _distanceFromPlayer = Vector2.Distance(_rb.position, player.position);
    _speed = _patrolSpeed;
  }

  // Update is called once per frame
  private void FixedUpdate()
  {
    _distanceFromTarget = Vector2.Distance(_rb.position, _currentTarget.position);
    _distanceFromPlayer = Vector2.Distance(_rb.position, player.position);
    _isPatrolling = _distanceFromPlayer <= rangeAwakeDistance;
    _reachedTarget = _distanceFromTarget < 1f;

    if (_distanceFromPlayer > rangeAwakeDistance)
    {
      if (IsInvoking(nameof(UpdatePath)))
      {
        CancelInvoke(nameof(UpdatePath));
        _seeker.CancelCurrentPathRequest();
      }
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
      _speed = _chaseSpeed;
      Debug.Log("Player has ENTERED our chase range");
    }
  }

  private void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _isChasing = false;
      _isPatrolling = true;
      _speed = _patrolSpeed;
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
}
