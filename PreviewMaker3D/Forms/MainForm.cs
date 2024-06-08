using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SharpGL;
using System.Globalization;
using System.Diagnostics;
using Accord.Video.FFMPEG;
using System.IO;
using Newtonsoft.Json;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using SharpGL.SceneGraph.Primitives;

namespace PreviewMaker3D
{
    public partial class MainForm : Form
    {
        static VectorFloat3D Normal(VectorFloat3D vertex1, VectorFloat3D vertex2, VectorFloat3D vertex3)
        {
            VectorFloat3D side1 = new VectorFloat3D(
                vertex2.x - vertex1.x,
                vertex2.y - vertex1.y,
                vertex2.z - vertex1.z
            );

            VectorFloat3D side2 = new VectorFloat3D(
                vertex3.x - vertex1.x,
                vertex3.y - vertex1.y,
                vertex3.z - vertex1.z
            );

            VectorFloat3D normal = new VectorFloat3D(
                side1.y * side2.z - side1.z * side2.y,
                side1.z * side2.x - side1.x * side2.z,
                side1.x * side2.y - side1.y * side2.x
            );

            float length = (float)Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
            normal.x /= length;
            normal.y /= length;
            normal.z /= length;

            return normal;
        }
        static VectorFloat3D CameraNormal => mainForm.project.FreeCam.Normal;
        public static float CalculateParallelism(VectorFloat3D vertex1, VectorFloat3D vertex2, VectorFloat3D vertex3)
        {
            VectorFloat3D N1 = Normal(vertex1, vertex2, vertex3);
            VectorFloat3D N2 = CameraNormal;

            float dotProduct = N1.x * N2.x + N1.y * N2.y + N1.z * N2.z;

            float lengthN1 = (float)Math.Sqrt(N1.x * N1.x + N1.y * N1.y + N1.z * N1.z);
            float lengthN2 = (float)Math.Sqrt(N2.x * N2.x + N2.y * N2.y + N2.z * N2.z);

            float angle = (float)Math.Acos(dotProduct / (lengthN1 * lengthN2));

            float parallelism = 1.0f - angle / (float)Math.PI;

            return 1f - parallelism;
        }

        public static MainForm mainForm;
        public static void UnFocus()
        {
            mainForm.openglControl.Focus();
        }

        public MainForm()
        {
            mainForm = this;
            new MyConsole(this);
            InitializeComponent();
            GL = openglControl.OpenGL;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            openglControl.MouseWheel += (object sender, MouseEventArgs e) =>
            {
                if (e.Delta > 0)
                    project.FreeCam.MoveForward(5f);
                else
                    project.FreeCam.MoveBack(5f);
            };

            Random random = new Random();

            for (int i = 0; i < 10; i++)
                path.Points.Add(new VectorFloat3D(random.Next(-10, 10), random.Next(-10, 10), random.Next(-10, 10)));

            AddCube();
        }

        public void AddCube()
        {
            //Animated Cube = new Animated();

            Cube.AddProperty(new Property<VectorFloat3D>("Position", new VectorFloat3D(0, 0, 0)));
            Cube.AddProperty(new Property<Quaternion>("Rotation", new Quaternion(1f, 0f, 0f, 0f)));
            Cube.AddProperty(new Property<VectorFloat3D>("LocalScale", new VectorFloat3D(1f, 1f, 1f)));
            Cube.AddProperty(new Property<VectorFloat3D>("GlobalScale", new VectorFloat3D(1f, 1f, 1f)));

            Cube.Renderers.Add(new CubeRenderer(Cube));

            Cube.Controlers.Add(new ControlerPosition()
            {
                Position = Cube.GetProperty("Position") as Property<VectorFloat3D>,
                Rotation = Cube.GetProperty("Rotation") as Property<Quaternion>
            });
            Cube.Controlers.Add(new ControlerRotation()
            {
                Position = Cube.GetProperty("Position") as Property<VectorFloat3D>,
                Rotation = Cube.GetProperty("Rotation") as Property<Quaternion>
            });
            Cube.Controlers.Add(new ControlerScale()
            {
                Position = Cube.GetProperty("Position") as Property<VectorFloat3D>,
                Rotation = Cube.GetProperty("Rotation") as Property<Quaternion>
            });

            project.scene.Add(Cube);
        }

        OpenGL GL;
        Project project = new Project()
        {
            FreeCam = new Camera()
            {
                Position = new VectorFloat3D(4f, 4f, 4f),
                Rotation = new Quaternion(0.37f, 0.11f, -0.88f, 0.27f)
            }
        };
        int fps = 0;
        int currentFPS = 0;
        Stopwatch SW = Stopwatch.StartNew();
        int second = 0;
        Path path = new Path();
        Animated Cube = new Animated();
        string mod = "Position";

        private void openglControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            GL.ClearColor(0.7f, 1.0f, 0.7f, 0.0f);
            GL.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            GL.LoadIdentity();
            GL.Enable(OpenGL.GL_POLYGON_OFFSET_FILL);
            GL.Enable(OpenGL.GL_TEXTURE_2D);
            GL.Enable(OpenGL.GL_NORMALIZE);
            GL.Enable(OpenGL.GL_DEPTH_TEST);

            project.FreeCam.LookAt(GL);

            GL.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
            //foreach (Animated obj in project.scene)
            //    obj.Render(GL);

            Cube.Render(GL);

            RenderGround();

            GL.Disable(OpenGL.GL_DEPTH_TEST);

            Point mousePos = openglControl.PointToClient(Cursor.Position);
            Cube.RenderControlers(GL, mousePos.X, openglControl.Height - mousePos.Y);

            GL.Flush();

            fpsCount();
        }

        void RenderGround()
        {
            GL.LineWidth(2.0f);
            GL.Begin(OpenGL.GL_LINES);

            GL.Color(0.25f, 0.25f, 0.25f, 1.0f);
            for (float i = -10.0f; i <= 10.0f; i++)
            {
                GL.Vertex(i, -0.001f, 10.0f);
                GL.Vertex(i, -0.001f, -10.0f);
                GL.Vertex(-10.0f, -0.001f, i);
                GL.Vertex(10.0f, -0.001f, i);
            }

            GL.Color(1.0f, 0.0f, 0.0f, 1.0f);
            GL.Vertex(0.0f, 0.0f, 0.0f);
            GL.Vertex(15.0f, 0.0f, 0.0f);

            GL.Color(0.0f, 1.0f, 0.0f, 1.0f);
            GL.Vertex(0.0f, 0.0f, 0.0f);
            GL.Vertex(0.0f, 15.0f, 0.0f);

            GL.Color(0.0f, 0.0f, 1.0f, 1.0f);
            GL.Vertex(0.0f, 0.0f, 0.0f);
            GL.Vertex(0.0f, 0.0f, 15.0f);

            GL.End();

            GL.Color(0.0f, 0.0f, 0.0f, 1.0f);
            GL.PointSize(3.0f);
            GL.Begin(OpenGL.GL_POINTS);
            GL.Vertex(15.0f, 0.0f, 0.0f);
            GL.Vertex(0.0f, 15.0f, 0.0f);
            GL.Vertex(0.0f, 0.0f, 15.0f);

            GL.End();
        }

        public void fpsCount()
        {
            Text = $"Tick: {Convert.ToString(second)} FPS: {Convert.ToString(currentFPS)} | {project.FreeCam.Rotation} | Mod: {mod} | Global: {global}";
            fps++;
            if (SW.ElapsedMilliseconds > 1000)
            {
                second++;
                currentFPS = fps;
                fps = 0;
                SW.Restart();
            }
        }

        bool pressed = false;
        bool pressedLeft = false;
        bool pressedRight = false;
        Point MPosition;
        Point StartMPosition;

        private void openglControl1_MouseDown(object sender, MouseEventArgs e)
        {
            pressed = true;
            if (e.Button == MouseButtons.Left) pressedLeft = true;
            if (e.Button == MouseButtons.Right) pressedRight = true;
            MPosition = MousePosition;
            StartMPosition = MousePosition;
            Cursor.Hide();
        }

        private void openglControl1_MouseUp(object sender, MouseEventArgs e)
        {
            pressed = false;
            pressedLeft = false;
            pressedRight = false;
            Cursor.Position = StartMPosition;
            Cursor.Show();
        }

        private void openglControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!pressed) return;

            if (pressedRight)
            {
                project.FreeCam.Rotation = Quaternion.FromAxisAngle((MPosition.X - MousePosition.X) / 20.0f, 0, 1, 0) * project.FreeCam.Rotation;
                project.FreeCam.Rotation *= Quaternion.FromAxisAngle((MousePosition.Y - MPosition.Y) / 20.0f, 1, 0, 0);
            }
            else if (pressedLeft)
            {
                project.FreeCam.Position = Quaternion.FromAxisAngle((MPosition.X - MousePosition.X) / 20.0f, 0, 1, 0).RotateVector(project.FreeCam.Position);
                project.FreeCam.Position = Quaternion.FromAxisAngle((MPosition.Y - MousePosition.Y) / 20.0f, 1, 0, 0).RotateVector(project.FreeCam.Position);
                project.FreeCam.Rotation = Quaternion.FromAxisAngle((MPosition.X - MousePosition.X) / 20.0f, 0, 1, 0) * project.FreeCam.Rotation;
                project.FreeCam.Rotation = project.FreeCam.Rotation * Quaternion.FromAxisAngle((MousePosition.Y - MPosition.Y) / 20.0f, 1, 0, 0);
            }
            Cursor.Position = openglControl.PointToScreen(new Point(openglControl.Width / 2, openglControl.Height / 2));
            MPosition = MousePosition;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pressed)
            {
                if (movingRight) project.FreeCam.MoveRight();
                if (movingLeft) project.FreeCam.MoveLeft();
                if (movingUp) project.FreeCam.MoveUp();
                if (movingDown) project.FreeCam.MoveDown();
                if (movingBack) project.FreeCam.MoveBack();
                if (movingForward) project.FreeCam.MoveForward();
                if (rotatingLeft) project.FreeCam.Rotation *= Quaternion.FromAxisAngle(1f, 0, 0, 1);
                if (rotatingRight) project.FreeCam.Rotation *= Quaternion.FromAxisAngle(-1f, 0, 0, 1);
                if (rotatingCenter)
                {
                    var q = project.FreeCam.Rotation;

                    float ysqr = q.y * q.y;

                    float t0 = +2f * (q.w * q.y + q.z * q.x);
                    float t1 = +1f - 2f * (q.x * q.x + ysqr);
                    float yaw = MathF.Atan2(t0, t1);

                    float t2 = 2f * (q.w * q.x - q.z * q.y);
                    t2 = t2 > 1f ? 1f : t2;
                    t2 = t2 < -1f ? -1f : t2;
                    float pitch = MathF.Asin(t2);

                    Quaternion yawQuat = Quaternion.FromAxisAngle(yaw * 180f / MathF.PI, 0, 1, 0);
                    Quaternion pitchQuat = Quaternion.FromAxisAngle(pitch * 180f / MathF.PI, 1, 0, 0);

                    project.FreeCam.Rotation = yawQuat * pitchQuat;
                }
                return;
            }

            Property<VectorFloat3D> position = Cube.GetProperty("Position") as Property<VectorFloat3D>;
            Property<Quaternion> rotation = Cube.GetProperty("Rotation") as Property<Quaternion>;
            Property<VectorFloat3D> globalScale = Cube.GetProperty("GlobalScale") as Property<VectorFloat3D>;
            Property<VectorFloat3D> localScale = Cube.GetProperty("LocalScale") as Property<VectorFloat3D>;
            switch (mod)
            {
                case "Position":
                    if (movingRight)
                        if (global)
                            position.Value += new VectorFloat3D(0.1f, 0f, 0f);
                        else
                            position.Value += rotation.Value.RotateVector(new VectorFloat3D(0.1f, 0f, 0f));
                    if (movingLeft)
                        if (global)
                            position.Value += new VectorFloat3D(-0.1f, 0f, 0f);
                        else
                            position.Value += rotation.Value.RotateVector(new VectorFloat3D(-0.1f, 0f, 0f));
                    if (movingUp)
                        if (global)
                            position.Value += new VectorFloat3D(0f, 0.1f, 0f);
                        else
                            position.Value += rotation.Value.RotateVector(new VectorFloat3D(0f, 0.1f, 0f));
                    if (movingDown)
                        if (global)
                            position.Value += new VectorFloat3D(0f, -0.1f, 0f);
                        else
                            position.Value += rotation.Value.RotateVector(new VectorFloat3D(0f, -0.1f, 0f));
                    if (movingBack)
                        if (global)
                            position.Value += new VectorFloat3D(0f, 0f, 0.1f);
                        else
                            position.Value += rotation.Value.RotateVector(new VectorFloat3D(0f, 0f, 0.1f));
                    if (movingForward)
                        if (global)
                            position.Value += new VectorFloat3D(0f, 0f, -0.1f);
                        else
                            position.Value += rotation.Value.RotateVector(new VectorFloat3D(0f, 0f, -0.1f));
                    break;
                case "Rotation":
                    if (movingRight)
                        if (global)
                            rotation.Value = Quaternion.FromAxisAngle(1, 1, 0, 0) * rotation.Value;
                        else
                            rotation.Value = rotation.Value * Quaternion.FromAxisAngle(1, 1, 0, 0);
                    if (movingLeft)
                        if (global)
                            rotation.Value = Quaternion.FromAxisAngle(-1, 1, 0, 0) * rotation.Value;
                        else
                            rotation.Value = rotation.Value * Quaternion.FromAxisAngle(-1, 1, 0, 0);
                    if (movingUp)
                        if (global)
                            rotation.Value = Quaternion.FromAxisAngle(1, 0, 1, 0) * rotation.Value;
                        else
                            rotation.Value = rotation.Value * Quaternion.FromAxisAngle(1, 0, 1, 0);
                    if (movingDown)
                        if (global)
                            rotation.Value = Quaternion.FromAxisAngle(-1, 0, 1, 0) * rotation.Value;
                        else
                            rotation.Value = rotation.Value * Quaternion.FromAxisAngle(-1, 0, 1, 0);
                    if (movingBack)
                        if (global)
                            rotation.Value = Quaternion.FromAxisAngle(1, 0, 0, 1) * rotation.Value;
                        else
                            rotation.Value = rotation.Value * Quaternion.FromAxisAngle(1, 0, 0, 1);
                    if (movingForward)
                        if (global)
                            rotation.Value = Quaternion.FromAxisAngle(-1, 0, 0, 1) * rotation.Value;
                        else
                            rotation.Value = rotation.Value * Quaternion.FromAxisAngle(-1, 0, 0, 1);
                    break;
                case "Scale":
                    if (movingRight)
                        if (global)
                            globalScale.Value += new VectorFloat3D(0.1f, 0f, 0f);
                        else
                            localScale.Value += new VectorFloat3D(0.1f, 0f, 0f);
                    if (movingLeft)
                        if (global)
                            globalScale.Value += new VectorFloat3D(-0.1f, 0f, 0f);
                        else
                            localScale.Value += new VectorFloat3D(-0.1f, 0f, 0f);
                    if (movingUp)
                        if (global)
                            globalScale.Value += new VectorFloat3D(0f, 0.1f, 0f);
                        else
                            localScale.Value += new VectorFloat3D(0f, 0.1f, 0f);
                    if (movingDown)
                        if (global)
                            globalScale.Value += new VectorFloat3D(0f, -0.1f, 0f);
                        else
                            localScale.Value += new VectorFloat3D(0f, -0.1f, 0f);
                    if (movingBack)
                        if (global)
                            globalScale.Value += new VectorFloat3D(0f, 0f, 0.1f);
                        else
                            localScale.Value += new VectorFloat3D(0f, 0f, 0.1f);
                    if (movingForward)
                        if (global)
                            globalScale.Value += new VectorFloat3D(0f, 0f, -0.1f);
                        else
                            localScale.Value += new VectorFloat3D(0f, 0f, -0.1f);
                    break;
            }
        }

        bool movingRight;
        bool movingLeft;
        bool movingUp;
        bool movingDown;
        bool movingBack;
        bool movingForward;
        bool rotatingRight;
        bool rotatingLeft;
        bool rotatingCenter;

        public static bool global = false;

        private void openglControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    movingForward = true;
                    break;
                case Keys.S:
                    movingBack = true;
                    break;
                case Keys.A:
                    movingLeft = true;
                    break;
                case Keys.D:
                    movingRight = true;
                    break;
                case Keys.E:
                    movingUp = true;
                    break;
                case Keys.Q:
                    movingDown = true;
                    break;

                case Keys.Z:
                    rotatingLeft = true;
                    break;
                case Keys.C:
                    rotatingRight = true;
                    break;
                case Keys.X:
                    rotatingCenter = true;
                    break;

                case Keys.G:
                    global = !global;
                    break;
                case Keys.R:
                    switch (mod)
                    {
                        case "Position":
                            mod = "Rotation";
                            break;
                        case "Rotation":
                            mod = "Scale";
                            break;
                        case "Scale":
                            mod = "Position";
                            break;
                    }
                    break;
            }
        }

        private void openglControl1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    movingForward = false;
                    break;
                case Keys.S:
                    movingBack = false;
                    break;
                case Keys.A:
                    movingLeft = false;
                    break;
                case Keys.D:
                    movingRight = false;
                    break;
                case Keys.E:
                    movingUp = false;
                    break;
                case Keys.Q:
                    movingDown = false;
                    break;

                case Keys.Z:
                    rotatingLeft = false;
                    break;
                case Keys.C:
                    rotatingRight = false;
                    break;
                case Keys.X:
                    rotatingCenter = false;
                    break;
            }
        }
    }
}
