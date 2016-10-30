using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
  public GameObject particlePrefab;
  public GameObject springPrefab;

  public TextMesh modeText;

  public float rotationSpeed = 4.0f;
  private float currentRotation = 0.0f;

  public static bool editMode = true;

  private GameObject instrumentObject = null;

  void Update () {
    Camera.main.GetComponent<Skybox>().material.SetFloat("_Rotation", currentRotation % 360.0f);
    currentRotation += rotationSpeed * Time.deltaTime;

    if(Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
    if(Input.GetKeyDown(KeyCode.Space)) {
      editMode = !editMode;
      if(editMode && instrumentObject != null) {
        GameObject.Destroy(instrumentObject);
        instrumentObject = null;
      } else if(!editMode && instrumentObject == null) {
        instrumentObject = new GameObject("Instrument");
        instrumentObject.AddComponent<Instrument>();
      }
      // TEMP debugging..
      modeText.text = editMode ? "Edit Mode" : "";
      //modeText.color = 0.6f * (editMode ? Color.gray : Color.white);
    }
	}

  public Particle InstantiateParticle (Vector3 position) {
    GameObject particleObject = GameObject.Instantiate(particlePrefab, transform) as GameObject;
    Particle particle = particleObject.GetComponent<Particle>();
    particle.transform.position = position;
    return particle;
  }

  public void DestroyParticle (Particle particle) {
    GameObject.Destroy(particle.gameObject);
  }

  public Spring InstantiateSpring (Particle a, Particle b) {
    GameObject springObject = GameObject.Instantiate(springPrefab, transform) as GameObject;
    Spring spring = springObject.GetComponent<Spring>();
    spring.particleA = a;
    spring.particleB = b;
    return spring;
  }

  public void DestroySpring (Spring spring) {
    GameObject.Destroy(spring.gameObject);
  }
}
