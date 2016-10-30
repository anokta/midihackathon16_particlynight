using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

// Main Ruratae class that communicates with the native implementation.
public static class Ruratae {
  // Initializes the system.
  public static void InitializeSystem (int maxParticles, int maxSprings) {
    if (!initialized) {
      Initialize(AudioSettings.outputSampleRate, maxParticles, maxSprings);
      initialized = true;
    }
  }

  // Shuts down the system.
  public static void ShutdownSystem () {
    if (initialized) {
      initialized = false;
      Shutdown();
    }
  }

  // Processes the next buffer.
  public static void Process (float[] data) {
    if (initialized) {
      Process(data.Length, data);
    }
  }

  public static int CreateParticle (Particle particle) {
    int id = -1;
    if (initialized) {
      Vec3 position = Vec3FromUnityVector(particle.transform.position);
      id = CreateParticle(position, particle.recipMass);
    }
    return id;
  }

  public static void DestroyParticle (Particle particle) {
    if (initialized) {
      DestroyParticle(particle.Id);
    }
  }

  public static Vector3 UpdateParticlePosition (Particle particle) {
    if (initialized) {
      Vec3 position = new Vec3();
      IntPtr positionPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Vec3)));
      Marshal.StructureToPtr(position, positionPtr, false);
      GetParticlePosition(particle.Id, positionPtr);
      position = (Vec3) Marshal.PtrToStructure(positionPtr, typeof(Vec3));
      Marshal.FreeHGlobal(positionPtr);
      return new Vector3(position.x, position.y, position.z);
    }
    return particle.transform.position;
  }

  public static void ImpulseParticle (Particle particle, Vector3 force) {
    if (initialized) {
      ImpulseParticle(particle.Id, Vec3FromUnityVector(force));
    }
  }  

  public static void PluckParticle (Particle particle, Vector3 displacement) {
    if (initialized) {
      PluckParticle(particle.Id, Vec3FromUnityVector(displacement));
    }
  }

  public static int CreateSpring (Spring spring) {
    int id = -1;
    if (initialized) {
      id = CreateSpring(spring.particleA.Id, spring.particleB.Id, spring.stiffness, spring.damping, 
                        spring.restLength);
    }
    return id;
  }

  public static void DestroySpring (Spring spring) {
    if (initialized) {
      DestroySpring(spring.Id);
    }
  }

  // Simple three-dimensional vector.
  [StructLayout(LayoutKind.Sequential)]
  private struct Vec3 {
    public float x;
    public float y;
    public float z;
  };

  private static Vec3 Vec3FromUnityVector (Vector3 unityVector) {
    Vec3 vec = new Vec3();
    vec.x = unityVector.x;
    vec.y = unityVector.y;
    vec.z = unityVector.z;
    return vec;
  }

  // Denotes whether the system is initialized.
  private static bool initialized = false;

#if UNITY_IOS
  private const string pluginName = "__Internal";
#else
  private const string pluginName = "rurataeunity";
#endif

  // System handlers.
  [DllImport(pluginName)]
  private static extern void Initialize (int sampleRate, int maxParticles, int maxSprings);

  [DllImport(pluginName)]
  private static extern void Shutdown ();

  [DllImport(pluginName)]
  private static extern void Process (int length, [In, Out]float[] output);

  // Particle handlers.
  [DllImport(pluginName)]
  private static extern int CreateParticle (Vec3 position, float recipMass);

  [DllImport(pluginName)]
  private static extern void DestroyParticle (int particleId);

  [DllImport(pluginName)]
  private static extern void GetParticlePosition (int particleId, [In, Out]IntPtr positionPtr);

  [DllImport(pluginName)]
  private static extern void ImpulseParticle (int particleId, Vec3 force);

  [DllImport(pluginName)]
  private static extern void PluckParticle (int particleId, Vec3 displacement);

  // Spring handlers.
  [DllImport(pluginName)]
  private static extern int CreateSpring (int particleIdA, int particleIdB, float stiffness,
                                          float damping, float restlength);

  [DllImport(pluginName)]
  private static extern void DestroySpring (int springId);
}
