using Accord;
using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewMaker3D
{
    public abstract class Controler
    {
        public abstract void Render(OpenGL gl, int mouseX, int mouseY);
        public abstract bool IsMouseOver(OpenGL gl, int mouseX, int mouseY);

        public static void ProjectToScreen(OpenGL gl, double objX, double objY, double objZ, out int screenX, out int screenY)
        {
            int[] viewport = new int[4];
            double[] modelview = new double[16];
            double[] projection = new double[16];
            double[] winX = new double[1];
            double[] winY = new double[1];
            double[] winZ = new double[1];

            gl.GetInteger(OpenGL.GL_VIEWPORT, viewport);
            gl.GetDouble(OpenGL.GL_MODELVIEW_MATRIX, modelview);
            gl.GetDouble(OpenGL.GL_PROJECTION_MATRIX, projection);

            gl.Project(objX, objY, objZ, modelview, projection, viewport, winX, winY, winZ);

            screenX = (int)winX[0];
            screenY = (int)winY[0];
        }

        public static bool IsMouseOverLine(OpenGL gl, VectorFloat3D start, VectorFloat3D end, int mouseX, int mouseY, int threshold = 5)
        {
            ProjectToScreen(gl, start.x, start.y, start.z, out int startX, out int startY);
            ProjectToScreen(gl, end.x, end.y, end.z, out int endX, out int endY);

            int dx = endX - startX;
            int dy = endY - startY;

            if (dx == 0 && dy == 0)
                return Math.Abs(mouseX - startX) <= threshold && Math.Abs(mouseY - startY) <= threshold;

            float t = ((mouseX - startX) * dx + (mouseY - startY) * dy) / (float)(dx * dx + dy * dy);

            if (t < 0 || t > 1)
                return false;

            int projX = (int)(startX + t * dx);
            int projY = (int)(startY + t * dy);

            double distanceToLine = Math.Sqrt(Math.Pow(mouseX - projX, 2) + Math.Pow(mouseY - projY, 2));

            return distanceToLine <= threshold;
        }

        public static bool IsMouseOverPoint(OpenGL gl, VectorFloat3D point, int mouseX, int mouseY, int threshold = 5)
        {
            ProjectToScreen(gl, point.x, point.y, point.z, out int screenX, out int screenY);
            return Math.Abs(screenX - mouseX) <= threshold && Math.Abs(screenY - mouseY) <= threshold;
        }
    }

    public class ControlerPosition : Controler
    {
        public Property<VectorFloat3D> Position;
        public Property<Quaternion> Rotation;

        public override void Render(OpenGL gl, int mouseX, int mouseY)
        {
            if (IsMouseOver(gl, mouseX, mouseY)) return;

            VectorFloat3D position = Position.Value;
            Quaternion rotation = Rotation.Value;

            if (MainForm.global)
            {
                gl.LineWidth(4f);
                gl.Begin(OpenGL.GL_LINES);
                gl.Color(1f, 0f, 0f);
                gl.Vertex(position.x, position.y, position.z);
                gl.Vertex(position.x + 3f, position.y, position.z);
                gl.Color(0f, 1f, 0f);
                gl.Vertex(position.x, position.y, position.z);
                gl.Vertex(position.x, position.y + 3f, position.z);
                gl.Color(0f, 0f, 1f);
                gl.Vertex(position.x, position.y, position.z);
                gl.Vertex(position.x, position.y, position.z + 3f);
                gl.End();
            }
            else
            {
                VectorFloat3D vertex;

                gl.LineWidth(4f);
                gl.Begin(OpenGL.GL_LINES);
                gl.Color(1f, 0f, 0f);
                gl.Vertex(position.x, position.y, position.z);
                vertex = position + rotation.RotateVector(new VectorFloat3D(3f, 0f, 0f));
                gl.Vertex(vertex.x, vertex.y, vertex.z);
                gl.Color(0f, 1f, 0f);
                gl.Vertex(position.x, position.y, position.z);
                vertex = position + rotation.RotateVector(new VectorFloat3D(0f, 3f, 0f));
                gl.Vertex(vertex.x, vertex.y, vertex.z);
                gl.Color(0f, 0f, 1f);
                gl.Vertex(position.x, position.y, position.z);
                vertex = position + rotation.RotateVector(new VectorFloat3D(0f, 0f, 3f));
                gl.Vertex(vertex.x, vertex.y, vertex.z);
                gl.End();
            }

            gl.PointSize(16f);
            gl.Begin(OpenGL.GL_POINTS);
            gl.Color(0f, 0f, 0f);
            gl.Vertex(position.x, position.y, position.z);
            gl.End();
        }

        public override bool IsMouseOver(OpenGL gl, int mouseX, int mouseY)
        {
            VectorFloat3D position = Position.Value;
            Quaternion rotation = Rotation.Value;

            if (MainForm.global)
            {
                if (IsMouseOverLine(gl, position, position + new VectorFloat3D(3f, 0f, 0f), mouseX, mouseY) ||
                    IsMouseOverLine(gl, position, position + new VectorFloat3D(0f, 3f, 0f), mouseX, mouseY) ||
                    IsMouseOverLine(gl, position, position + new VectorFloat3D(0f, 0f, 3f), mouseX, mouseY))
                    return true;
            }
            else
            {
                if (IsMouseOverLine(gl, position, position + rotation.RotateVector(new VectorFloat3D(3f, 0f, 0f)), mouseX, mouseY) ||
                    IsMouseOverLine(gl, position, position + rotation.RotateVector(new VectorFloat3D(0f, 3f, 0f)), mouseX, mouseY) ||
                    IsMouseOverLine(gl, position, position + rotation.RotateVector(new VectorFloat3D(0f, 0f, 3f)), mouseX, mouseY))
                    return true;
            }

            return false;
        }
    }

    public class ControlerRotation : Controler
    {
        public Property<VectorFloat3D> Position;
        public Property<Quaternion> Rotation;

        public override void Render(OpenGL gl, int mouseX, int mouseY)
        {
            if (IsMouseOver(gl, mouseX, mouseY)) return;

            VectorFloat3D position = Position.Value;
            Quaternion rotation = Rotation.Value;

            int detalization = 32;
            float step = MathF.PI / detalization * 2;
            float radius = 2f;
            float angle;
            VectorFloat3D Vertex;

            gl.LineWidth(4f);

            if (MainForm.global)
            {
                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Color(0, 0, 1f);
                angle = 0f;
                for (int i = 0; i < detalization; i++, angle += step)
                {
                    Vertex = position + new VectorFloat3D(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f);
                    gl.Vertex(Vertex.x, Vertex.y, Vertex.z);
                }
                gl.End();

                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Color(0, 1f, 0);
                angle = 0f;
                for (int i = 0; i < detalization; i++, angle += step)
                {
                    Vertex = position + new VectorFloat3D(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle));
                    gl.Vertex(Vertex.x, Vertex.y, Vertex.z);
                }
                gl.End();

                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Color(1f, 0, 0);
                angle = 0f;
                for (int i = 0; i < detalization; i++, angle += step)
                {
                    Vertex = position + new VectorFloat3D(0f, radius * MathF.Cos(angle), radius * MathF.Sin(angle));
                    gl.Vertex(Vertex.x, Vertex.y, Vertex.z);
                }
                gl.End();
            }
            else
            {
                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Color(0, 0, 1f);
                angle = 0f;
                for (int i = 0; i < detalization; i++, angle += step)
                {
                    Vertex = position + rotation.RotateVector(new VectorFloat3D(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f));
                    gl.Vertex(Vertex.x, Vertex.y, Vertex.z);
                }
                gl.End();

                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Color(0, 1f, 0);
                angle = 0f;
                for (int i = 0; i < detalization; i++, angle += step)
                {
                    Vertex = position + rotation.RotateVector(new VectorFloat3D(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
                    gl.Vertex(Vertex.x, Vertex.y, Vertex.z);
                }
                gl.End();

                gl.Begin(OpenGL.GL_LINE_LOOP);
                gl.Color(1f, 0, 0);
                angle = 0f;
                for (int i = 0; i < detalization; i++, angle += step)
                {
                    Vertex = position + rotation.RotateVector(new VectorFloat3D(0f, radius * MathF.Cos(angle), radius * MathF.Sin(angle)));
                    gl.Vertex(Vertex.x, Vertex.y, Vertex.z);
                }
                gl.End();
            }
        }

        public override bool IsMouseOver(OpenGL gl, int mouseX, int mouseY)
        {
            VectorFloat3D position = Position.Value;
            Quaternion rotation = Rotation.Value;
            float radius = 2f;
            int detalization = 32;
            float step = MathF.PI / detalization * 2;
            float angle = 0f;
            VectorFloat3D start, end;

            if (MainForm.global)
            {
                start = position + new VectorFloat3D(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f);
                for (int i = 1; i <= detalization; i++)
                {
                    angle += step;
                    end = position + new VectorFloat3D(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f);
                    if (IsMouseOverLine(gl, start, end, mouseX, mouseY))
                    {
                        return true;
                    }
                    start = end;
                }
                angle = 0f;
                start = position + new VectorFloat3D(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle));
                for (int i = 1; i <= detalization; i++)
                {
                    angle += step;
                    end = position + new VectorFloat3D(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle));
                    if (IsMouseOverLine(gl, start, end, mouseX, mouseY))
                    {
                        return true;
                    }
                    start = end;
                }
                angle = 0f;
                start = position + new VectorFloat3D(0f, radius * MathF.Cos(angle), radius * MathF.Sin(angle));
                for (int i = 1; i <= detalization; i++)
                {
                    angle += step;
                    end = position + new VectorFloat3D(0f, radius * MathF.Cos(angle), radius * MathF.Sin(angle));
                    if (IsMouseOverLine(gl, start, end, mouseX, mouseY))
                    {
                        return true;
                    }
                    start = end;
                }
            }
            else
            {
                start = position + rotation.RotateVector(new VectorFloat3D(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f));
                for (int i = 1; i <= detalization; i++)
                {
                    angle += step;
                    end = position + rotation.RotateVector(new VectorFloat3D(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f));
                    if (IsMouseOverLine(gl, start, end, mouseX, mouseY))
                    {
                        return true;
                    }
                    start = end;
                }
                angle = 0f;
                start = position + rotation.RotateVector(new VectorFloat3D(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
                for (int i = 1; i <= detalization; i++)
                {
                    angle += step;
                    end = position + rotation.RotateVector(new VectorFloat3D(radius * MathF.Cos(angle), 0f, radius * MathF.Sin(angle)));
                    if (IsMouseOverLine(gl, start, end, mouseX, mouseY))
                    {
                        return true;
                    }
                    start = end;
                }
                angle = 0f;
                start = position + rotation.RotateVector(new VectorFloat3D(0f, radius * MathF.Cos(angle), radius * MathF.Sin(angle)));
                for (int i = 1; i <= detalization; i++)
                {
                    angle += step;
                    end = position + rotation.RotateVector(new VectorFloat3D(0f, radius * MathF.Cos(angle), radius * MathF.Sin(angle)));
                    if (IsMouseOverLine(gl, start, end, mouseX, mouseY))
                    {
                        return true;
                    }
                    start = end;
                }
            }

            return false;
        }
    }

    public class ControlerScale : Controler
    {
        public Property<VectorFloat3D> Position;
        public Property<Quaternion> Rotation;
        public Property<VectorFloat3D> LocalScale;
        public Property<VectorFloat3D> GlobalScale;

        public override void Render(OpenGL gl, int mouseX, int mouseY)
        {
            if (IsMouseOver(gl, mouseX, mouseY)) return;

            VectorFloat3D position = Position.Value;
            Quaternion rotation = Rotation.Value;

            if (MainForm.global)
            {
                gl.PointSize(8f);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Color(1f, 0f, 0f);
                gl.Vertex(position.x + 3f, position.y, position.z);
                gl.Color(0f, 1f, 0f);
                gl.Vertex(position.x, position.y + 3f, position.z);
                gl.Color(0f, 0f, 1f);
                gl.Vertex(position.x, position.y, position.z + 3f);
                gl.End();
            }
            else
            {
                VectorFloat3D vertex;

                gl.PointSize(8f);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Color(1f, 0f, 0f);
                vertex = position + rotation.RotateVector(new VectorFloat3D(3f, 0f, 0f));
                gl.Vertex(vertex.x, vertex.y, vertex.z);
                gl.Color(0f, 1f, 0f);
                vertex = position + rotation.RotateVector(new VectorFloat3D(0f, 3f, 0f));
                gl.Vertex(vertex.x, vertex.y, vertex.z);
                gl.Color(0f, 0f, 1f);
                vertex = position + rotation.RotateVector(new VectorFloat3D(0f, 0f, 3f));
                gl.Vertex(vertex.x, vertex.y, vertex.z);
                gl.End();
            }
        }

        public override bool IsMouseOver(OpenGL gl, int mouseX, int mouseY)
        {
            VectorFloat3D position = Position.Value;
            Quaternion rotation = Rotation.Value;

            if (MainForm.global)
            {
                if (IsMouseOverPoint(gl, new VectorFloat3D(position.x + 3f, position.y, position.z), mouseX, mouseY) ||
                    IsMouseOverPoint(gl, new VectorFloat3D(position.x, position.y + 3f, position.z), mouseX, mouseY) ||
                    IsMouseOverPoint(gl, new VectorFloat3D(position.x, position.y, position.z + 3f), mouseX, mouseY))
                    return true;
            }
            else
            {
                if (IsMouseOverPoint(gl, position + rotation.RotateVector(new VectorFloat3D(3f, 0f, 0f)), mouseX, mouseY) ||
                    IsMouseOverPoint(gl, position + rotation.RotateVector(new VectorFloat3D(0f, 3f, 0f)), mouseX, mouseY) ||
                    IsMouseOverPoint(gl, position + rotation.RotateVector(new VectorFloat3D(0f, 0f, 3f)), mouseX, mouseY))
                    return true;
            }

            return false;
        }
    }
}
