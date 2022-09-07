using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using static Noise;

public class NoiseVisualization : Visualization {

  [SerializeField]
  SpaceTRS domain = new SpaceTRS {
    scale = 8f
  };

  [SerializeField]
  int seed;

  static int noiseId = Shader.PropertyToID("_Noise");

  NativeArray<float4> noise;

  ComputeBuffer noiseBuffer;

  static ScheduleDelegate[] noiseJobs = {
    Job<Lattice1D>.ScheduleParallel,
    Job<Lattice2D>.ScheduleParallel,
    Job<Lattice3D>.ScheduleParallel,
  };

  [SerializeField, Range(1, 3)]
  int dimensions = 3;

  protected override void EnableVisualization (int dataLength, MaterialPropertyBlock propertyBlock) {
    noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
    noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
    propertyBlock.SetBuffer(noiseId, noiseBuffer);
  }

  protected override void DisableVisualization () {
    noise.Dispose();
    noiseBuffer.Release();
    noiseBuffer = null;
  }

  protected override void UpdateVisualization (NativeArray<float3x4> positions, int resolution, JobHandle handle) {
    noiseJobs[dimensions - 1](positions, noise, seed, domain, resolution, handle).Complete();
    noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
  }

}
