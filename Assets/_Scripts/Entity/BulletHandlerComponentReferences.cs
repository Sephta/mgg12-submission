using System;
using UnityEngine;

[Serializable]
public class BulletHandlerComponentReferences
{
  [SerializeField] public Rigidbody2D rigidbody2D;
  [SerializeField] public CircleCollider2D circleCollider2D;
  [SerializeField] public SpriteRenderer spriteRenderer;
}