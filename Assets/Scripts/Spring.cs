using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Ruratae spring.
public class Spring : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Particle particleA;

    public Particle particleB;

    public float stiffness = 3000000.0f;

    public float damping = 0.0f;

    private const float minThickness = 0.1f;
    private const float minPercentage = 0.0f;
    private const float maxPercentage = 1.25f;

    public float restLength = 0.0f;
    private float restLengthNormalized = 0.5f * (minPercentage + maxPercentage);

    private Vector2 pressPosition;
    private float pressRestLengthNormalized;

    // Unique id.
    public int Id
    {
        get { return id; }
    }

    private int id = -1;

    public void Initialize()
    {
        if (id == -1)
        {
            GetComponent<CapsuleCollider>().enabled = false;
      id = Ruratae.CreateSpring(this);
        }
    }

    public void Shutdown()
    {
        if (id != -1)
    {
            GetComponent<CapsuleCollider>().enabled = true;
            Ruratae.DestroySpring(this);
            id = -1;
        }
    }

    void Update()
    {
        if (particleA == null || particleB == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        SetTransform(particleA.transform.position, particleB.transform.position);
    }

    public void SetTransform(Vector3 begin, Vector3 end)
    {
        Vector3 direction = end - begin;
        transform.position = 0.5f * (begin + end);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        float length = direction.magnitude;

        if (GameManager.editMode)
        {
            restLength = 0.0f;// restLengthNormalized * length;
        }
        float thickness = 0.0f;
        if (length > 0.0f)
        {
            thickness = minThickness * (1.0f + restLength / length);
        }
        transform.localScale = new Vector3(thickness, 0.5f * length, thickness);
    }

    // Implements |IPointerClickHandler.OnPointerClick|.
    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject.Destroy(gameObject);
    }

    // Implements |IPointerDownHandler.OnPointerDown|.
    public void OnPointerDown(PointerEventData eventData)
    {
        pressPosition = eventData.pressPosition;
        pressRestLengthNormalized = restLengthNormalized;
    }

    // Implements |IPointerUpHandler.OnPointerUp|.
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GameManager.editMode)
        {
            return;
        }
        if (!eventData.dragging)
        {
            GameObject.Destroy(gameObject);
        }
    }

    // Implements |IDragHandler.OnDrag|.
    public void OnDrag(PointerEventData eventData)
    {
        if (!GameManager.editMode || !Input.GetKey(KeyCode.LeftControl))
        {
            return;
        }
        Vector2 difference = eventData.position - pressPosition;
        float delta = 2.0f * (difference.x / Screen.width + difference.y / Screen.height);
        restLengthNormalized = Mathf.Clamp(pressRestLengthNormalized + delta, minPercentage,
                                           maxPercentage);
    }

    private Vector3 WorldFromScreenPosition(Camera camera, Vector3 screenPosition)
    {
        screenPosition.z = -camera.transform.position.z;
        return camera.ScreenToWorldPoint(screenPosition);
    }
}