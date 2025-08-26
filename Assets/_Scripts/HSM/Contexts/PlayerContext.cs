using System;
using UnityEngine;
using UnityEngine.Splines;

namespace stal.HSM.Contexts
{
  [Serializable]
  public class PlayerContext
  {
    [Header("Monobehavior Components")]
    public Transform transform;
    public Transform bulletSpawnTransform;
    public Rigidbody2D rigidbody2D;
    public BoxCollider2D boxCollider2D;
    public Camera mainCamera;

    [Header("Prefabs")]
    public GameObject bramble;
    public GameObject bullet;

    [Header("Misc.")]
    [ReadOnly] public float targetSpeed;
    [ReadOnly] public float coyoteTime;
    [ReadOnly] public float jumpBufferWindow;
    [ReadOnly] public int jumpCount;
    [ReadOnly] public bool wasGroundedLastFrame;
    [ReadOnly] public bool jumpEndEarly = false;

    [Header("Debug")]
    public bool drawDebugGizmos;
    [ReadOnly] public string statePath;
    [HideInInspector] public string previousStatePath;
    [ReadOnly] public Vector2 mouseScreenToWorldPos;
  }
}
