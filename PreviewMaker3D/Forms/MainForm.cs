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
    public enum PropertyTypes
    {
        Var_VecF3,
        Var_VecF2,
        Var_VecI2,
        Var_Bool,
        Var_Int,
        Var_Pix32RGBA,
        Var_Float,
        Var_Texture,
        Anim_VecF3,
        Anim_VecF2,
        Anim_VecI2,
        Anim_Bool,
        Anim_Int,
        Anim_Pix32RGBA,
        Anim_Float,
        Anim_Texture,
        GlobalPosition,
        VecF3_Plus_VecF3,
        VecF3_Div_Int
    }

    public enum RendererTypes
    {
        Cube,
        Cone,
        Cilinder,
        Sphere,
        Triangle,
        Point,
        Line,
        Poly
    }

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
        static VectorFloat3D CameraNormal => mainForm.FreeCam.Normal;
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
        public List<Animated> Scene => scene;
        public TreeView treeView => timeLine.TreeView;
        public static void UnFocus()
        {
            mainForm.openglControl.Focus();
        }

        public static void DeleteProperty(PropertyBase Property)
        {
            ((Animated)mainForm.timeLine.selectedObject.Tag).Properties.Remove(Property);
            RefreshModifiers();
            mainForm.timeLine.SelectObject(mainForm.timeLine.selectedObject);
        }

        public static void DeleteRenderer(Renderer renderer)
        {
            (mainForm.timeLine.selectedObject.Tag as Animated).Renderers.Remove(renderer);
            RefreshModifiers();
            mainForm.timeLine.SelectObject(mainForm.timeLine.selectedObject);
        }

        public static void RefreshModifiers()
        {
            foreach (PropertyModifier p in mainForm.Modifiers)
                p.RefreshModifier();
        }

        public static void ClearModifiers()
        {
            mainForm.splitContainer1.Panel2.Controls.Clear();
        }

        public List<PropertyModifier> Modifiers = new List<PropertyModifier>();
        public List<Button> Renderers = new List<Button>();

        public MainForm()
        {
            mainForm = this;
            new MyConsole(this);
            InitializeComponent();
            GL = openglControl.OpenGL;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            treeView.AfterSelect += TreeView_AfterSelect;

            openglControl.MouseWheel += (object sender, MouseEventArgs e) =>
            {
                if (e.Delta > 0)
                    FreeCam.MoveForward(5f);
                else
                    FreeCam.MoveBack(5f);
            };

            tabPage1.MouseDoubleClick += (object sender, MouseEventArgs e) =>
            {
                if (timeLine.selectedObject != null)
                {
                    PropertyCreator variableCreator = new PropertyCreator(true);
                    variableCreator.ShowDialog();
                    if (variableCreator.DialogResult == DialogResult.OK)
                    {
                        (timeLine.selectedObject.Tag as Animated).AddProperty(PropertyCreator.result as PropertyBase);
                        TreeView_AfterSelect(null, null);
                        timeLine.SelectObject(timeLine.selectedObject);
                    }
                }
            };

            tabPage2.MouseDoubleClick += (object sender, MouseEventArgs e) =>
            {
                if (timeLine.selectedObject != null)
                {
                    PropertyCreator variableCreator = new PropertyCreator(false);
                    variableCreator.ShowDialog();
                    if (variableCreator.DialogResult == DialogResult.OK)
                    {
                        (timeLine.selectedObject.Tag as Animated).AddProperty(PropertyCreator.result as PropertyBase);
                        TreeView_AfterSelect(null, null);
                        timeLine.SelectObject(timeLine.selectedObject);
                    }
                }
            };

            tabPage3.MouseDoubleClick += (object sender, MouseEventArgs e) =>
            {
                if (timeLine.selectedObject != null)
                {
                    RendererCreator rendererCreator = new RendererCreator();
                    rendererCreator.ShowDialog();
                    if (rendererCreator.DialogResult == DialogResult.OK)
                    {
                        (timeLine.selectedObject.Tag as Animated).Renderers.Add(RendererCreator.result);
                        TreeView_AfterSelect(null, null);
                        timeLine.SelectObject(timeLine.selectedObject);
                    }
                }
            };
        }

        OpenGL GL;
        public List<Animated> scene = new List<Animated>();
        Camera FreeCam = new Camera()
        {
            Position = new VectorFloat3D(4.0f, 4.0f, 4.0f),
            Rotation = new VectorFloat3D(MathF.PI / 6, -MathF.PI * 3 / 4, 0.0f)
        };
        int fps = 0;
        int currentFPS = 0;
        Stopwatch SW = Stopwatch.StartNew();
        int second = 0;

        private void openglControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            GL.ClearColor(0.7f, 1.0f, 0.7f, 0.0f);
            GL.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            GL.LoadIdentity();
            GL.Enable(OpenGL.GL_POLYGON_OFFSET_FILL);
            GL.Enable(OpenGL.GL_TEXTURE_2D);
            GL.Enable(OpenGL.GL_NORMALIZE);
            GL.Enable(OpenGL.GL_CULL_FACE);

            FreeCam.LookAt(GL);

            GL.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
            foreach (Animated obj in scene)
                obj.Render(GL);

            RenderGround();

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
            Text = $"Tick: {Convert.ToString(second)} FPS: {Convert.ToString(currentFPS)} Time: {PropertyBase.Time}";
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
        Point MPosition;
        Point StartMPosition;

        private void openglControl1_MouseDown(object sender, MouseEventArgs e)
        {
            pressed = true;
            MPosition = MousePosition;
            StartMPosition = MousePosition;
            Cursor.Hide();
        }

        private void openglControl1_MouseUp(object sender, MouseEventArgs e)
        {
            pressed = false;
            Cursor.Position = StartMPosition;
            Cursor.Show();
        }

        private void openglControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!pressed) return;
            FreeCam.Rotation.y -= (MousePosition.X - MPosition.X) / 200.0f;
            FreeCam.Rotation.x += (MousePosition.Y - MPosition.Y) / 200.0f;
            Cursor.Position = openglControl.PointToScreen(new Point(openglControl.Width / 2, openglControl.Height / 2));
            MPosition = MousePosition;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (movingRight) FreeCam.MoveRight();
            if (movingLeft) FreeCam.MoveLeft();
            if (movingUp) FreeCam.MoveUp();
            if (movingDown) FreeCam.MoveDown();
            if (movingBack) FreeCam.MoveBack();
            if (movingForward) FreeCam.MoveForward();
            if (rotatingLeft) FreeCam.Rotation.z += 0.01f;
            if (rotatingRight) FreeCam.Rotation.z -= 0.01f;
            if (rotatingCenter) FreeCam.Rotation.z = 0;
            if (resetView)
            {
                FreeCam.Position = new VectorFloat3D(4.0f, 4.0f, 4.0f);
                FreeCam.Rotation = new VectorFloat3D(MathF.PI / 6, -MathF.PI * 3 / 4, 0.0f);
            }
            timeLine.PictureBox.Invalidate();
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
        bool resetView;

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

                case Keys.R:
                    resetView = true;
                    break;

                case Keys.D1:
                    AddTexture();
                    break;
                case Keys.D2:
                    RenderImage();
                    break;
                case Keys.D3:
                    RenderVideo();
                    break;
                case Keys.D4:
                    SaveProject();
                    break;
                case Keys.D5:
                    LoadProject();
                    break;

                case Keys.OemMinus:
                    if (PropertyBase.Time > 0)
                        PropertyBase.Time--;
                    break;
                case Keys.Oemplus:
                    PropertyBase.Time++;
                    break;
            }
        }

        OpenFileDialog openFileDialog = new OpenFileDialog();
        void AddTexture()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                Textures.AddTexture(GL, openFileDialog.FileName);
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        void RenderImage()
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            int frameWidth = openglControl.Width - openglControl.Width % 2;
            int frameHeight = openglControl.Height - openglControl.Height % 2;
            Bitmap frame = new Bitmap(frameWidth, frameHeight);
            openglControl.DrawToBitmap(frame, new Rectangle(0, 0, frameWidth, frameHeight));
            frame.Save($"{saveFileDialog.FileName}.png");
        }

        void RenderVideo()
        {
            VideoRenderForm videoRenderForm = new VideoRenderForm();
            int fps = 24;
            int left = 0;
            int right = 24;
            videoRenderForm.IntModifier1.Text = "6";
            videoRenderForm.IntModifier1.Text = "0";
            videoRenderForm.IntModifier1.Text = "24";
            videoRenderForm.IntModifier1.GetValue = () => { return fps; };
            videoRenderForm.IntModifier2.GetValue = () => { return left; };
            videoRenderForm.IntModifier3.GetValue = () => { return right; };
            videoRenderForm.IntModifier1.SetValue = (int value) => { if (value > 0 && value <= 60) fps = value; };
            videoRenderForm.IntModifier2.SetValue = (int value) => { if (value < right && value >= 0) left = value; };
            videoRenderForm.IntModifier3.SetValue = (int value) => { if (value > left) right = value; };
            videoRenderForm.Button.Click += (object sender, EventArgs e) =>
            {
                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                int frameWidth = openglControl.Width - openglControl.Width % 2;
                int frameHeight = openglControl.Height - openglControl.Height % 2;

                VideoFileWriter writer = new VideoFileWriter();
                writer.Open($"{saveFileDialog.FileName}.avi", frameWidth, frameHeight, fps, VideoCodec.Default);

                Bitmap frame = new Bitmap(frameWidth, frameHeight);
                for (PropertyBase.Time = left; PropertyBase.Time <= right; PropertyBase.Time++)
                {
                    openglControl.DrawToBitmap(frame, new Rectangle(0, 0, frameWidth, frameHeight));
                    writer.WriteVideoFrame(frame);
                }

                writer.Close();
                videoRenderForm.Close();
            };

            videoRenderForm.ShowDialog();
        }

        void SaveProject()
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            int lastIndex = saveFileDialog.FileName.LastIndexOf('.');

            string filePath = lastIndex >= 0 ? saveFileDialog.FileName.Substring(0, lastIndex) : saveFileDialog.FileName;

            using (StreamWriter writer = File.CreateText($"{filePath}.txt"))
            {
                writer.WriteLine(CustomSerializer.Serialize(new Project() { scene = scene }));
            }
        }

        void LoadProject()
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            using (StreamReader reader = new StreamReader(openFileDialog.FileName))
            {
                string text = reader.ReadToEnd();
                Project project = CustomSerializer.Deserialize(text);
                foreach (Animated animated in project.scene)
                {
                    scene.Add(animated);
                    treeView.Nodes.Add(new TreeNode($"{animated.name} [{animated.id}]") { Tag = animated });
                }
            }
        }

        public static class CustomSerializer
        {
            public static string Serialize(Project objects)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects, // Додамо типи в серіалізований JSON
                    Formatting = Formatting.Indented
                };
                return JsonConvert.SerializeObject(objects, settings);
            }

            public static Project Deserialize(string json)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects // Відновимо типи з JSON
                };
                return JsonConvert.DeserializeObject<Project>(json, settings);
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

                case Keys.R:
                    resetView = false;
                    break;
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Tag is Animated animated)
                animated.DisplayProperties(tabControl1);
        }

        public void CreateCube()
        {
            Animated Cube = Generate();

            Animated Generate()
            {
                Animated Object = new Animated();
                Object.name = "Cube";
                Object.AddProperty(new AnimatedPropertyVectorFloat3D("Position", default));
                Object.AddProperty(new AnimatedPropertyVectorFloat3D("Rotation", default));
                Object.AddProperty(new AnimatedPropertyVectorFloat3D("Scale", new VectorFloat3D(1.0f, 1.0f, 1.0f)));
                Object.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(0, 0, 0)));
                Object.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color2", new Pixel32bppRGBA(255, 0, 0)));
                Object.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color3", new Pixel32bppRGBA(255, 255, 255)));
                Object.AddProperty(new AnimatedProperty<bool>("Visible", true));

                ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("Points");
                Property<VectorFloat3D> CubePosition = Object.GetProperty("Position") as Property<VectorFloat3D>;
                Property<VectorFloat3D> CubeRotation = Object.GetProperty("Rotation") as Property<VectorFloat3D>;
                Property<VectorFloat3D> CubeScale = Object.GetProperty("Scale") as Property<VectorFloat3D>;
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(1, 1, 1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(1, 1, -1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(1, -1, -1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(1, -1, 1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(-1, 1, 1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(-1, 1, -1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(-1, -1, -1))));
                Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                    new Variable<VectorFloat3D>(new VectorFloat3D(-1, -1, 1))));
                Object.AddProperty(Points);
                PointRenderer pointRenderer = new PointRenderer("CubePoints");
                pointRenderer.Points = Object.GetProperty("Points") as ListOf<VectorFloat3D>;
                pointRenderer.Color = Object.GetProperty("Color1") as Property<Pixel32bppRGBA>;
                pointRenderer.Visible = Object.GetProperty("Visible") as Property<bool>;
                Object.Renderers.Add(pointRenderer);

                ListOf<Line> Lines = new ListOf<Line>("Lines");
                Lines.Add(new Line(0, 1));
                Lines.Add(new Line(1, 2));
                Lines.Add(new Line(2, 3));
                Lines.Add(new Line(3, 0));
                Lines.Add(new Line(4, 5));
                Lines.Add(new Line(5, 6));
                Lines.Add(new Line(6, 7));
                Lines.Add(new Line(7, 4));
                Lines.Add(new Line(0, 4));
                Lines.Add(new Line(1, 5));
                Lines.Add(new Line(2, 6));
                Lines.Add(new Line(3, 7));
                Object.AddProperty(Lines);
                LineRenderer lineRenderer = new LineRenderer("CubeLines");
                lineRenderer.Points = Object.GetProperty("Points") as ListOf<VectorFloat3D>;
                lineRenderer.Lines = Object.GetProperty("Lines") as ListOf<Line>;
                lineRenderer.Color = Object.GetProperty("Color2") as Property<Pixel32bppRGBA>;
                lineRenderer.Visible = Object.GetProperty("Visible") as Property<bool>;
                Object.Renderers.Add(lineRenderer);

                ListOf<Poly> Polygones = new ListOf<Poly>("Polys");
                Polygones.Add(new Poly(2, 1, 0));
                Polygones.Add(new Poly(0, 3, 2));
                Polygones.Add(new Poly(4, 5, 6));
                Polygones.Add(new Poly(6, 7, 4));
                Polygones.Add(new Poly(5, 4, 0));
                Polygones.Add(new Poly(0, 1, 5));
                Polygones.Add(new Poly(6, 5, 1));
                Polygones.Add(new Poly(1, 2, 6));
                Polygones.Add(new Poly(7, 6, 2));
                Polygones.Add(new Poly(2, 3, 7));
                Polygones.Add(new Poly(0, 7, 3));
                Polygones.Add(new Poly(0, 4, 7));
                Object.AddProperty(Polygones);
                PolyRenderer polyRenderer = new PolyRenderer("CubePolygones");
                polyRenderer.Points = Object.GetProperty("Points") as ListOf<VectorFloat3D>;
                polyRenderer.Polygones = Object.GetProperty("Polys") as ListOf<Poly>;
                polyRenderer.Color = Object.GetProperty("Color3") as Property<Pixel32bppRGBA>;
                polyRenderer.Visible = Object.GetProperty("Visible") as Property<bool>;
                Object.Renderers.Add(polyRenderer);

                return Object;
            }

            scene.Add(Cube);
            treeView.Nodes.Add(new TreeNode($"{Cube.name} [{Cube.id}]") { Tag = Cube });
            Console.WriteLine($"\tAdded: {Cube.name} [{Cube.id}]");
        }

        public void CreateCone()
        {
            Animated Cone = new Animated() { name = "Cone" };
            Cone.AddProperty(new AnimatedPropertyVectorFloat3D("Position", default));
            Cone.AddProperty(new AnimatedPropertyVectorFloat3D("Rotation", default));
            Cone.AddProperty(new AnimatedPropertyVectorFloat3D("Scale", new VectorFloat3D(1.0f, 1.0f, 1.0f)));
            Cone.AddProperty(new AnimatedProperty<float>("Radius", 1.0f));
            Cone.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(0, 0, 0)));
            Cone.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color2", new Pixel32bppRGBA(255, 0, 0)));
            Cone.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color3", new Pixel32bppRGBA(255, 255, 255)));
            Cone.AddProperty(new AnimatedProperty<bool>("Visible", true));

            List<Property<VectorFloat3D>> Points = new List<Property<VectorFloat3D>>();
            Property<VectorFloat3D> ConePosition = Cone.GetProperty("Position") as Property<VectorFloat3D>;
            Property<VectorFloat3D> ConeRotation = Cone.GetProperty("Rotation") as Property<VectorFloat3D>;
            Property<VectorFloat3D> ConeScale = Cone.GetProperty("Scale") as Property<VectorFloat3D>;
            Points.Add(new ExpressionVf3DGP(ConePosition, ConeRotation, ConeScale,
                new Variable<VectorFloat3D>(new VectorFloat3D(0, 2, 0))));
            int detalization = 8;
            float alpha = 0f;
            float alphaStep = 2f * MathF.PI / detalization;
            for (int i = 1; i <= detalization; i++, alpha += alphaStep)
                Points.Add(new ExpressionVf3DGP($"Point {i}", ConePosition, ConeRotation, ConeScale,
                    new ExpressionVf3MulFloat(
                        $"Point {i}",
                        new Variable<VectorFloat3D>($"Point {i}", new VectorFloat3D(MathF.Sin(alpha), 0f, MathF.Cos(alpha))),
                        Cone.GetProperty("Radius") as Property<float>)));
            Cone.AddProperty(new Variable<List<Property<VectorFloat3D>>>("Points", Points));
            PointRenderer pointRenderer = new PointRenderer("ConePoints");
            pointRenderer.Points = Cone.GetProperty("Points") as ListOf<VectorFloat3D>;
            pointRenderer.Color = Cone.GetProperty("Color1") as Property<Pixel32bppRGBA>;
            pointRenderer.Visible = Cone.GetProperty("Visible") as Property<bool>;
            Cone.Renderers.Add(pointRenderer);

            List<Line> Lines = new List<Line>();
            for (int i = 1; i < detalization; i++)
            {
                Lines.Add(new Line(i, i + 1));
                Lines.Add(new Line(i + 1, 0));
            }
            Lines.Add(new Line(detalization, 1));
            Lines.Add(new Line(1, 0));
            Cone.AddProperty(new Variable<List<Line>>("Lines", Lines));
            LineRenderer lineRenderer = new LineRenderer("ConeLines");
            lineRenderer.Points = Cone.GetProperty("Points") as ListOf<VectorFloat3D>;
            lineRenderer.Lines = Cone.GetProperty("Lines") as ListOf<Line>;
            lineRenderer.Color = Cone.GetProperty("Color2") as Property<Pixel32bppRGBA>;
            lineRenderer.Visible = Cone.GetProperty("Visible") as Property<bool>;
            Cone.Renderers.Add(lineRenderer);

            List<Poly> Polygones = new List<Poly>();
            for (int i = 1; i < detalization; i++)
                Polygones.Add(new Poly(i, i + 1, 0));
            Polygones.Add(new Poly(detalization, 1, 0));
            for (int i = 2; i < detalization; i++)
                Polygones.Add(new Poly(1, i + 1, i));
            Cone.AddProperty(new Variable<List<Poly>>("Polygones", Polygones));
            PolyRenderer polyRenderer = new PolyRenderer("ConePolygones");
            polyRenderer.Points = Cone.GetProperty("Points") as ListOf<VectorFloat3D>;
            polyRenderer.Polygones = Cone.GetProperty("Polygones") as ListOf<Poly>;
            polyRenderer.Color = Cone.GetProperty("Color3") as Property<Pixel32bppRGBA>;
            polyRenderer.Visible = Cone.GetProperty("Visible") as Property<bool>;
            Cone.Renderers.Add(polyRenderer);

            scene.Add(Cone);
            treeView.Nodes.Add(new TreeNode($"{Cone.name} [{Cone.id}]") { Tag = Cone });
            Console.WriteLine($"\tAdded: {Cone.name} [{Cone.id}]");
        }

        public void CreateCilinder()
        {
            Animated Cilinder = new Animated() { name = "Cilinder" };
            Cilinder.AddProperty(new AnimatedPropertyVectorFloat3D("Position", default));
            Cilinder.AddProperty(new AnimatedPropertyVectorFloat3D("Rotation", default));
            Cilinder.AddProperty(new AnimatedPropertyVectorFloat3D("Scale", new VectorFloat3D(1.0f, 1.0f, 1.0f)));
            Cilinder.AddProperty(new Variable<float>("Radius", 1.0f));
            Cilinder.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(0, 0, 0)));
            Cilinder.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color2", new Pixel32bppRGBA(255, 0, 0)));
            Cilinder.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color3", new Pixel32bppRGBA(255, 255, 255)));
            Cilinder.AddProperty(new AnimatedProperty<MyTexture>("Texture", Textures.None));
            Cilinder.AddProperty(new AnimatedProperty<bool>("Visible", true));

            List<Property<VectorFloat3D>> Points = new List<Property<VectorFloat3D>>();
            Property<VectorFloat3D> CilinderPosition = Cilinder.GetProperty("Position") as Property<VectorFloat3D>;
            Property<VectorFloat3D> CilinderRotation = Cilinder.GetProperty("Rotation") as Property<VectorFloat3D>;
            Property<VectorFloat3D> CilinderScale = Cilinder.GetProperty("Scale") as Property<VectorFloat3D>;
            int detalization = 8;
            float alpha = 0f;
            float alphaStep = 2f * MathF.PI / detalization;
            for (int i = 0; i < detalization; i++, alpha += alphaStep)
            {
                Points.Add(new ExpressionVf3DGP($"Point {i}", CilinderPosition, CilinderRotation, CilinderScale,
                    new ExpressionVf3MulFloat(
                        $"Point {i}",
                        new Variable<VectorFloat3D>($"Point {i}", new VectorFloat3D(MathF.Sin(alpha), 1.0f, MathF.Cos(alpha))),
                        Cilinder.GetProperty("Radius") as Property<float>)));
            }
            for (int i = detalization; i < detalization * 2; i++, alpha += alphaStep)
            {
                Points.Add(new ExpressionVf3DGP($"Point {i}", CilinderPosition, CilinderRotation, CilinderScale,
                    new ExpressionVf3MulFloat(
                        $"Point {i}",
                        new Variable<VectorFloat3D>($"Point {i}", new VectorFloat3D(MathF.Sin(alpha), -1.0f, MathF.Cos(alpha))),
                        Cilinder.GetProperty("Radius") as Property<float>)));
            }
            Cilinder.AddProperty(new Variable<List<Property<VectorFloat3D>>>("Points", Points));
            PointRenderer pointRenderer = new PointRenderer("CilinderPoints");
            pointRenderer.Points = Cilinder.GetProperty("Points") as ListOf<VectorFloat3D>;
            pointRenderer.Color = Cilinder.GetProperty("Color1") as Property<Pixel32bppRGBA>;
            pointRenderer.Visible = Cilinder.GetProperty("Visible") as Property<bool>;
            Cilinder.Renderers.Add(pointRenderer);

            List<Line> Lines = new List<Line>();
            for (int i = 1; i < detalization; i++)
            {
                Lines.Add(new Line(i + detalization, i + detalization - 1));
                Lines.Add(new Line(i, i - 1));
                Lines.Add(new Line(i, i + detalization));
            }
            Lines.Add(new Line(0, detalization - 1));
            Lines.Add(new Line(detalization, detalization + detalization - 1));
            Lines.Add(new Line(0, detalization));
            Cilinder.AddProperty(new Variable<List<Line>>("Lines", Lines));
            LineRenderer lineRenderer = new LineRenderer("CilinderLines");
            lineRenderer.Points = Cilinder.GetProperty("Points") as ListOf<VectorFloat3D>;
            lineRenderer.Lines = Cilinder.GetProperty("Lines") as ListOf<Line>;
            lineRenderer.Color = Cilinder.GetProperty("Color2") as Property<Pixel32bppRGBA>;
            lineRenderer.Visible = Cilinder.GetProperty("Visible") as Property<bool>;
            Cilinder.Renderers.Add(lineRenderer);

            List<Poly> Polygones = new List<Poly>();
            for (int i = 1; i < detalization; i++)
            {
                Polygones.Add(new Poly(i - 1, i + detalization, i));
                Polygones.Add(new Poly(i + detalization, i - 1, i + detalization - 1));
            }
            Polygones.Add(new Poly(0, detalization + detalization - 1, detalization));
            Polygones.Add(new Poly(0, detalization - 1, detalization + detalization - 1));
            for (int i = 2; i < detalization; i++)
                Polygones.Add(new Poly(0, i - 1, i));
            for (int i = detalization + 2; i < detalization * 2; i++)
                Polygones.Add(new Poly(detalization, i, i - 1));
            Cilinder.AddProperty(new Variable<List<Poly>>("Polygones", Polygones));
            PolyRenderer polyRenderer = new PolyRenderer("CilinderPolygones");
            polyRenderer.Points = Cilinder.GetProperty("Points") as ListOf<VectorFloat3D>;
            polyRenderer.Polygones = Cilinder.GetProperty("Polygones") as ListOf<Poly>;
            polyRenderer.Color = Cilinder.GetProperty("Color3") as Property<Pixel32bppRGBA>;
            polyRenderer.Visible = Cilinder.GetProperty("Visible") as Property<bool>;
            Cilinder.Renderers.Add(polyRenderer);

            scene.Add(Cilinder);
            treeView.Nodes.Add(new TreeNode($"{Cilinder.name} [{Cilinder.id}]") { Tag = Cilinder });
            Console.WriteLine($"\tAdded: {Cilinder.name} [{Cilinder.id}]");
        }

        public void CreateSphere()
        {
            Animated Sphere = new Animated() { name = "Sphere" };

            Sphere.AddProperty(new AnimatedPropertyVectorFloat3D("Position", default));
            Sphere.AddProperty(new AnimatedPropertyVectorFloat3D("Rotation", default));
            Sphere.AddProperty(new AnimatedPropertyVectorFloat3D("Scale", new VectorFloat3D(1.0f, 1.0f, 1.0f)));
            Sphere.AddProperty(new AnimatedProperty<float>("Radius", 1.0f));
            Sphere.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(0, 0, 0)));
            Sphere.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color2", new Pixel32bppRGBA(255, 0, 0)));
            Sphere.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color3", new Pixel32bppRGBA(255, 255, 255)));
            Sphere.AddProperty(new AnimatedProperty<bool>("Visible", true));

            List<Property<VectorFloat3D>> Points = new List<Property<VectorFloat3D>>();
            Property<VectorFloat3D> SpherePosition = Sphere.GetProperty("Position") as Property<VectorFloat3D>;
            Property<VectorFloat3D> SphereRotation = Sphere.GetProperty("Rotation") as Property<VectorFloat3D>;
            Property<VectorFloat3D> SphereScale = Sphere.GetProperty("Scale") as Property<VectorFloat3D>;

            int detalization1 = 8;
            int detalization2 = 8;

            float alphaStep = MathF.PI / detalization1;
            float betaStep = 2 * MathF.PI / detalization2;
            float alpha = alphaStep;
            float beta = 0;

            for (int i = 1; i < detalization1; i++, alpha += alphaStep, beta = 0)
                for (int j = 0; j < detalization2; j++, beta += betaStep)
                    Points.Add(new ExpressionVf3DGP($"Point {i}", SpherePosition, SphereRotation, SphereScale,
                    new ExpressionVf3MulFloat(
                        $"Point {i}",
                        new Variable<VectorFloat3D>($"Point {i}", new VectorFloat3D(
                        MathF.Sin(alpha) * MathF.Sin(beta),
                        MathF.Cos(alpha),
                        MathF.Sin(alpha) * MathF.Cos(beta))),
                        Sphere.GetProperty("Radius") as Property<float>)));
            Points.Add(new ExpressionVf3DGP($"Point", SpherePosition, SphereRotation, SphereScale,
                    new ExpressionVf3MulFloat(
                        $"Point",
                        new Variable<VectorFloat3D>($"Point", new VectorFloat3D(0, 1, 0)),
                        Sphere.GetProperty("Radius") as Property<float>)));
            Points.Add(new ExpressionVf3DGP($"Point", SpherePosition, SphereRotation, SphereScale,
                    new ExpressionVf3MulFloat(
                        $"Point",
                        new Variable<VectorFloat3D>($"Point", new VectorFloat3D(0, -1, 0)),
                        Sphere.GetProperty("Radius") as Property<float>)));
            Sphere.AddProperty(new Variable<List<Property<VectorFloat3D>>>("Points", Points));
            PointRenderer pointRenderer = new PointRenderer("ConePoints");
            pointRenderer.Points = Sphere.GetProperty("Points") as ListOf<VectorFloat3D>;
            pointRenderer.Color = Sphere.GetProperty("Color1") as Property<Pixel32bppRGBA>;
            pointRenderer.Visible = Sphere.GetProperty("Visible") as Property<bool>;
            Sphere.Renderers.Add(pointRenderer);

            List<Line> Lines = new List<Line>();
            for (int i = 0; i < detalization1 - 1; i++)
            {
                for (int j = 1; j < detalization2; j++)
                    Lines.Add(new Line(i * detalization2 + j, i * detalization2 + j - 1));
                Lines.Add(new Line(i * detalization2, i * detalization2 + detalization2 - 1));
            }
            for (int i = 0; i < detalization1 - 2; i++)
                for (int j = 0; j < detalization2; j++)
                    Lines.Add(new Line(i * detalization2 + j, (i + 1) * detalization2 + j));
            for (int j = 0; j < detalization2; j++)
            {
                Lines.Add(new Line(j, (detalization1 - 1) * detalization2));
                Lines.Add(new Line((detalization1 - 2) * detalization2 + j, (detalization1 - 1) * detalization2 + 1));
            }
            Sphere.AddProperty(new Variable<List<Line>>("Lines", Lines));
            LineRenderer lineRenderer = new LineRenderer("ConeLines");
            lineRenderer.Points = Sphere.GetProperty("Points") as ListOf<VectorFloat3D>;
            lineRenderer.Lines = Sphere.GetProperty("Lines") as ListOf<Line>;
            lineRenderer.Color = Sphere.GetProperty("Color2") as Property<Pixel32bppRGBA>;
            lineRenderer.Visible = Sphere.GetProperty("Visible") as Property<bool>;
            Sphere.Renderers.Add(lineRenderer);

            ListOf<Poly> Polygones = new ListOf<Poly>("");
            for (int i = 1; i < detalization1 - 1; i++)
            {
                for (int j = 1; j < detalization2; j++)
                {
                    Polygones.Add(new Poly(i * detalization2 + j - 1, i * detalization2 + j, (i - 1) * detalization2 + j));
                    Polygones.Add(new Poly(i * detalization2 + j - 1, (i - 1) * detalization2 + j, (i - 1) * detalization2 + j - 1));
                }
                Polygones.Add(new Poly(i * detalization2 + detalization2 - 1, i * detalization2, (i - 1) * detalization2));
                Polygones.Add(new Poly(i * detalization2 + detalization2 - 1, (i - 1) * detalization2, (i - 1) * detalization2 + detalization2 - 1));
            }
            for (int j = 1; j < detalization2; j++)
            {
                Polygones.Add(new Poly(j - 1, j, (detalization1 - 1) * detalization2));
                Polygones.Add(new Poly((detalization1 - 2) * detalization2 + j, (detalization1 - 2) * detalization2 + j - 1, (detalization1 - 1) * detalization2 + 1));
            }
            Polygones.Add(new Poly(detalization2 - 1, 0, (detalization1 - 1) * detalization2));
            Polygones.Add(new Poly((detalization1 - 2) * detalization2, (detalization1 - 2) * detalization2 + detalization2 - 1, (detalization1 - 1) * detalization2 + 1));
            Sphere.AddProperty(Polygones);
            PolyRenderer polyRenderer = new PolyRenderer("SpherePolygones");
            polyRenderer.Points = Sphere.GetProperty("Points") as ListOf<VectorFloat3D>;
            polyRenderer.Polygones = Sphere.GetProperty("Polygones") as ListOf<Poly>;
            polyRenderer.Color = Sphere.GetProperty("Color3") as Property<Pixel32bppRGBA>;
            polyRenderer.Visible = Sphere.GetProperty("Visible") as Property<bool>;
            Sphere.Renderers.Add(polyRenderer);

            scene.Add(Sphere);
            treeView.Nodes.Add(new TreeNode($"{Sphere.name} [{Sphere.id}]") { Tag = Sphere });
            Console.WriteLine($"\tAdded: {Sphere.name} [{Sphere.id}]");
        }

        public void CreateObject()
        {
            Animated Object = new Animated() { name = "Object" };
            scene.Add(Object);
            treeView.Nodes.Add(new TreeNode($"{Object.name} [{Object.id}]") { Tag = Object });
            Console.WriteLine($"\tAdded: {Object.name} [{Object.id}]");
        }

        public void CreatePolygone()
        {
            Animated Polygone = new Animated() { name = "Polygone" };
            Polygone.AddProperty(new AnimatedPropertyVectorFloat3D("Point1", new VectorFloat3D(1, 0, 0)));
            Polygone.AddProperty(new AnimatedPropertyVectorFloat3D("Point2", new VectorFloat3D(0, 1, 0)));
            Polygone.AddProperty(new AnimatedPropertyVectorFloat3D("Point3", new VectorFloat3D(0, 0, 1)));
            Polygone.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(0, 0, 0)));
            Polygone.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color2", new Pixel32bppRGBA(255, 0, 0)));
            Polygone.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color3", new Pixel32bppRGBA(255, 255, 255)));
            Polygone.AddProperty(new AnimatedProperty<bool>("Visible", true));

            ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("Points");
            Points.Add(Polygone.GetProperty("Point1") as Property<VectorFloat3D>);
            Points.Add(Polygone.GetProperty("Point2") as Property<VectorFloat3D>);
            Points.Add(Polygone.GetProperty("Point3") as Property<VectorFloat3D>);
            Polygone.AddProperty(Points);
            PointRenderer pointRenderer = new PointRenderer("CubePoints");
            pointRenderer.Points = Points;
            pointRenderer.Color = Polygone.GetProperty("Color1") as Property<Pixel32bppRGBA>;
            pointRenderer.Visible = Polygone.GetProperty("Visible") as Property<bool>;
            Polygone.Renderers.Add(pointRenderer);

            ListOf<Line> Lines = new ListOf<Line>("");
            Lines.Add(new Line(0, 1));
            Lines.Add(new Line(1, 2));
            Lines.Add(new Line(2, 0));
            Polygone.AddProperty(Lines);
            LineRenderer lineRenderer = new LineRenderer("CubeLines");
            lineRenderer.Points = Points;
            lineRenderer.Lines = Polygone.GetProperty("Lines") as ListOf<Line>;
            lineRenderer.Color = Polygone.GetProperty("Color2") as Property<Pixel32bppRGBA>;
            lineRenderer.Visible = Polygone.GetProperty("Visible") as Property<bool>;
            Polygone.Renderers.Add(lineRenderer);

            List<Poly> Polygones = new List<Poly>();
            Polygones.Add(new Poly(0, 1, 2));
            Polygone.AddProperty(new Variable<List<Poly>>("Polygones", Polygones));
            PolyRenderer polyRenderer = new PolyRenderer("CubePolygones");
            polyRenderer.Points = Points;
            polyRenderer.Polygones = Polygone.GetProperty("Polygones") as ListOf<Poly>;
            polyRenderer.Color = Polygone.GetProperty("Color3") as Property<Pixel32bppRGBA>;
            polyRenderer.Visible = Polygone.GetProperty("Visible") as Property<bool>;
            Polygone.Renderers.Add(polyRenderer);

            scene.Add(Polygone);
            treeView.Nodes.Add(new TreeNode($"{Polygone.name} [{Polygone.id}]") { Tag = Polygone });
            Console.WriteLine($"\tAdded: {Polygone.name} [{Polygone.id}]");
        }

        public void CreatePoint()
        {
            Animated Point = new Animated() { name = "Point" };
            Point.AddProperty(new AnimatedPropertyVectorFloat3D("Point", new VectorFloat3D(0, 0, 0)));
            Point.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(255, 255, 255)));
            Point.AddProperty(new AnimatedProperty<bool>("Visible", true));

            ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("");
            Points.Add(Point.GetProperty("Point") as Property<VectorFloat3D>);
            PointRenderer pointRenderer = new PointRenderer("Point");
            pointRenderer.Points = Points;
            pointRenderer.Color = Point.GetProperty("Color1") as Property<Pixel32bppRGBA>;
            pointRenderer.Visible = Point.GetProperty("Visible") as Property<bool>;
            Point.Renderers.Add(pointRenderer);

            scene.Add(Point);
            treeView.Nodes.Add(new TreeNode($"{Point.name} [{Point.id}]") { Tag = Point });
            Console.WriteLine($"\tCreated: {Point.name} [{Point.id}]");
        }

        public void CreateLine()
        {
            Animated Line = new Animated() { name = "Line" };
            Line.AddProperty(new AnimatedPropertyVectorFloat3D("Point1", new VectorFloat3D(0, 0, 0)));
            Line.AddProperty(new AnimatedPropertyVectorFloat3D("Point2", new VectorFloat3D(1, 1, 1)));
            Line.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color1", new Pixel32bppRGBA(0, 0, 0)));
            Line.AddProperty(new AnimatedPropertyPixel32bppRGBA("Color2", new Pixel32bppRGBA(255, 0, 0)));
            Line.AddProperty(new AnimatedProperty<bool>("Visible", true));

            ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>("");
            Points.Add(Line.GetProperty("Point1") as Property<VectorFloat3D>);
            Points.Add(Line.GetProperty("Point2") as Property<VectorFloat3D>);
            PointRenderer pointRenderer = new PointRenderer("Point");
            pointRenderer.Points = Points;
            pointRenderer.Color = Line.GetProperty("Color1") as Property<Pixel32bppRGBA>;
            pointRenderer.Visible = Line.GetProperty("Visible") as Property<bool>;
            Line.Renderers.Add(pointRenderer);

            ListOf<Line> Lines = new ListOf<Line>("");
            Lines.Add(new Line(0, 1));
            LineRenderer lineRenderer = new LineRenderer("CilinderLines");
            lineRenderer.Points = Points;
            lineRenderer.Lines = Lines;
            lineRenderer.Color = Line.GetProperty("Color2") as Property<Pixel32bppRGBA>;
            lineRenderer.Visible = Line.GetProperty("Visible") as Property<bool>;
            Line.Renderers.Add(lineRenderer);

            scene.Add(Line);
            treeView.Nodes.Add(new TreeNode($"{Line.name} [{Line.id}]") { Tag = Line });
            Console.WriteLine($"\tCreated: {Line.name} [{Line.id}]");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CreateCube();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            CreateCilinder();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            CreateCone();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            CreateSphere();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            CreatePolygone();
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            CreatePoint();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            CreateLine();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            CreateObject();
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            // Зчитайте код методу з файлу
            string code = @"
            using System;
            using PreviewMaker3D;
            using SharpGL;

            public class DynamicCode
            {
                public Animated Method()
                {
                    Animated Point = new Animated();
                    Point.name = ""Point"";
                    Point.AddProperty(new Variable<VectorFloat3D>(""Point"", new VectorFloat3D()));
                    Point.AddProperty(new Variable<Pixel32bppRGBA>(""Color"", new Pixel32bppRGBA()));
                    Point.AddProperty(new Variable<int>(""Size"", 8));
                    Point.AddProperty(new Variable<bool>(""Visible"", true));

                    Point1Renderer pointRenderer = new Point1Renderer(""Point"");
                    pointRenderer.Point = Point.GetProperty(""Point"") as Property<VectorFloat3D>;
                    pointRenderer.Color = Point.GetProperty(""Color"") as Property<Pixel32bppRGBA>;
                    pointRenderer.Size = Point.GetProperty(""Size"") as Property<int>;
                    pointRenderer.Visible = Point.GetProperty(""Visible"") as Property<bool>;
                    Point.Renderers.Add(pointRenderer);

                    return Point;
                }

                public static string Ask(string name)
                {
                    return Console.ReadLine();
                }

                public class Point1Renderer : Renderer
                {
                    public Property<VectorFloat3D> Point;
                    public new Property<Pixel32bppRGBA> Color;
                    public Property<int> Size;
                    public new Property<bool> Visible;

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
            ";

            string code2 = @"
            using System;
            using PreviewMaker3D;

            public class DynamicCode
            {
                public Animated Method()
                {
                    Animated Cube = new Animated();
                    Cube.name = ""Cube"";
                    Cube.AddProperty(new AnimatedPropertyVectorFloat3D(""Position"", new VectorFloat3D()));
                    Cube.AddProperty(new AnimatedPropertyVectorFloat3D(""Rotation"", new VectorFloat3D()));
                    Cube.AddProperty(new AnimatedPropertyVectorFloat3D(""Scale"", new VectorFloat3D(1.0f, 1.0f, 1.0f)));
                    Cube.AddProperty(new AnimatedPropertyPixel32bppRGBA(""Color1"", new Pixel32bppRGBA(0, 0, 0)));
                    Cube.AddProperty(new AnimatedPropertyPixel32bppRGBA(""Color2"", new Pixel32bppRGBA(255, 0, 0)));
                    Cube.AddProperty(new AnimatedPropertyPixel32bppRGBA(""Color3"", new Pixel32bppRGBA(255, 255, 255)));
                    Cube.AddProperty(new AnimatedProperty<bool>(""Visible"", true));

                    ListOf<VectorFloat3D> Points = new ListOf<VectorFloat3D>(""Points"");
                    Property<VectorFloat3D> CubePosition = Cube.GetProperty(""Position"") as Property<VectorFloat3D>;
                    Property<VectorFloat3D> CubeRotation = Cube.GetProperty(""Rotation"") as Property<VectorFloat3D>;
                    Property<VectorFloat3D> CubeScale = Cube.GetProperty(""Scale"") as Property<VectorFloat3D>;
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(1, 1, 1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(1, 1, -1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(1, -1, -1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(1, -1, 1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(-1, 1, 1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(-1, 1, -1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(-1, -1, -1))));
                    Points.Add(new ExpressionVf3DGP(CubePosition, CubeRotation, CubeScale,
                        new Variable<VectorFloat3D>(new VectorFloat3D(-1, -1, 1))));
                    Cube.AddProperty(Points);
                    PointRenderer pointRenderer = new PointRenderer(""CubePoints"");
                    pointRenderer.Points = Cube.GetProperty(""Points"") as ListOf<VectorFloat3D>;
                    pointRenderer.Color = Cube.GetProperty(""Color1"") as Property<Pixel32bppRGBA>;
                    pointRenderer.Visible = Cube.GetProperty(""Visible"") as Property<bool>;
                    Cube.Renderers.Add(pointRenderer);

                    ListOf<Line> Lines = new ListOf<Line>(""Lines"");
                    Lines.Add(new Line(0, 1));
                    Lines.Add(new Line(1, 2));
                    Lines.Add(new Line(2, 3));
                    Lines.Add(new Line(3, 0));
                    Lines.Add(new Line(4, 5));
                    Lines.Add(new Line(5, 6));
                    Lines.Add(new Line(6, 7));
                    Lines.Add(new Line(7, 4));
                    Lines.Add(new Line(0, 4));
                    Lines.Add(new Line(1, 5));
                    Lines.Add(new Line(2, 6));
                    Lines.Add(new Line(3, 7));
                    Cube.AddProperty(Lines);
                    LineRenderer lineRenderer = new LineRenderer(""CubeLines"");
                    lineRenderer.Points = Cube.GetProperty(""Points"") as ListOf<VectorFloat3D>;
                    lineRenderer.Lines = Cube.GetProperty(""Lines"") as ListOf<Line>;
                    lineRenderer.Color = Cube.GetProperty(""Color2"") as Property<Pixel32bppRGBA>;
                    lineRenderer.Visible = Cube.GetProperty(""Visible"") as Property<bool>;
                    Cube.Renderers.Add(lineRenderer);

                    ListOf<Poly> Polygones = new ListOf<Poly>(""Polys"");
                    Polygones.Add(new Poly(2, 1, 0));
                    Polygones.Add(new Poly(0, 3, 2));
                    Polygones.Add(new Poly(4, 5, 6));
                    Polygones.Add(new Poly(6, 7, 4));
                    Polygones.Add(new Poly(5, 4, 0));
                    Polygones.Add(new Poly(0, 1, 5));
                    Polygones.Add(new Poly(6, 5, 1));
                    Polygones.Add(new Poly(1, 2, 6));
                    Polygones.Add(new Poly(7, 6, 2));
                    Polygones.Add(new Poly(2, 3, 7));
                    Polygones.Add(new Poly(0, 7, 3));
                    Polygones.Add(new Poly(0, 4, 7));
                    Cube.AddProperty(Polygones);
                    PolyRenderer polyRenderer = new PolyRenderer(""CubePolygones"");
                    polyRenderer.Points = Cube.GetProperty(""Points"") as ListOf<VectorFloat3D>;
                    polyRenderer.Polygones = Cube.GetProperty(""Polys"") as ListOf<Poly>;
                    polyRenderer.Color = Cube.GetProperty(""Color3"") as Property<Pixel32bppRGBA>;
                    polyRenderer.Visible = Cube.GetProperty(""Visible"") as Property<bool>;
                    Cube.Renderers.Add(polyRenderer);

                    return Cube;
                }

                public static string Ask(string name)
                {
                    return Console.ReadLine();
                }
            }
            ";

            // Створіть компілятор C#
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            // Додайте залежності
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("SharpGL.dll");
            parameters.ReferencedAssemblies.Add("PreviewMaker3D.exe"); // Додайте вашу програму

            // Скомпілюйте код
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            // Перевірте, чи в компіляції не виникли помилки
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine($"Error in line {error.Line}: {error.ErrorText}");
                }
            }
            else
            {
                // Отримайте скомпільовану програму
                Assembly assembly = results.CompiledAssembly;

                // Створіть екземпляр класу і викличте метод
                Type dynamicCodeType = assembly.GetType("DynamicCode");

                Animated Object = (Animated)dynamicCodeType.GetMethod("Method").Invoke(Activator.CreateInstance(dynamicCodeType), null);

                scene.Add(Object);
                treeView.Nodes.Add(new TreeNode($"{Object.name} [{Object.id}]") { Tag = Object });
                Console.WriteLine($"\tAdded: {Object.name} [{Object.id}]");
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            AddTexture();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            RenderImage();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            RenderVideo();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            LoadProject();
        }
    }
}
