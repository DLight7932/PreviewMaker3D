using SharpGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewMaker3D
{
    internal class Path
    {
        public List<VectorFloat3D> Points = new List<VectorFloat3D>();

        public void Render(OpenGL gl)
        {
            // Ініціалізація малювання ліній
            gl.Begin(OpenGL.GL_LINE_STRIP);
            gl.Color(0f, 0f, 0f);
            gl.LineWidth(2f);

            int n = Points.Count;
            if (n < 2) return;

            // Масиви для координат
            float[] xs = new float[n], ys = new float[n], zs = new float[n];
            for (int i = 0; i < n; i++)
            {
                xs[i] = Points[i].x;
                ys[i] = Points[i].y;
                zs[i] = Points[i].z;
            }

            // Інтерполяція
            List<VectorFloat3D> interpolatedPoints = new List<VectorFloat3D>();
            for (int i = 0; i < n - 1; i++)
            {
                for (float t = 0; t <= 1; t += 0.1f)
                {
                    float x = CubicInterpolate(xs, i, t, n);
                    float y = CubicInterpolate(ys, i, t, n);
                    float z = CubicInterpolate(zs, i, t, n);
                    interpolatedPoints.Add(new VectorFloat3D(x, y, z));
                }
            }
            interpolatedPoints.Add(Points[n - 1]);

            // Малювання інтерпольованих точок
            foreach (var point in interpolatedPoints)
            {
                gl.Vertex(point.x, point.y, point.z);
            }

            gl.End();

            gl.PointSize(8f);
            gl.Begin(OpenGL.GL_POINTS);
            gl.Color(1f, 0f, 0f);

            foreach (VectorFloat3D vector in Points)
                gl.Vertex(vector.x, vector.y, vector.z);

            gl.End();
        }

        private float CubicInterpolate(float[] p, int i, float t, int n)
        {
            float p0 = p[ClampIndex(i - 1, n)];
            float p1 = p[ClampIndex(i, n)];
            float p2 = p[ClampIndex(i + 1, n)];
            float p3 = p[ClampIndex(i + 2, n)];

            float a0 = p3 - p2 - p0 + p1;
            float a1 = p0 - p1 - a0;
            float a2 = p2 - p0;
            float a3 = p1;

            return a0 * t * t * t + a1 * t * t + a2 * t + a3;
        }

        private int ClampIndex(int index, int length)
        {
            if (index < 0) return 0;
            if (index >= length) return length - 1;
            return index;
        }
    }
}
