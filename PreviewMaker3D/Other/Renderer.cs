using System;
using System.Collections.Generic;
using System.Text;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System.Drawing;
using System.Windows.Forms;

namespace PreviewMaker3D
{
    public abstract class Renderer
    {
        public string name;

        public Renderer(string name_)
        {
            name = name_;
        }

        public abstract void Render(OpenGL gl);
    }

    public class PolyRenderer : Renderer
    {
        public ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("Points");
        public Property<Pixel32bppRGBA> Color;
        public Property<bool> Visible;
        public ListOf<Poly> Polygones;

        public PolyRenderer(string name_) : base(name_) { }

        public override void Render(OpenGL gl)
        {
            if (!Visible.Value || Points == null || Polygones == null) return;

            gl.Begin(OpenGL.GL_TRIANGLES);
            for (int i = 0; i < Polygones.Count; i++)
            {
                VectorFloat3D point1 = Points[Polygones[i].Value.p1].Value;
                VectorFloat3D point2 = Points[Polygones[i].Value.p2].Value;
                VectorFloat3D point3 = Points[Polygones[i].Value.p3].Value;
                float brightnessFactor = MainForm.CalculateParallelism(point1, point2, point3);
                Pixel32bppRGBA color = Color.Value;

                gl.Color(color.FloatR * brightnessFactor, color.FloatG * brightnessFactor, color.FloatB * brightnessFactor);
                gl.Vertex(point1.x, point1.y, point1.z);
                gl.Vertex(point2.x, point2.y, point2.z);
                gl.Vertex(point3.x, point3.y, point3.z);
            }
            gl.End();
        }
    }

    public class LineRenderer : Renderer
    {
        public ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("Points");
        public Property<Pixel32bppRGBA> Color;
        public Property<bool> Visible;
        public ListOf<Line> Lines;

        public LineRenderer(string name_) : base(name_) { }

        public override void Render(OpenGL gl)
        {
            if (!Visible.Value || Points == null || Lines == null) return;

            gl.PolygonOffset(1.0f, 1.0f);
            gl.LineWidth(2);
            Pixel32bppRGBA color = Color.Value;
            gl.Color(color.FloatR, color.FloatG, color.FloatB);
            gl.Begin(OpenGL.GL_LINES);
            for (int i = 0; i < Lines.Count; i++)
            {
                VectorFloat3D point1 = Points[Lines[i].Value.p1].Value;
                VectorFloat3D point2 = Points[Lines[i].Value.p2].Value;
                gl.Vertex(point1.x, point1.y, point1.z);
                gl.Vertex(point2.x, point2.y, point2.z);
            }
            gl.End();
        }
    }

    public class PointRenderer : Renderer
    {
        public ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("Points");
        public Property<Pixel32bppRGBA> Color;
        public Property<bool> Visible;

        public PointRenderer(string name_) : base(name_) { }

        public override void Render(OpenGL gl)
        {
            if (!Visible.Value || Points == null) return;

            gl.PointSize(3);
            Pixel32bppRGBA color = Color.Value;
            gl.Color(color.FloatR, color.FloatG, color.FloatB);
            gl.Begin(OpenGL.GL_POINTS);
            for (int i = 0; i < Points.Count; i++)
            {
                VectorFloat3D point = Points[i].Value;
                gl.Vertex(point.x, point.y, point.z);
            }
            gl.End();
        }
    }

    public class Point1Renderer : Renderer
    {
        public Property<VectorFloat3D> Point;
        public Property<Pixel32bppRGBA> Color;
        public Property<int> Size;
        public Property<bool> Visible;

        public Point1Renderer(string name_) : base(name_) { }

        public override void Render(OpenGL gl)
        {
            if (!Visible.Value) return;

            gl.PointSize(Size.Value);
            Pixel32bppRGBA color = Color.Value;
            gl.Color(color.FloatR, color.FloatG, color.FloatB);
            gl.Begin(OpenGL.GL_POINTS);
            VectorFloat3D point = Point.Value;
            gl.Vertex(point.x, point.y, point.z);
            gl.End();
        }
    }
}
