using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewMaker3D
{
    public abstract class Renderer
    {
        public abstract void Render(OpenGL gl);

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

        public bool IsMouseOverPoint(OpenGL gl, VectorFloat3D point, int mouseX, int mouseY, int threshold = 5)
        {
            ProjectToScreen(gl, point.x, point.y, point.z, out int screenX, out int screenY);
            return Math.Abs(screenX - mouseX) <= threshold && Math.Abs(screenY - mouseY) <= threshold;
        }

        public bool IsMouseOverLine(OpenGL gl, VectorFloat3D start, VectorFloat3D end, int mouseX, int mouseY, int threshold = 5)
        {
            ProjectToScreen(gl, start.x, start.y, start.z, out int startX, out int startY);
            ProjectToScreen(gl, end.x, end.y, end.z, out int endX, out int endY);

            double lineLength = Math.Sqrt(Math.Pow(endX - startX, 2) + Math.Pow(endY - startY, 2));
            double distanceToLine = Math.Abs((endX - startX) * (startY - mouseY) - (startX - mouseX) * (endY - startY)) / lineLength;

            return distanceToLine <= threshold;
        }
    }

    public class CubeRenderer : Renderer
    {
        public Property<VectorFloat3D> Position;
        public Property<Quaternion> Rotation;
        public Property<VectorFloat3D> LocalScale;
        public Property<VectorFloat3D> GlobalScale;

        public bool IsMouseOver(OpenGL gl, int mouseX, int mouseY)
        {
            VectorFloat3D position = Position.Value;

            return

                (IsMouseOverPoint(gl, new VectorFloat3D(position.x + 3f, position.y, position.z), mouseX, mouseY) ||
                IsMouseOverPoint(gl, new VectorFloat3D(position.x, position.y + 3f, position.z), mouseX, mouseY) ||
                IsMouseOverPoint(gl, new VectorFloat3D(position.x, position.y, position.z + 3f), mouseX, mouseY) ||
                IsMouseOverPoint(gl, position, mouseX, mouseY, threshold: 8)) ||

                (IsMouseOverLine(gl, position, new VectorFloat3D(position.x + 3f, position.y, position.z), mouseX, mouseY) ||
                IsMouseOverLine(gl, position, new VectorFloat3D(position.x, position.y + 3f, position.z), mouseX, mouseY) ||
                IsMouseOverLine(gl, position, new VectorFloat3D(position.x, position.y, position.z + 3f), mouseX, mouseY));
        }

        public CubeRenderer(Animated cube)
        {
            Position = cube.GetProperty("Position") as Property<VectorFloat3D>;
            Rotation = cube.GetProperty("Rotation") as Property<Quaternion>;
            LocalScale = cube.GetProperty("LocalScale") as Property<VectorFloat3D>;
            GlobalScale = cube.GetProperty("GlobalScale") as Property<VectorFloat3D>;
        }

        public override void Render(OpenGL gl)
        {
            var vertices = new float[,]
            {
            { -1f, -1f, -1f }, { 1f, -1f, -1f }, { 1f, 1f, -1f }, { -1f, 1f, -1f },
            { -1f, -1f, 1f }, { 1f, -1f, 1f }, { 1f, 1f, 1f }, { -1f, 1f, 1f }
            };

            var faces = new int[,]
            {
            { 0, 1, 2, 3 }, { 1, 5, 6, 2 }, { 5, 4, 7, 6 },
            { 4, 0, 3, 7 }, { 3, 2, 6, 7 }, { 4, 5, 1, 0 }
            };

            var edges = new int[,]
            {
            { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 },
            { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 },
            { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
            };

            var position = Position.Value;
            var rotation = Rotation.Value;
            var localScale = LocalScale.Value;
            var globalScale = GlobalScale.Value;

            const float offset = 1.001f;

            gl.Color(1f, 1f, 1f);
            gl.Begin(OpenGL.GL_QUADS);
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var vertex = new VectorFloat3D(
                        vertices[faces[i, j], 0],
                        vertices[faces[i, j], 1],
                        vertices[faces[i, j], 2]);

                    vertex *= localScale;
                    vertex = rotation.RotateVector(vertex);
                    vertex *= globalScale;
                    vertex += position;

                    gl.Vertex(vertex.x, vertex.y, vertex.z);
                }
            }
            gl.End();

            gl.Color(0f, 0f, 0f);
            gl.LineWidth(2.0f);
            gl.Begin(OpenGL.GL_LINES);
            for (int i = 0; i < edges.GetLength(0); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var vertex = new VectorFloat3D(
                        vertices[edges[i, j], 0],
                        vertices[edges[i, j], 1],
                        vertices[edges[i, j], 2]);

                    vertex *= localScale;
                    vertex = rotation.RotateVector(vertex);
                    vertex *= globalScale;
                    vertex += position;

                    vertex.x *= offset;
                    vertex.y *= offset;
                    vertex.z *= offset;

                    gl.Vertex(vertex.x, vertex.y, vertex.z);
                }
            }
            gl.End();

            gl.Color(1f, 0f, 0f);
            gl.PointSize(8.0f);
            gl.Begin(OpenGL.GL_POINTS);
            for (int i = 0; i < vertices.GetLength(0); i++)
            {
                var vertex = new VectorFloat3D(
                    vertices[i, 0],
                    vertices[i, 1],
                    vertices[i, 2]);

                vertex *= localScale;
                vertex = rotation.RotateVector(vertex);
                vertex *= globalScale;
                vertex += position;

                vertex.x *= offset;
                vertex.y *= offset;
                vertex.z *= offset;

                gl.Vertex(vertex.x, vertex.y, vertex.z);
            }
            gl.End();
        }
    }
}
