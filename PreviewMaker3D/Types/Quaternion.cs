using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewMaker3D
{
    public struct Quaternion
    {
        public float w;
        public float x;
        public float y;
        public float z;

        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Quaternion Conjugate()
        {
            return new Quaternion(w, -x, -y, -z);
        }

        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(
                q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z,
                q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y,
                q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x,
                q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w
            );
        }

        public static Quaternion operator *(Quaternion q, VectorFloat3D v)
        {
            return new Quaternion(
                -q.x * v.x - q.y * v.y - q.z * v.z,
                q.w * v.x + q.y * v.z - q.z * v.y,
                q.w * v.y - q.x * v.z + q.z * v.x,
                q.w * v.z + q.x * v.y - q.y * v.x
            );
        }

        public VectorFloat3D RotateVector(VectorFloat3D vector)
        {
            Quaternion result = this * vector * Conjugate();
            return new VectorFloat3D(result.x, result.y, result.z);
        }

        public static Quaternion FromAxisAngle(float angleDegrees, float x, float y, float z)
        {
            float angleRadians = angleDegrees * MathF.PI / 180f;
            float halfAngle = angleRadians / 2f;
            float s = MathF.Sin(halfAngle);
            return new Quaternion(MathF.Cos(halfAngle), x * s, y * s, z * s);
        }

        public override string ToString()
        {
            return w.ToString("F2") + " " +
                x.ToString("F2") + " " +
                y.ToString("F2") + " " +
                z.ToString("F2");
        }
    }
}
