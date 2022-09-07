using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

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
    handle.Complete();
    noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
  }
}
