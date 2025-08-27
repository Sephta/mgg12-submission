using System;
using UnityEngine;

[Serializable]
public class AnimatorComponentReferences
{
  [SerializeField] public Animator animator;
  [SerializeField] public SpriteRenderer spriteRenderer;
}

[Serializable]
public class CombatAnimatorComponentReferences : AnimatorComponentReferences
{
  [SerializeField] public BoxCollider2D playerHitZone;
  [SerializeField] public Rigidbody2D playerRigidBody;
}
