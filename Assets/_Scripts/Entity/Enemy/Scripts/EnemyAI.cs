using Pathfinding;
using UnityEngine;

/* -----------------------------------------------
 * DESCRIPTION: 
 *   Script for generic AI chase behavior
 *
 *
 * -----------------------------------------------
*/

public class EnemyAI : MonoBehaviour
{
  [SerializeField] private Transform EnemyGFXTransform;
  [SerializeField] private float speed = 2f;
  [SerializeField] private Transform target;
  [SerializeField] private float nextWaypointDistance = 3f;
  [SerializeField] private float maxChaseDistance = 12f;

  private Path _path;
  private float _speed;
  private int _currentWaypoint = 0;
  private bool _reachedTarget = false;
  private float _distanceFromTarget = -1f;
  private Seeker _seeker;
  private Rigidbody2D _rb;
  private Vector3 _originalLocation;

  private void Start()
  {
    _speed = 100f * speed;
    _seeker = GetComponent<Seeker>();
    _rb = GetComponent<Rigidbody2D>();
    _originalLocation = _rb.transform.position;
    _distanceFromTarget = Vector2.Distance(_rb.position, target.position);

  }

  // Update is called once per frame
  private void FixedUpdate()
  {
    _distanceFromTarget = Vector2.Distance(_rb.position, target.position);
    _reachedTarget = _distanceFromTarget < 1f;

    // get all the early exit conditions out of the way
    if (_distanceFromTarget > maxChaseDistance || _reachedTarget)
    {
      if (IsInvoking("UpdatePath"))
      {
        CancelInvoke("UpdatePath");
        _seeker.CancelCurrentPathRequest();
      }
      if (_rb.transform.position != _originalLocation)
      {
        _rb.transform.position = _originalLocation;
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

    if (_distanceFromTarget < 1f)
    {
      _reachedTarget = true;
      return;
    }

    // only want to move if the right conditions are met
    MoveEnemyOnPath();
  }


  private void UpdatePath()
  {
    if (_distanceFromTarget > maxChaseDistance) return;
    if (_seeker.IsDone())
    {
      _seeker.StartPath(_rb.position, target.position, OnPathComplete);
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

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.CompareTag("Player"))
    {
      Debug.Log("OUCH!!");
    }
  }
}
