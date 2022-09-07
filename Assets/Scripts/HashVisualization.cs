using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class HashVisualization : Visualization {

  [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
  struct HashJob : IJobFor {

    [WriteOnly]
    public NativeArray<uint4> hashes;

    [ReadOnly]
    public NativeArray<float3x4> positions;

    public SmallXXHash4 hash;

    public float3x4 domainTRS;

    public void Execute(int i) {
      float4x3 p = TransformPositions(domainTRS, transpose(positions[i]));

      int4 u = (int4)floor(p.c0);
      int4 v = (int4)floor(p.c1);
      int4 w = (int4)floor(p.c2);
      hashes[i] = hash.Eat(u).Eat(v).Eat(w);
    }

    float4x3 TransformPositions (float3x4 trs, float4x3 p) => float4x3(
      trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x,
      trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y,
      trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z
    );

  }

  [SerializeField]
  SpaceTRS domain = new SpaceTRS {
    scale = 8f
  };

  [SerializeField]
  int seed;

  static int hashesId = Shader.PropertyToID("_Hashes");

  NativeArray<uint4> hashes;

  ComputeBuffer hashesBuffer;

  protected override void EnableVisualization (int dataLength, MaterialPropertyBlock propertyBlock) {
    hashes = new NativeArray<uint4>(dataLength, Allocator.Persistent);
    hashesBuffer = new ComputeBuffer(dataLength * 4, 4);
    propertyBlock.SetBuffer(hashesId, hashesBuffer);
  }

  protected override void DisableVisualization () {
    hashes.Dispose();
    hashesBuffer.Release();
    hashesBuffer = null;
  }

  protected override void UpdateVisualization (NativeArray<float3x4> positions, int resolution, JobHandle handle) {
    new HashJob {
      positions = positions,
      hashes = hashes,
      hash = SmallXXHash.Seed(seed),
      domainTRS = domain.Matrix,
    }.ScheduleParallel(hashes.Length, resolution, handle).Complete();
  
    hashesBuffer.SetData(hashes.Reinterpret<uint>(4 * 4));
  }
}
