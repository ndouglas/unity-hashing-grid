using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public abstract class Visualization : MonoBehaviour {

  static int 
    positionsId = Shader.PropertyToID("_Positions"),
    normalsId = Shader.PropertyToID("_Normals"),
    configId = Shader.PropertyToID("_Config");

  NativeArray<float3x4> positions, normals;

  [SerializeField]
  Mesh instanceMesh;

  [SerializeField]
  Material material;

  [SerializeField, Range(1, 512)]
  int resolution = 16;

  ComputeBuffer positionsBuffer, normalsBuffer;

  MaterialPropertyBlock propertyBlock;

  [SerializeField, Range(-0.5f, 0.5f)]
  float displacement = 0.1f;

  bool isDirty;

  Bounds bounds;

  [SerializeField, Range(0.1f, 10f)]
  float instanceScale = 2f;

  public enum Shape {
    Plane,
    Sphere,
    Torus,
  }

  static Shapes.ScheduleDelegate[] shapeJobs = {
    Shapes.Job<Shapes.Plane>.ScheduleParallel,
    Shapes.Job<Shapes.Sphere>.ScheduleParallel,
    Shapes.Job<Shapes.Torus>.ScheduleParallel,
  };

  [SerializeField]
  Shape shape;

  void OnEnable() {
    isDirty = true;
    int length = resolution * resolution;
    length = length / 4 + (length & 1);
    positions = new NativeArray<float3x4>(length, Allocator.Persistent);
    normals = new NativeArray<float3x4>(length, Allocator.Persistent);
    positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
    normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

    propertyBlock ??= new MaterialPropertyBlock();
    EnableVisualization(length, propertyBlock);
    propertyBlock.SetBuffer(positionsId, positionsBuffer);
    propertyBlock.SetBuffer(normalsId, normalsBuffer);
    propertyBlock.SetVector(configId, new Vector4(
      resolution, 
      instanceScale / resolution, 
      displacement
    ));

  }

  void OnDisable() {
    positions.Dispose();
    normals.Dispose();
    positionsBuffer.Release();
    normalsBuffer.Release();
    positionsBuffer = null;
    normalsBuffer = null;
    DisableVisualization();
  }

  void OnValidate() {
    if (positionsBuffer != null && enabled) {
      OnDisable();
      OnEnable();
    }
  }

  void Update() {
    if (isDirty || transform.hasChanged) {
      isDirty = false;
      transform.hasChanged = false;

      UpdateVisualization(
        positions, 
        resolution,
        shapeJobs[(int)shape](
          positions,
          normals,
          resolution,
          transform.localToWorldMatrix,
          default
        )
      );
      positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
      normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * 4));
      bounds = new Bounds(
        transform.position, 
        float3(2f * cmax(abs(transform.lossyScale)) + displacement)
      );
    }
    Graphics.DrawMeshInstancedProcedural(
      instanceMesh, 
      0, 
      material, 
      bounds,
      resolution * resolution, 
      propertyBlock
    );
  }

  protected abstract void EnableVisualization (int dataLength, MaterialPropertyBlock propertyBlock);

  protected abstract void DisableVisualization ();

  protected abstract void UpdateVisualization (NativeArray<float3x4> positions, int resolution, JobHandle handle);

}
