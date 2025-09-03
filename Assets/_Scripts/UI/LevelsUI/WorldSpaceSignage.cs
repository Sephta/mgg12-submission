using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WorldSpaceSignage : MonoBehaviour
{
  [Serializable]
  private class ComponentReferences
  {
    public CircleCollider2D circleCollider2D;
    public TextMeshProUGUI tmPro;
    public RectTransform canvasRectTransform;
    public RectTransform signageRectTransform;
  }

  [Space(10)]

  [Header("Component References")]

  [SerializeField] private ComponentReferences _refs;

  [Space(10)]

  [Header("Signage Settings")]

  [SerializeField, Range(0.1f, 10f), OnValueChanged(nameof(OnValueChangedCallback))]
  private float _detectionRadius = 3f;

  [SerializeField, OnValueChanged(nameof(OnValueChangedCallback))]
  private Vector2 _offset = Vector2.zero;

  [SerializeField, OnValueChanged(nameof(OnValueChangedCallback))]
  private Vector2 _widthAndHeight = new(4f, 1f);

  [SerializeField, Range(0.1f, 1f), OnValueChanged(nameof(OnValueChangedCallback))]
  private float _fontSizeScaler = 0.1f;

  [SerializeField, OnValueChanged(nameof(OnValueChangedCallback))]
  private int _fontSize = 12;

  [Space(10)]

  [SerializeField, ResizableTextArea, OnValueChanged(nameof(OnValueChangedCallback))]
  private string _text = "";

  [Space(10)]

  [Header("Debug Settings")]

  [SerializeField, OnValueChanged(nameof(OnValueChangedCallback))]
  private bool _displayText = true;

  /* ---------------------------------------------------------------- */
  /*                           Unity Functions                        */
  /* ---------------------------------------------------------------- */

  private void Awake()
  {
    if (_refs.circleCollider2D == null) _refs.circleCollider2D = GetComponent<CircleCollider2D>();
    if (_refs.canvasRectTransform == null || _refs.signageRectTransform == null)
    {
      Debug.LogError("<" + name + ":" + gameObject.GetInstanceID() + "> | either canvas or signage RectTransfrom is not plugged in through the inspector. Deactivating GameObject...");
      gameObject.SetActive(false);
    }
    if (_refs.tmPro == null)
    {
      Debug.LogError("<" + name + ":" + gameObject.GetInstanceID() + "> | TextMeshPro - text is not plugged in through the inspector. Deactivating GameObject...");
      gameObject.SetActive(false);
    }
  }

  // private void Start() {}

  private void OnEnable()
  {
    OnValueChangedCallback();
    _refs.tmPro.enabled = false;
  }

  // private void OnDisable() {}
  // private void Update() {}
  // private void FixedUpdate() {}

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _refs.tmPro.enabled = true;
    }
  }

  private void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.gameObject.CompareTag("Player"))
    {
      _refs.tmPro.enabled = false;
    }
  }

  /* ---------------------------------------------------------------- */
  /*                               PUBLIC                             */
  /* ---------------------------------------------------------------- */

  /* ---------------------------------------------------------------- */
  /*                               PRIVATE                            */
  /* ---------------------------------------------------------------- */

  private void OnValueChangedCallback()
  {
    if (_refs.tmPro == null || _refs.canvasRectTransform == null || _refs.signageRectTransform == null) return;

    // _refs.canvasRectTransform.position = _offset;
    _refs.signageRectTransform.localPosition = _offset;

    // _refs.canvasRectTransform.sizeDelta = _widthAndHeight;
    _refs.signageRectTransform.sizeDelta = _widthAndHeight;

    _refs.tmPro.fontSize = _fontSizeScaler * _fontSize;
    _refs.tmPro.text = _text;

    _refs.tmPro.enabled = _displayText;

    if (_refs.circleCollider2D == null) return;

    _refs.circleCollider2D.radius = _detectionRadius;
  }
}
