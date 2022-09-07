using Unity.Mathematics;

[System.Serializable]
public struct SpaceTRS {

    public float3 translation, rotation, scale;

    public float3x4 Matrix {
      get {
        float4x4 matrix = float4x4.TRS(translation, quaternion.EulerZXY(math.radians(rotation)), scale);
        return math.float3x4(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz, matrix.c3.xyz);
      }
    }

}
