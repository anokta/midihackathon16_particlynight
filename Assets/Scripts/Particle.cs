using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Ruratae particle.
public class Particle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
  public float recipMass = 0.0f;

  // Unique id.
  public int Id {
    get { return id; }
  }

  public Material movable, stationary;

  private int id = -1;

  private Vector3 editPosition;

  private Vector3 pressOffset;

  void Update () {
    // Update scale.
    transform.GetComponent<Renderer>().material = recipMass > 0.0f ? movable : stationary;
    if(!GameManager.editMode && id != -1) {
      transform.position = Vector3.Lerp(transform.position, Ruratae.UpdateParticlePosition(this),
                                        16 * Time.deltaTime);
    }
  }

  public void Initialize () {
    if (id == -1) {
      editPosition = transform.position;
      id = Ruratae.CreateParticle(this);
    }
  }

  public void Shutdown () {
    if (id != -1) {
      Ruratae.DestroyParticle(this);
      transform.position = editPosition;
      id = -1;
    }
  }

  // Implements |IPointerDownHandler.OnPointerDown|.
  public void OnPointerDown (PointerEventData eventData) {
    if (GameManager.editMode) {
      // Set position.
      Vector3 position = WorldFromScreenPosition(eventData.pressEventCamera,
                                                 eventData.pressPosition);
      pressOffset = transform.position - position;
    } else {
      // Strike!
      if (Input.GetMouseButtonDown(0)) {
        Ruratae.ImpulseParticle(this, new Vector3(Random.Range(-1.0f, 1.0f), 
                                                  Random.Range(-1.0f, 1.0f), 
                                                  Random.Range(-1.0f, 1.0f)));
      } else if (Input.GetMouseButtonDown(1)) {
        Ruratae.PluckParticle(this, 4.0f * new Vector3(Random.Range(-1.0f, 1.0f), 
                                                       Random.Range(-1.0f, 1.0f), 
                                                       Random.Range(-1.0f, 1.0f)));
      }
    }
  }

  // Implements |IPointerUpHandler.OnPointerUp|.
  public void OnPointerUp (PointerEventData eventData) {
    if (!GameManager.editMode) {
      return;
    }
    if (Input.GetMouseButtonUp(1)) {
      recipMass = 1.0f - recipMass;
    } else if (!eventData.dragging) {
      GameObject.Destroy(gameObject);
    }
  }

  // Implements |IDragHandler.OnDrag|.
  public void OnDrag (PointerEventData eventData) {
    if (!GameManager.editMode || Input.GetKey(KeyCode.LeftControl)) {
      return;
    }
    Vector3 position = WorldFromScreenPosition(eventData.pressEventCamera, eventData.position);
    transform.position = position + pressOffset;
  }

  private Vector3 WorldFromScreenPosition (Camera camera, Vector3 screenPosition) {
    screenPosition.z = -camera.transform.position.z;
    return camera.ScreenToWorldPoint(screenPosition);
  }
}
