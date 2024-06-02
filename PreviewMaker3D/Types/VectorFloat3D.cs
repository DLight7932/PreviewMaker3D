using System;
using System.Collections.Generic;
using System.Text;

namespace PreviewMaker3D
{
    public struct VectorFloat3D
    {
        public float x;
        public float y;
        public float z;

        public VectorFloat3D(float x_, float y_, float z_)
        {
            x = x_;
            y = y_;
            z = z_;
        }

        public static VectorFloat3D Interpolate(VectorFloat3D v1, VectorFloat3D v2, int unit, int aspect) =>
            new VectorFloat3D(
                v1.x + (v2.x - v1.x) * aspect / unit,
                v1.y + (v2.y - v1.y) * aspect / unit,
                v1.z + (v2.z - v1.z) * aspect / unit);

        public static VectorFloat3D operator %(VectorFloat3D v1, VectorFloat3D v2) =>
            new VectorFloat3D(
            (MathF.Cos(v2.z) * MathF.Cos(v2.y) + MathF.Sin(v2.z) * MathF.Sin(v2.x) * MathF.Sin(v2.y)) * v1.x + (-MathF.Sin(v2.z) * MathF.Cos(v2.y) + MathF.Cos(v2.z) * MathF.Sin(v2.x) * MathF.Sin(v2.y)) * v1.y + MathF.Cos(v2.x) * MathF.Sin(v2.y) * v1.z,
            MathF.Sin(v2.z) * MathF.Cos(v2.x) * v1.x + MathF.Cos(v2.z) * MathF.Cos(v2.x) * v1.y - MathF.Sin(v2.x) * v1.z,
            (-MathF.Cos(v2.z) * MathF.Sin(v2.y) + MathF.Sin(v2.z) * MathF.Sin(v2.x) * MathF.Cos(v2.y)) * v1.x + (MathF.Sin(v2.z) * MathF.Sin(v2.y) + MathF.Cos(v2.z) * MathF.Sin(v2.x) * MathF.Cos(v2.y)) * v1.y + MathF.Cos(v2.x) * MathF.Cos(v2.y) * v1.z
                );

        public static VectorFloat3D operator -(VectorFloat3D v) =>
            new VectorFloat3D(-v.x, -v.y, -v.z);
        public static VectorFloat3D operator +(VectorFloat3D v1, VectorFloat3D v2) =>
            new VectorFloat3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        public static VectorFloat3D operator -(VectorFloat3D v1, VectorFloat3D v2) =>
            new VectorFloat3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        public static VectorFloat3D operator *(VectorFloat3D v1, VectorFloat3D v2) =>
            new VectorFloat3D(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        public static VectorFloat3D operator /(VectorFloat3D v1, VectorFloat3D v2) =>
            new VectorFloat3D(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        public static VectorFloat3D operator /(VectorFloat3D v, int i) =>
            new VectorFloat3D(v.x / i, v.y / i, v.z / i);
        public static VectorFloat3D operator *(VectorFloat3D v, int i) =>
            new VectorFloat3D(v.x * i, v.y * i, v.z * i);
        public static VectorFloat3D operator /(VectorFloat3D v, float f) =>
            new VectorFloat3D(v.x / f, v.y / f, v.z / f);
        public static VectorFloat3D operator *(VectorFloat3D v, float f) =>
            new VectorFloat3D(v.x * f, v.y * f, v.z * f);

        public override string ToString() => $"[{x},{y},{z}]";

        public bool TryParse(string s)
        {
            VectorFloat3D result;
            char[] charsToTrim = { '[', ']' };
            string[] S = s.Trim(charsToTrim).Split(',');
            if (!float.TryParse(S[0], out result.x) || 
                !float.TryParse(S[1], out result.y) || 
                !float.TryParse(S[2], out result.z)) return false;
            this = result;
            return true;
        }

        public static VectorFloat3D Parse(string s)
        {
            char[] charsToTrim = { '[', ']' };
            string[] S = s.Trim(charsToTrim).Split(',');
            return new VectorFloat3D(float.Parse(S[0]), float.Parse(S[1]), float.Parse(S[2]));
        }
    }
}
