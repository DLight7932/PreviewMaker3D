using System;
using System.Collections.Generic;
using System.Text;

namespace PreviewMaker3D
{
    public struct VectorFloat2D
    {
        public float x;
        public float y;

        public VectorFloat2D(float x_, float y_)
        {
            x = x_;
            y = y_;
        }

        public static VectorFloat2D operator -(VectorFloat2D v)
        {
            return new VectorFloat2D(-v.x, -v.y);
        }
        public static VectorFloat2D operator +(VectorFloat2D v1, VectorFloat2D v2)
        {
            return new VectorFloat2D(v1.x + v2.x, v1.y + v2.y);
        }
        public static VectorFloat2D operator -(VectorFloat2D v1, VectorFloat2D v2)
        {
            return new VectorFloat2D(v1.x - v2.x, v1.y - v2.y);
        }
        public static VectorFloat2D operator *(VectorFloat2D v1, VectorFloat2D v2)
        {
            return new VectorFloat2D(v1.x * v2.x, v1.y * v2.y);
        }
        public static VectorFloat2D operator /(VectorFloat2D v1, VectorFloat2D v2)
        {
            return new VectorFloat2D(v1.x / v2.x, v1.y / v2.y);
        }
        public static VectorFloat2D operator /(VectorFloat2D v, int i)
        {
            return new VectorFloat2D(v.x / i, v.y / i);
        }
        public static VectorFloat2D operator *(VectorFloat2D v, int i)
        {
            return new VectorFloat2D(v.x * i, v.y * i);
        }

        public override string ToString()
        {
            return $"[{x} {y}]";
        }

        public bool TryParse(string s)
        {
            VectorFloat2D result;

            int i = 1;

            string X = "";
            for (; i < s.Length && s[i] != ' '; i++)
                X += s[i];
            if (!float.TryParse(X, out result.x))
                return false;

            string Y = "";
            i++;
            for (; i < s.Length && s[i] != ']'; i++)
                Y += s[i];
            if (!float.TryParse(Y, out result.y))
                return false;

            this = result;
            return true;
        }

        public static VectorFloat2D Parse(string s)
        {
            VectorFloat2D result;

            int i = 1;

            string X = "";
            for (; i < s.Length && s[i] != ' '; i++)
                X += s[i];
            if (!float.TryParse(X, out result.x))
                throw new Exception();

            string Y = "";
            i++;
            for (; i < s.Length && s[i] != ']'; i++)
                Y += s[i];
            if (!float.TryParse(Y, out result.y))
                throw new Exception();

            return result;
        }
    }
}
