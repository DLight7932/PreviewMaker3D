using System;
using System.Collections.Generic;
using System.Text;

namespace PreviewMaker3D
{
    public struct VectorInt2D
    {
        public int x;
        public int y;

        public VectorInt2D(int x_, int y_)
        {
            x = x_;
            y = y_;
        }

        public static VectorInt2D operator -(VectorInt2D v)
        {
            return new VectorInt2D(-v.x, -v.y);
        }
        public static VectorInt2D operator +(VectorInt2D v1, VectorInt2D v2)
        {
            return new VectorInt2D(v1.x + v2.x, v1.y + v2.y);
        }
        public static VectorInt2D operator -(VectorInt2D v1, VectorInt2D v2)
        {
            return new VectorInt2D(v1.x - v2.x, v1.y - v2.y);
        }
        public static VectorInt2D operator *(VectorInt2D v1, VectorInt2D v2)
        {
            return new VectorInt2D(v1.x * v2.x, v1.y * v2.y);
        }
        public static VectorInt2D operator /(VectorInt2D v1, VectorInt2D v2)
        {
            return new VectorInt2D(v1.x / v2.x, v1.y / v2.y);
        }
        public static VectorInt2D operator /(VectorInt2D v, int i)
        {
            return new VectorInt2D(v.x / i, v.y / i);
        }
        public static VectorInt2D operator *(VectorInt2D v, int i)
        {
            return new VectorInt2D(v.x * i, v.y * i);
        }

        public override string ToString()
        {
            return $"[{x} {y}]";
        }

        public bool TryParse(string s)
        {
            VectorInt2D result;

            int i = 1;

            string X = "";
            for (; i < s.Length && s[i] != ' '; i++)
                X += s[i];
            if (!int.TryParse(X, out result.x))
                return false;

            string Y = "";
            i++;
            for (; i < s.Length && s[i] != ']'; i++)
                Y += s[i];
            if (!int.TryParse(Y, out result.y))
                return false;

            this = result;
            return true;
        }

        public static VectorInt2D Parse(string s)
        {
            VectorInt2D result;

            int i = 1;

            string X = "";
            for (; i < s.Length && s[i] != ' '; i++)
                X += s[i];
            if (!int.TryParse(X, out result.x))
                throw new Exception();

            string Y = "";
            i++;
            for (; i < s.Length && s[i] != ']'; i++)
                Y += s[i];
            if (!int.TryParse(Y, out result.y))
                throw new Exception();

            return result;
        }
    }
}
