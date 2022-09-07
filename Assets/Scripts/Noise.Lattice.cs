using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise {

  struct LatticeSpan4 {
    public int4 p0, p1;
    public float4 t;
  }

  static LatticeSpan4 GetLatticeSpan4 (float4 coordinates) {
    float4 points = floor(coordinates);
    LatticeSpan4 span;
    span.p0 = (int4) points;
    span.p1 = span.p0 + 1;
    span.t = coordinates - points;
    span.t = span.t * span.t * span.t * (span.t * (span.t * 6f - 15f) + 10f);
    return span;
  }

  public struct Lattice1D : INoise {

    public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash) {
      LatticeSpan4 x = GetLatticeSpan4(positions.c0);
      return lerp(hash.Eat(x.p0).Floats01A, hash.Eat(x.p1).Floats01A, x.t) * 2f - 1f;
    }

  }

  public struct Lattice2D : INoise {

    public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash) {
      LatticeSpan4 
        x = GetLatticeSpan4(positions.c0),
        z = GetLatticeSpan4(positions.c2);
      SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1);
      return lerp(
        lerp(h0.Eat(z.p0).Floats01A, h0.Eat(z.p1).Floats01A, z.t),
        lerp(h1.Eat(z.p0).Floats01A, h1.Eat(z.p1).Floats01A, z.t),
        x.t
      ) * 2f - 1f;
    }

  }

  public struct Lattice3D : INoise {

    public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash) {
      LatticeSpan4 
        x = GetLatticeSpan4(positions.c0),
        y = GetLatticeSpan4(positions.c1),
        z = GetLatticeSpan4(positions.c2);
      SmallXXHash4 
        h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1),
        h00 = h0.Eat(y.p0), h01 = h0.Eat(y.p1),
        h10 = h1.Eat(y.p0), h11 = h1.Eat(y.p1);
      return lerp(
        lerp(
          lerp(h00.Eat(z.p0).Floats01A, h00.Eat(z.p1).Floats01A, z.t),
          lerp(h01.Eat(z.p0).Floats01A, h01.Eat(z.p1).Floats01A, z.t),
          y.t
        ),
        lerp(
          lerp(h10.Eat(z.p0).Floats01A, h10.Eat(z.p1).Floats01A, z.t),
          lerp(h11.Eat(z.p0).Floats01A, h11.Eat(z.p1).Floats01A, z.t),
          y.t
        ),
        x.t
      ) * 2f - 1f;
    }

  }

}