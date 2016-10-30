using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Instrument : MonoBehaviour {
  // Instrument particles.
  private Particle[] particles = null;

  // Instrument springs.
  private Spring[] springs = null;

  [SerializeField]
  private AudioSource source = null;
  private float[] monoData = null;

  private int keyNote = 48;
  private int[] inverseMajorScale = { 0, 0, 1, 1, 2, 3, 2, 4, 3, 5, 4, 6 };

  List<Particle> movables = new List<Particle>();

  private static int CompareParticle (Particle lhs, Particle rhs) {
    float result = lhs.transform.position.x - rhs.transform.position.x;
    if (result == 0.0f) {
      result = lhs.transform.position.y - rhs.transform.position.y;
    }
    return (int)result;
  }

  void Awake () {
    int numFrames = AudioSettings.GetConfiguration().dspBufferSize;
    monoData = new float[numFrames];
    if (source == null) {
      source = gameObject.AddComponent<AudioSource>();
    }
  }

  void OnDestroy () {
    monoData = null;
    Destroy(source);
  }

  void OnEnable () {
    Initialize();
  }

  void OnDisable () {
    Shutdown();
  }

  void Update () {
    // TODO(anokta): Get positions & update..
  }

  void OnNoteOn (MidiJack.MidiChannel channel, int note, float velocity) {
    int index = note - keyNote;
    int numOctavs = index / 12;
    int particleIndex = 7 * numOctavs + inverseMajorScale[(index + 60)  % inverseMajorScale.Length];
    Vector3 direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
    float strength = velocity < 0.95f ? Mathf.Min(Mathf.Max(2 * velocity, 0.5f), 1.5f) : Mathf.Pow(2 * velocity, 2.0f);
    Ruratae.ImpulseParticle(movables[particleIndex % movables.Count], 0.05f * direction * strength);//2 * Vector3.one * Mathf.Pow(velocity, 2.0f));
  }

  void OnAudioFilterRead (float[] data, int channels) {
    if (GameManager.editMode) {
      return;
    }
    // Process the next buffer block.
    Ruratae.Process(monoData);
    for (int frame = 0; frame < monoData.Length; ++frame) {
      for (int ch = 0; ch < channels; ++ch) {
        data[channels * frame + ch] = monoData[frame];
      }
    }
  }

  public void Initialize () {
    // Initialize the system.
    particles = GameObject.FindObjectsOfType<Particle>();
    springs = GameObject.FindObjectsOfType<Spring>();
    Ruratae.InitializeSystem(particles.Length, springs.Length);
    // Initialize particles.
    for (int particle = 0; particle < particles.Length; ++particle) {
      particles[particle].Initialize();
    }
    // Initialize springs.
    for (int spring = 0; spring < springs.Length; ++spring) {
      if (springs[spring].enabled) {
        springs[spring].Initialize();
      }
    }
    for (int i = 0; i < particles.Length; ++i) {
      if (particles[i].recipMass > 0.0f) {
        movables.Add(particles[i]);
      }
    }
    movables.Sort(CompareParticle);
    MidiJack.MidiMaster.noteOnDelegate += OnNoteOn;
  }

  public void Shutdown () {
    MidiJack.MidiMaster.noteOnDelegate -= OnNoteOn;
    // Shutdown particles.
    for (int particle = 0; particle < particles.Length; ++particle) {
      if (particles[particle] != null) {
        particles[particle].Shutdown();
      }
    }
    // Shutdown springs.
    for (int spring = 0; spring < springs.Length; ++spring) {
      if (springs[spring] != null && springs[spring].enabled) {
        springs[spring].Shutdown();
      }
    }
    Ruratae.ShutdownSystem();
  }
}