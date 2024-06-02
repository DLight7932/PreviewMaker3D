using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace PreviewMaker3D
{
    public class PropertyModifier : GroupBox
    {
        protected bool closed = false;
        protected int minimizedSize;
        protected int maximizedSize;
        PropertyBase Property;

        public PropertyModifier(PropertyBase Property_)
        {
            Property = Property_;

            minimizedSize = 25;
            maximizedSize = 25;
            Height = 25;

            MouseClick += (object sender, MouseEventArgs e) =>
            {
                if (e.Y >= 25) return;

                if (closed)
                {
                    closed = false;
                    foreach (Control control in Controls)
                        control.Visible = false;
                    Height = minimizedSize;
                }
                else
                {
                    closed = true;
                    foreach (Control control in Controls)
                        control.Visible = true;
                    Height = maximizedSize;
                }
            };

            MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Right)
                    MainForm.DeleteProperty(Property);
            };

            Text = Property.Name;
        }

        public virtual void RefreshModifier()
        {
            Text = Property.Name;
        }
    }


    public class FloatModifier : TextBox
    {
        bool mouseDown;
        bool mouseMoving;
        bool editingText;
        Point mousePosition;

        public delegate float DelegateGetValue();
        public delegate void DelegateSetValue(float value);
        public delegate void DelegateResetValue();
        public DelegateGetValue GetValue;
        public DelegateSetValue SetValue;
        public DelegateResetValue ResetValue;

        public FloatModifier()
        {
            ShortcutsEnabled = false;

            MouseEnter += (object sender, EventArgs e) =>
            {
                if (!editingText)
                    Cursor = Cursors.SizeWE;
            };
            MouseLeave += (object sender, EventArgs e) =>
            {
                Cursor = Cursors.Default;
            };

            MouseDown += (object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ResetValue();
                    RefreshText();
                    MainForm.UnFocus();
                    return;
                }

                mouseDown = true;
                mousePosition = Cursor.Position;
            };
            MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (mouseMoving)
                    MainForm.UnFocus();

                else if (!editingText && e.Button == MouseButtons.Left)
                {
                    editingText = true;
                    SelectAll();
                    Cursor = Cursors.Default;
                }

                mouseDown = false;
                mouseMoving = false;
            };

            KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    MainForm.UnFocus();
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Enter)
                {
                    MainForm.UnFocus();
                    e.SuppressKeyPress = true;
                }
                if (float.TryParse(Text + (char)e.KeyCode, out float result))
                    SetValue(result);
            };

            MouseMove += (object sender, MouseEventArgs e) =>
            {
                if (mouseDown && !editingText)
                {
                    mouseMoving = true;
                    DeselectAll();
                }
                if (mouseMoving)
                {
                    SetValue(GetValue() + (Cursor.Position.X - mousePosition.X) / 10.0f);
                    Cursor.Position = mousePosition;
                    RefreshText();
                }
            };

            Leave += (object sender, EventArgs e) =>
            {
                mouseDown = false;
                mouseMoving = false;
                editingText = false;
                RefreshText();
                MainForm.UnFocus();
            };
        }

        public void RefreshText()
        {
            Text = GetValue().ToString("F3").TrimEnd('0').TrimEnd('.');
        }
    }

    public class FloatPropertyModifier : PropertyModifier
    {
        protected Property<float> Property;

        public override void RefreshModifier()
        {
            Modifier.RefreshText();
        }

        FloatModifier Modifier = new FloatModifier();

        public FloatPropertyModifier(Property<float> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Controls.Add(Modifier);

            Resize += (object sender, EventArgs e) =>
            {
                t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = t.Right - t.Left;
                Controls.Remove(t);

                Modifier.Location = new Point(t.Left, t.Top);
                Modifier.Width = width;
            };

            Modifier.GetValue = () => Property.Value;

            Modifier.SetValue = (float value) => Property.SetValue(value);

            Modifier.ResetValue = () => Property.SetValue(Property.defaultValue);

            RefreshModifier();
        }
    }

    public class VectorFloat3DPropertyModifier : PropertyModifier
    {
        protected Property<VectorFloat3D> Property;

        FloatModifier XModifier = new FloatModifier();
        FloatModifier YModifier = new FloatModifier();
        FloatModifier ZModifier = new FloatModifier();

        public override void RefreshModifier()
        {
            XModifier.RefreshText();
            YModifier.RefreshText();
            ZModifier.RefreshText();
        }

        public VectorFloat3DPropertyModifier(Property<VectorFloat3D> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Controls.Add(XModifier);
            Controls.Add(YModifier);
            Controls.Add(ZModifier);

            Resize += (object sender, EventArgs e) =>
            {
                t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = (t.Right - t.Left) / 3;
                Controls.Remove(t);

                XModifier.Location = new Point(t.Left, t.Top);
                XModifier.Width = width;
                YModifier.Location = new Point(XModifier.Right, t.Top);
                YModifier.Width = width;
                ZModifier.Location = new Point(YModifier.Right, t.Top);
                ZModifier.Width = width;
            };

            XModifier.GetValue = () => Property.Value.x;
            YModifier.GetValue = () => Property.Value.y;
            ZModifier.GetValue = () => Property.Value.z;

            XModifier.SetValue = (float value) => Property.SetValue(new VectorFloat3D(value, Property.Value.y, Property.Value.z));
            YModifier.SetValue = (float value) => Property.SetValue(new VectorFloat3D(Property.Value.x, value, Property.Value.z));
            ZModifier.SetValue = (float value) => Property.SetValue(new VectorFloat3D(Property.Value.x, Property.Value.y, value));

            XModifier.ResetValue = () => Property.SetValue(new VectorFloat3D(Property.defaultValue.x, Property.Value.y, Property.Value.z));
            YModifier.ResetValue = () => Property.SetValue(new VectorFloat3D(Property.Value.x, Property.defaultValue.y, Property.Value.z));
            ZModifier.ResetValue = () => Property.SetValue(new VectorFloat3D(Property.Value.x, Property.Value.y, Property.defaultValue.z));

            RefreshModifier();
        }
    }

    public class VF3Modifier : TableLayoutPanel
    {
        protected Property<VectorFloat3D> Property;

        FloatModifier XModifier = new FloatModifier();
        FloatModifier YModifier = new FloatModifier();
        FloatModifier ZModifier = new FloatModifier();
        public Button button = new Button() { Text = "X" };

        public void RefreshModifier()
        {
            XModifier.RefreshText();
            YModifier.RefreshText();
            ZModifier.RefreshText();
        }

        public VF3Modifier(Property<VectorFloat3D> property_)
        {
            ColumnCount = 4;
            RowCount = 1;

            Dock = DockStyle.Top;
            Property = property_;

            button.Margin = new Padding(0, 0, 0, 0);
            XModifier.Margin = new Padding(0, 0, 0, 0);
            YModifier.Margin = new Padding(0, 0, 0, 0);
            ZModifier.Margin = new Padding(0, 0, 0, 0);
            Controls.Add(button);
            Controls.Add(XModifier);
            Controls.Add(YModifier);
            Controls.Add(ZModifier);

            //button.Click += (object sender, EventArgs e) =>
            //{
            //    property_.Value.RemoveAt(index);
            //};

            Height = XModifier.Bottom + 5;

            Resize += (object sender, EventArgs e) =>
            {
                TextBox t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = Width / 4;
                Controls.Remove(t);

                button.Location = new Point(t.Left, t.Top);
                button.Width = width;
                button.Height = XModifier.Height;
                XModifier.Location = new Point(button.Left, t.Top);
                XModifier.Width = width;
                YModifier.Location = new Point(XModifier.Right, t.Top);
                YModifier.Width = width;
                ZModifier.Location = new Point(YModifier.Right, t.Top);
                ZModifier.Width = width;
            };

            XModifier.GetValue = () => Property.Value.x;
            YModifier.GetValue = () => Property.Value.y;
            ZModifier.GetValue = () => Property.Value.z;

            XModifier.SetValue = (float value) => Property.SetValue(new VectorFloat3D(value, Property.Value.y, Property.Value.z));
            YModifier.SetValue = (float value) => Property.SetValue(new VectorFloat3D(Property.Value.x, value, Property.Value.z));
            ZModifier.SetValue = (float value) => Property.SetValue(new VectorFloat3D(Property.Value.x, Property.Value.y, value));

            XModifier.ResetValue = () => Property.SetValue(new VectorFloat3D(0, Property.Value.y, Property.Value.z));
            YModifier.ResetValue = () => Property.SetValue(new VectorFloat3D(Property.Value.x, 0, Property.Value.z));
            ZModifier.ResetValue = () => Property.SetValue(new VectorFloat3D(Property.Value.x, Property.Value.y, 0));

            RefreshModifier();
        }
    }

    public class ListOfVF3PropertyModifier : PropertyModifier
    {
        protected ListOf<VectorFloat3D> Property;

        Property<List<VF3Modifier>> Modifiers = new Variable<List<VF3Modifier>>("Points", new List<VF3Modifier>());
        Button button;

        public override void RefreshModifier()
        {
            Controls.Clear();
            Modifiers.Value.Clear();
            for (int i = 0; i < Property.Count; i++)
            {
                VF3Modifier modifier = new VF3Modifier(Property[i]);

                modifier.ColumnStyles.Add(new ColumnStyle());
                modifier.ColumnStyles.Add(new ColumnStyle());

                Controls.Add(modifier);
                Modifiers.Value.Add(modifier);

                modifier.button.Click += (object sender, EventArgs e) =>
                {
                    RefreshModifier();
                };
            }
            Controls.Add(button);
            if (Modifiers.Value.Count > 0)
                maximizedSize = Modifiers.Value[0].Bottom + 5;
            else
                maximizedSize = button.Bottom + 5;
            Height = maximizedSize;
        }

        public ListOfVF3PropertyModifier(ListOf<VectorFloat3D> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            minimizedSize = 25;

            button = new Button() { Text = "Add", Dock = DockStyle.Top };

            RefreshModifier();

            button.Click += (object sender, EventArgs e) =>
            {
                Property.Add(new Variable<VectorFloat3D>("", new VectorFloat3D()));
                RefreshModifier();
            };

            Resize += (object sender, EventArgs e) =>
            {
                TextBox t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = (t.Right - t.Left) / 3;
                Controls.Remove(t);

                foreach (VF3Modifier Modifier in Modifiers.Value)
                    Modifier.Width = width;
            };

            RefreshModifier();
        }
    }

    public class VectorFloat2DPropertyModifier : PropertyModifier
    {
        protected Property<VectorFloat2D> Property;

        FloatModifier XModifier = new FloatModifier();
        FloatModifier YModifier = new FloatModifier();

        public override void RefreshModifier()
        {
            XModifier.RefreshText();
            YModifier.RefreshText();
        }

        public VectorFloat2DPropertyModifier(Property<VectorFloat2D> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Controls.Add(XModifier);
            Controls.Add(YModifier);

            Resize += (object sender, EventArgs e) =>
            {
                t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = (t.Right - t.Left) / 2;
                Controls.Remove(t);

                XModifier.Location = new Point(t.Left, t.Top);
                XModifier.Width = width;
                YModifier.Location = new Point(XModifier.Right, t.Top);
                YModifier.Width = width;
            };

            XModifier.GetValue = () => Property.Value.x;
            YModifier.GetValue = () => Property.Value.y;

            XModifier.SetValue = (float value) => Property.SetValue(new VectorFloat2D(value, Property.Value.y));
            YModifier.SetValue = (float value) => Property.SetValue(new VectorFloat2D(Property.Value.x, value));

            XModifier.ResetValue = () => Property.SetValue(new VectorFloat2D(Property.defaultValue.x, Property.Value.y));
            YModifier.ResetValue = () => Property.SetValue(new VectorFloat2D(Property.Value.x, Property.defaultValue.y));

            RefreshModifier();
        }
    }

    public class LineModifier : TableLayoutPanel
    {
        protected Property<Line> Property;

        IntModifier XModifier = new IntModifier();
        IntModifier YModifier = new IntModifier();
        public Button button = new Button() { Text = "X" };

        public void RefreshModifier()
        {
            XModifier.RefreshText();
            YModifier.RefreshText();
        }

        public LineModifier(Property<Line> property_)
        {
            ColumnCount = 3;
            RowCount = 1;

            Dock = DockStyle.Top;
            Property = property_;

            button.Margin = new Padding(0, 0, 0, 0);
            XModifier.Margin = new Padding(0, 0, 0, 0);
            YModifier.Margin = new Padding(0, 0, 0, 0);
            Controls.Add(button);
            Controls.Add(XModifier);
            Controls.Add(YModifier);

            //button.Click += (object sender, EventArgs e) =>
            //{
            //    property_.Value.RemoveAt(index);
            //};

            Height = XModifier.Bottom + 5;

            Resize += (object sender, EventArgs e) =>
            {
                TextBox t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = Width / 3;
                Controls.Remove(t);

                button.Location = new Point(t.Left, t.Top);
                button.Width = width;
                button.Height = XModifier.Height;
                XModifier.Location = new Point(button.Left, t.Top);
                XModifier.Width = width;
                YModifier.Location = new Point(XModifier.Right, t.Top);
                YModifier.Width = width;
            };

            XModifier.GetValue = () => Property.Value.p1;
            YModifier.GetValue = () => Property.Value.p2;

            XModifier.SetValue = (int value) => Property.SetValue(new Line(value, Property.Value.p2));
            YModifier.SetValue = (int value) => Property.SetValue(new Line(Property.Value.p1, value));

            XModifier.ResetValue = () => Property.SetValue(new Line(0, Property.Value.p2));
            YModifier.ResetValue = () => Property.SetValue(new Line(Property.Value.p1, 0));

            RefreshModifier();
        }
    }

    public class ListOfLinePropertyModifier : PropertyModifier
    {
        protected ListOf<Line> Property;

        Property<List<LineModifier>> Modifiers = new Variable<List<LineModifier>>("Points", new List<LineModifier>());
        Button button;

        public override void RefreshModifier()
        {
            Controls.Clear();
            Modifiers.Value.Clear();
            for (int i = 0; i < Property.Count; i++)
            {
                LineModifier modifier = new LineModifier(Property[i]);

                modifier.ColumnStyles.Add(new ColumnStyle());
                modifier.ColumnStyles.Add(new ColumnStyle());

                Controls.Add(modifier);
                Modifiers.Value.Add(modifier);

                modifier.button.Click += (object sender, EventArgs e) =>
                {
                    RefreshModifier();
                };
            }
            Controls.Add(button);
            if (Modifiers.Value.Count > 0)
                maximizedSize = Modifiers.Value[0].Bottom + 5;
            else
                maximizedSize = button.Bottom + 5;
            Height = maximizedSize;
        }

        public ListOfLinePropertyModifier(ListOf<Line> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            minimizedSize = 25;

            button = new Button() { Text = "Add", Dock = DockStyle.Top };

            RefreshModifier();

            button.Click += (object sender, EventArgs e) =>
            {
                Property.Add(new Variable<Line>("", new Line()));
                RefreshModifier();
            };

            Resize += (object sender, EventArgs e) =>
            {
                TextBox t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = (t.Right - t.Left) / 3;
                Controls.Remove(t);

                foreach (LineModifier Modifier in Modifiers.Value)
                    Modifier.Width = width;
            };

            RefreshModifier();
        }
    }

    public class PolyModifier : TableLayoutPanel
    {
        protected Property<Poly> Property;

        IntModifier XModifier = new IntModifier();
        IntModifier YModifier = new IntModifier();
        IntModifier ZModifier = new IntModifier();
        public Button button = new Button() { Text = "X" };

        public void RefreshModifier()
        {
            XModifier.RefreshText();
            YModifier.RefreshText();
            ZModifier.RefreshText();
        }

        public PolyModifier(Property<Poly> property_)
        {
            ColumnCount = 4;
            RowCount = 1;

            Dock = DockStyle.Top;
            Property = property_;

            button.Margin = new Padding(0, 0, 0, 0);
            XModifier.Margin = new Padding(0, 0, 0, 0);
            YModifier.Margin = new Padding(0, 0, 0, 0);
            ZModifier.Margin = new Padding(0, 0, 0, 0);
            Controls.Add(button);
            Controls.Add(XModifier);
            Controls.Add(YModifier);
            Controls.Add(ZModifier);

            //button.Click += (object sender, EventArgs e) =>
            //{
            //    property_.Value.RemoveAt(index);
            //};

            Height = XModifier.Bottom + 5;

            Resize += (object sender, EventArgs e) =>
            {
                TextBox t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = Width / 4;
                Controls.Remove(t);

                button.Location = new Point(t.Left, t.Top);
                button.Width = width;
                button.Height = XModifier.Height;
                XModifier.Location = new Point(button.Left, t.Top);
                XModifier.Width = width;
                YModifier.Location = new Point(XModifier.Right, t.Top);
                YModifier.Width = width;
                ZModifier.Location = new Point(YModifier.Right, t.Top);
                ZModifier.Width = width;
            };

            XModifier.GetValue = () => Property.Value.p1;
            YModifier.GetValue = () => Property.Value.p2;
            ZModifier.GetValue = () => Property.Value.p3;

            XModifier.SetValue = (int value) => Property.SetValue(new Poly(value, Property.Value.p2, Property.Value.p3));
            YModifier.SetValue = (int value) => Property.SetValue(new Poly(Property.Value.p1, value, Property.Value.p3));
            ZModifier.SetValue = (int value) => Property.SetValue(new Poly(Property.Value.p1, Property.Value.p2, value));

            XModifier.ResetValue = () => Property.SetValue(new Poly(0, Property.Value.p2, Property.Value.p3));
            YModifier.ResetValue = () => Property.SetValue(new Poly(Property.Value.p1, 0, Property.Value.p3));
            ZModifier.ResetValue = () => Property.SetValue(new Poly(Property.Value.p1, Property.Value.p2, 0));

            RefreshModifier();
        }
    }

    public class ListOfPolyPropertyModifier : PropertyModifier
    {
        protected ListOf<Poly> Property;

        Property<List<PolyModifier>> Modifiers = new Variable<List<PolyModifier>>("Points", new List<PolyModifier>());
        Button button;

        public override void RefreshModifier()
        {
            Controls.Clear();
            Modifiers.Value.Clear();
            for (int i = 0; i < Property.Count; i++)
            {
                PolyModifier modifier = new PolyModifier(Property[i]);

                modifier.ColumnStyles.Add(new ColumnStyle());
                modifier.ColumnStyles.Add(new ColumnStyle());

                Controls.Add(modifier);
                Modifiers.Value.Add(modifier);

                modifier.button.Click += (object sender, EventArgs e) =>
                {
                    RefreshModifier();
                };
            }
            Controls.Add(button);
            if (Modifiers.Value.Count > 0)
                maximizedSize = Modifiers.Value[0].Bottom + 5;
            else
                maximizedSize = button.Bottom + 5;
            Height = maximizedSize;
        }

        public ListOfPolyPropertyModifier(ListOf<Poly> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            minimizedSize = 25;

            button = new Button() { Text = "Add", Dock = DockStyle.Top };

            RefreshModifier();

            button.Click += (object sender, EventArgs e) =>
            {
                Property.Add(new Variable<Poly>("", new Poly()));
                RefreshModifier();
            };

            Resize += (object sender, EventArgs e) =>
            {
                TextBox t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = (t.Right - t.Left) / 3;
                Controls.Remove(t);

                foreach (PolyModifier Modifier in Modifiers.Value)
                    Modifier.Width = width;
            };

            RefreshModifier();
        }
    }

    public class Pixel32bppRGBAPropertyModifier : PropertyModifier
    {
        Property<Pixel32bppRGBA> Property;
        ColorDialog colorDialog = new ColorDialog();
        Button button = new Button();
        bool mouseDown;

        public override void RefreshModifier()
        {
            button.Text = Property.ToString();
            colorDialog.Color = Color.FromArgb(Property.Value.r, Property.Value.g, Property.Value.b);
            button.BackColor = colorDialog.Color;

            if (colorDialog.Color.R + colorDialog.Color.G + colorDialog.Color.B < 255)
                button.ForeColor = Color.White;
            else
                button.ForeColor = Color.Black;
        }

        public Pixel32bppRGBAPropertyModifier(Property<Pixel32bppRGBA> property_) : base(property_)
        {
            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;
            Controls.Add(button);
            button.Dock = DockStyle.Fill;

            button.MouseDown += (object sender, MouseEventArgs e) =>
            {
                mouseDown = true;
            };

            button.MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (!mouseDown) return;

                if (e.Button == MouseButtons.Left && colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Property.SetValue(new Pixel32bppRGBA(
                        colorDialog.Color.R,
                        colorDialog.Color.G,
                        colorDialog.Color.B));
                    RefreshModifier();
                }

                else if (e.Button == MouseButtons.Right)
                {
                    Property.SetValue(Property.defaultValue);
                    RefreshModifier();
                }
            };

            RefreshModifier();
        }
    }

    public class BoolPropertyModifier : PropertyModifier
    {
        Property<bool> Property;
        Button button = new Button();
        bool mouseDown;

        public override void RefreshModifier()
        {
            button.Text = Property.ToString();
        }

        public BoolPropertyModifier(Property<bool> property_) : base(property_)
        {
            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;
            Controls.Add(button);
            button.Dock = DockStyle.Fill;
            button.BackColor = Color.White;

            button.MouseDown += (object sender, MouseEventArgs e) =>
            {
                mouseDown = true;
            };

            button.MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (!mouseDown) return;

                if (e.Button == MouseButtons.Left)
                {
                    Property.SetValue(!Property.Value);
                    RefreshModifier();
                }

                else if (e.Button == MouseButtons.Right)
                {
                    Property.SetValue(Property.defaultValue);
                    RefreshModifier();
                }

                mouseDown = false;
            };

            RefreshModifier();
        }
    }


    public class IntModifier : TextBox
    {
        bool mouseDown;
        bool mouseMoving;
        bool editingText;
        Point mousePosition;

        public delegate int DelegateGetValue();
        public delegate void DelegateSetValue(int value);
        public delegate void DelegateResetValue();
        public DelegateGetValue GetValue;
        public DelegateSetValue SetValue;
        public DelegateResetValue ResetValue;

        public IntModifier()
        {
            ShortcutsEnabled = false;

            MouseEnter += (object sender, EventArgs e) =>
            {
                if (!editingText)
                    Cursor = Cursors.SizeWE;
            };
            MouseLeave += (object sender, EventArgs e) =>
            {
                Cursor = Cursors.Default;
            };

            MouseDown += (object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ResetValue();
                    RefreshText();
                    MainForm.UnFocus();
                    return;
                }

                mouseDown = true;
                mousePosition = Cursor.Position;
            };
            MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (!mouseDown) return;

                if (mouseMoving)
                    MainForm.UnFocus();

                else if (!editingText && e.Button == MouseButtons.Left)
                {
                    editingText = true;
                    SelectAll();
                    Cursor = Cursors.Default;
                }

                mouseDown = false;
                mouseMoving = false;
            };

            KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    MainForm.UnFocus();
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Enter)
                {
                    MainForm.UnFocus();
                    e.SuppressKeyPress = true;
                }
                if (int.TryParse(Text + (char)e.KeyCode, out int result))
                    SetValue(result);
            };

            MouseMove += (object sender, MouseEventArgs e) =>
            {
                if (mouseDown && !editingText)
                {
                    mouseMoving = true;
                    DeselectAll();
                }
                if (mouseMoving)
                {
                    SetValue(GetValue() + Cursor.Position.X - mousePosition.X);
                    Cursor.Position = mousePosition;
                    RefreshText();
                }
            };

            Leave += (object sender, EventArgs e) =>
            {
                mouseDown = false;
                mouseMoving = false;
                editingText = false;
                RefreshText();
                MainForm.UnFocus();
            };
        }

        public void RefreshText()
        {
            Text = GetValue().ToString();
        }
    }

    public class IntPropertyModifier : PropertyModifier
    {
        protected Property<int> Property;

        public override void RefreshModifier()
        {
            Modifier.RefreshText();
        }

        IntModifier Modifier = new IntModifier();

        public IntPropertyModifier(Property<int> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Controls.Add(Modifier);

            Resize += (object sender, EventArgs e) =>
            {
                t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = t.Right - t.Left;
                Controls.Remove(t);

                Modifier.Location = new Point(t.Left, t.Top);
                Modifier.Width = width;
            };

            Modifier.GetValue = () => Property.Value;

            Modifier.SetValue = (int value) => Property.SetValue(value);

            Modifier.ResetValue = () => Property.SetValue(Property.defaultValue);

            RefreshModifier();
        }
    }

    public class VectorInt2DPropertyModifier : PropertyModifier
    {
        protected Property<VectorInt2D> Property;

        IntModifier XModifier = new IntModifier();
        IntModifier YModifier = new IntModifier();

        public override void RefreshModifier()
        {
            XModifier.RefreshText();
            YModifier.RefreshText();
        }

        public VectorInt2DPropertyModifier(Property<VectorInt2D> property_) : base(property_)
        {
            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Controls.Add(XModifier);
            Controls.Add(YModifier);

            Resize += (object sender, EventArgs e) =>
            {
                t = new TextBox() { Dock = DockStyle.Fill };
                Controls.Add(t);
                int width = (t.Right - t.Left) / 2;
                Controls.Remove(t);

                XModifier.Location = new Point(t.Left, t.Top);
                XModifier.Width = width;
                YModifier.Location = new Point(XModifier.Right, t.Top);
                YModifier.Width = width;
            };

            XModifier.GetValue = () => Property.Value.x;
            YModifier.GetValue = () => Property.Value.y;

            XModifier.SetValue = (int value) => Property.SetValue(new VectorInt2D(value, Property.Value.y));
            YModifier.SetValue = (int value) => Property.SetValue(new VectorInt2D(Property.Value.x, value));

            XModifier.ResetValue = () => Property.SetValue(new VectorInt2D(Property.defaultValue.x, Property.Value.y));
            YModifier.ResetValue = () => Property.SetValue(new VectorInt2D(Property.Value.x, Property.defaultValue.y));

            RefreshModifier();
        }
    }


    public class TexturePropertyModifier : PropertyModifier
    {
        Property<MyTexture> Property;
        ComboBox comboBox = new ComboBox();
        PictureBox pictureBox = new PictureBox();

        bool mouseDown;

        public override void RefreshModifier()
        {
            comboBox.Text = $"{Property.Value.name} [{Property.Value.id}]";
            pictureBox.Image = Property.Value.bitmap;
        }

        public TexturePropertyModifier(Property<MyTexture> property_) : base(property_)
        {
            TextBox t = new TextBox() { Dock = DockStyle.Fill };
            Controls.Add(t);
            Height = t.Bottom + 5;
            minimizedSize = 25;
            maximizedSize = t.Bottom + 5;
            Controls.Remove(t);

            Dock = DockStyle.Top;
            Property = property_;
            Text = Property.Name;

            Controls.Add(comboBox);
            comboBox.Dock = DockStyle.Fill;
            comboBox.ContextMenuStrip = new ContextMenuStrip();

            Controls.Add(pictureBox);
            pictureBox.Dock = DockStyle.Right;
            pictureBox.BackColor = Color.White;
            pictureBox.Width = pictureBox.Height;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            comboBox.MouseDown += (object sender, MouseEventArgs e) =>
            {
                mouseDown = true;
            };
            comboBox.MouseUp += (object sender, MouseEventArgs e) =>
            {
                if (mouseDown)
                {
                    mouseDown = true;
                    Property.SetValue(Property.defaultValue);
                    RefreshModifier();
                }
                mouseDown = false;
            };

            comboBox.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                foreach (MyTexture texture in Textures.textures)
                    if (texture.name == comboBox.SelectedItem.ToString())
                        Property.SetValue(texture);

                RefreshModifier();
            };

            comboBox.DropDown += (object sender, EventArgs e) =>
            {
                comboBox.Items.Clear();
                foreach (MyTexture texture in Textures.textures)
                    comboBox.Items.Add(texture.name);
            };

            comboBox.Leave += (object sender, EventArgs e) =>
            {
                RefreshModifier();
                mouseDown = false;
            };

            RefreshModifier();
        }
    }
}