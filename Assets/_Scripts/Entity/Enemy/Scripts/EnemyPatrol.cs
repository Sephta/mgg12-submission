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

    public List<Transform> patrolPoints;
    public Transform player;
    public Transform EnemyGFXTransform;
    public float speed = 2f;
    public float rangeAwakeDistance = 100f;
    public float beginChaseDistance = 5f;
    public float maxChaseDistance = 12f;
    public float nextWaypointDistance = 3f;

    private float _speed;
    private Path _path;
    private int _currentWaypoint = 0;
    private int _currentPatrolPoint = 0;
    private Transform _currentTarget;
    private bool _reachedTarget = false;
    private bool _isPatrolling = false;
    private bool _isChasing = false;
    private float _distanceFromTarget = -1f;
    private float _distanceFromPlayer = -1f;
    private Seeker _seeker;
    private Rigidbody2D _rb;
    private Collider2D _collision;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _speed = 100f * speed;
        _seeker = GetComponent<Seeker>();
        _rb = GetComponent<Rigidbody2D>();
        _collision = GetComponent<Collider2D>();
        _currentTarget = patrolPoints[_currentPatrolPoint];
        _distanceFromTarget = Vector2.Distance(_rb.position, _currentTarget.position);
        _distanceFromPlayer = Vector2.Distance(_rb.position, player.position);


        InvokeRepeating("UpdatePath", 0f, 0.5f);

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _distanceFromTarget = Vector2.Distance(_rb.position, _currentTarget.position);
        _distanceFromPlayer = Vector2.Distance(_rb.position, player.position);
        _isPatrolling = _distanceFromPlayer <= rangeAwakeDistance;
        _reachedTarget = _distanceFromTarget < 1f;

        Debug.Log("Distance from point #" + _currentPatrolPoint + " : " + _distanceFromTarget);
        Debug.Log("Has reached target? " + _reachedTarget);

        if (_distanceFromPlayer > rangeAwakeDistance)
        {
            if (IsInvoking("UpdatePath"))
            {
                CancelInvoke("UpdatePath");
                _seeker.CancelCurrentPathRequest();
            }
            return;
        }

        if (!IsInvoking("UpdatePath"))
        {
            InvokeRepeating("UpdatePath", 0f, 0.5f);
            return;
        }

        if (_path == null) return;

        if (_currentWaypoint >= _path.vectorPath.Count)
            return;

        if (_reachedTarget)
        {
            _currentPatrolPoint = _currentPatrolPoint == patrolPoints.Count - 1 ? 0 : _currentPatrolPoint + 1;
            _currentTarget = patrolPoints[_currentPatrolPoint];
            return;
        }

        MoveEnemyOnPath();
    }

    private void UpdatePath()
    {
        if (_seeker.IsDone())
        {
            if (_reachedTarget)
            {
                Debug.Log("Reached Target...");
                _currentPatrolPoint = _currentPatrolPoint == patrolPoints.Count - 1 ? 0 : _currentPatrolPoint + 1;
                _currentTarget = patrolPoints[_currentPatrolPoint];
            }
            Debug.Log("current patrol point = " + _currentPatrolPoint);
            _seeker.StartPath(_rb.position, _currentTarget.position, OnPathComplete);
        }
    }

    private void OnPathComplete(Path path)
    {
        if (path.error)
        {
            Debug.LogError("There was a pathing error. Exiting early OnPathComplete early.");
            return;
        }
        _path = path;
        _currentWaypoint = 0;
    }


    private void MoveEnemyOnPath()
    {
        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
        Vector2 force = direction * _speed * Time.deltaTime;

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
