using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviewMaker3D
{
    public partial class PropertyCreator : Form
    {
        public enum TypesEnum
        {
            Int,
            Bool,
            Float,
            Pixel32bppRGBA,
            VectorFloat3D,
            VectorFloat2D,
            VectorInt2D,
            Texture,
            GlobalPosition,
            Sum,
            Div
        }

        delegate bool CheckDelegate(string name, string value);

        public static object result;
        bool animatedValue;

        public PropertyCreator(bool animatedValue_)
        {
            animatedValue = animatedValue_;
            InitializeComponent();
            comboBox1.DataSource = Enum.GetValues(typeof(TypesEnum));
            comboBox1.SelectedIndex = -1;
            comboBox1.Text = "Type";
            comboBox1.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    switch ((TypesEnum)comboBox1.SelectedItem)
                    {
                        case TypesEnum.Int:
                            CreateValue("Int", "0", (string name, string value) =>
                            {
                                if (int.TryParse(value, out int result1))
                                    if (animatedValue)
                                        result = new AnimatedProperty<int>(name, int.Parse(value));
                                    else
                                        result = new Variable<int>(name, int.Parse(value));
                                return int.TryParse(value, out int result2);
                            });
                            break;
                        case TypesEnum.Float:
                            CreateValue("Float", "0", (string name, string value) =>
                            {
                                if (float.TryParse(value, out float result1))
                                    if (animatedValue)
                                        result = new AnimatedProperty<float>(name, float.Parse(value));
                                    else
                                        result = new Variable<float>(name, float.Parse(value));
                                return float.TryParse(value, out float result2);
                            });
                            break;
                        case TypesEnum.Bool:
                            CreateValue("Bool", "true", (string name, string value) =>
                            {
                                if (bool.TryParse(value, out bool result1))
                                    if (animatedValue)
                                        result = new AnimatedProperty<bool>(name, bool.Parse(value));
                                    else
                                        result = new Variable<bool>(name, bool.Parse(value));
                                return bool.TryParse(value, out bool result2);
                            });
                            break;
                        case TypesEnum.Pixel32bppRGBA:
                            CreateValue("Color", "000000", (string name, string value) =>
                            {
                                if (new Pixel32bppRGBA().TryParse(value))
                                    if (animatedValue)
                                        result = new AnimatedProperty<Pixel32bppRGBA>(name, Pixel32bppRGBA.Parse(value));
                                    else
                                        result = new Variable<Pixel32bppRGBA>(name, Pixel32bppRGBA.Parse(value));
                                return new Pixel32bppRGBA().TryParse(value);
                            });
                            break;
                        case TypesEnum.Texture:
                            CreateValue("Texture", "DEFAULT", (string name, string value) =>
                            {
                                if (animatedValue)
                                    result = new AnimatedProperty<MyTexture>(name, Textures.None);
                                else
                                    result = new Variable<MyTexture>(name, Textures.None);
                                return true;
                            });
                            break;
                        case TypesEnum.VectorFloat3D:
                            CreateValue("Point", "[0 0 0]", (string name, string value) =>
                            {
                                if (new VectorFloat3D().TryParse(value))
                                    if (animatedValue)
                                        result = new AnimatedProperty<VectorFloat3D>(name, VectorFloat3D.Parse(value));
                                    else
                                        result = new Variable<VectorFloat3D>(name, VectorFloat3D.Parse(value));
                                return new VectorFloat3D().TryParse(value);
                            });
                            break;
                        case TypesEnum.VectorFloat2D:
                            CreateValue("TextCoord", "[0 0]", (string name, string value) =>
                            {
                                if (new VectorFloat2D().TryParse(value))
                                    if (animatedValue)
                                        result = new AnimatedProperty<VectorFloat2D>(name, VectorFloat2D.Parse(value));
                                    else
                                        result = new Variable<VectorFloat2D>(name, VectorFloat2D.Parse(value));
                                return new VectorFloat2D().TryParse(value);
                            });
                            break;
                        case TypesEnum.VectorInt2D:
                            CreateValue("Point", "[0 0]", (string name, string value) =>
                             {
                                 if (new VectorInt2D().TryParse(value))
                                     if (animatedValue)
                                         result = new AnimatedProperty<VectorInt2D>(name, VectorInt2D.Parse(value));
                                     else
                                         result = new Variable<VectorInt2D>(name, VectorInt2D.Parse(value));
                                 return new VectorInt2D().TryParse(value);
                             });
                            break;
                        case TypesEnum.GlobalPosition:
                            CreateGlobalPosition();
                            break;
                        case TypesEnum.Sum:
                            CreateSum();
                            break;
                        case TypesEnum.Div:
                            CreateVecF3DDivInt();
                            break;
                    }
                };
        }

        void CreateValue(string name, string value, CheckDelegate checkDlegate)
        {
            for (int i = 0; i < Controls.Count; i++)
                if (Controls[i] != comboBox1)
                {
                    Controls.RemoveAt(i);
                    i--;
                }

            TextBox textBox1 = new TextBox() { Location = new Point(110, 10), Height = 24, Text = name, AutoSize = true };
            Controls.Add(textBox1);
            TextBox textBox2 = new TextBox() { Location = new Point(textBox1.Right + 10, 10), Height = 24, Text = value, AutoSize = true };
            Controls.Add(textBox2);

            Size = new Size(textBox2.Right + 30, 95);

            textBox2.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.KeyCode == Keys.Enter && textBox1.Text != "")
                {
                    if (!checkDlegate(textBox1.Text, textBox2.Text))
                        return;

                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
        }

        void CreateGlobalPosition()
        {
            for (int i = 0; i < Controls.Count; i++)
                if (Controls[i] != comboBox1)
                {
                    Controls.RemoveAt(i);
                    i--;
                }

            TextBox textBox1 = new TextBox() { Location = new Point(110, 10), Height = 24, Text = "Global Position", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(textBox1);
            Button button1 = new Button() { Location = new Point(textBox1.Right + 10, 10), Height = 24, Text = "Parent Position", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button1);
            Button button2 = new Button() { Location = new Point(button1.Right + 10, 10), Height = 24, Text = "Parent Rotation", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button2);
            Button button3 = new Button() { Location = new Point(button2.Right + 10, 10), Height = 24, Text = "Parent Scale", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button3);
            Button button4 = new Button() { Location = new Point(button3.Right + 10, 10), Height = 24, Text = "Position", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button4);

            Size = new Size(button4.Right + 30, 95);

            Property<VectorFloat3D> ParentPosition = null;
            Property<VectorFloat3D> ParentRotation = null;
            Property<VectorFloat3D> ParentScale = null;
            Property<VectorFloat3D> Position = null;

            textBox1.Enter += (object sender, EventArgs e) =>
            {
                textBox1.ForeColor = Color.Black;
            };

            button1.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;

                ParentPosition = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button1.Text = ParentPosition.Name;
                button1.ForeColor = Color.Black;
                CheckValues();
            };
            button2.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;

                ParentRotation = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button2.Text = ParentRotation.Name;
                button2.ForeColor = Color.Black;
                CheckValues();
            };
            button3.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;

                ParentScale = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button3.Text = ParentScale.Name;
                button3.ForeColor = Color.Black;
                CheckValues();
            };
            button4.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;

                Position = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button4.Text = Position.Name;
                button4.ForeColor = Color.Black;
                CheckValues();
            };

            void CheckValues()
            {
                if (ParentPosition == null ||
                    ParentRotation == null ||
                    ParentScale == null ||
                    Position == null)
                    return;

                result = new ExpressionVf3DGP(textBox1.Text,
                    ParentPosition,
                    ParentRotation,
                    ParentScale,
                    Position);

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        void CreateSum()
        {
            for (int i = 0; i < Controls.Count; i++)
                if (Controls[i] != comboBox1)
                {
                    Controls.RemoveAt(i);
                    i--;
                }

            TextBox textBox1 = new TextBox() { Location = new Point(110, 10), Height = 24, Text = "Vector Sum", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(textBox1);
            Button button1 = new Button() { Location = new Point(textBox1.Right + 10, 10), Height = 24, Text = "Operand 1", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button1);
            Button button2 = new Button() { Location = new Point(button1.Right + 10, 10), Height = 24, Text = "Operand 2", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button2);

            Size = new Size(button2.Right + 30, 95);

            Property<VectorFloat3D> operand1 = null;
            Property<VectorFloat3D> operand2 = null;

            textBox1.Enter += (object sender, EventArgs e) =>
            {
                textBox1.ForeColor = Color.Black;
            };

            button1.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;

                operand1 = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button1.Text = operand1.Name;
                button1.ForeColor = Color.Black;
                CheckValues();
            };
            button2.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;
                operand2 = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button2.Text = operand2.Name;
                button2.ForeColor = Color.Black;
                CheckValues();
            };

            void CheckValues()
            {
                if (operand1 == null ||
                    operand2 == null)
                    return;

                result = new ExpressionVf3PlusVf3(textBox1.Text,
                    operand1,
                    operand2);

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        void CreateVecF3DDivInt()
        {
            for (int i = 0; i < Controls.Count; i++)
                if (Controls[i] != comboBox1)
                {
                    Controls.RemoveAt(i);
                    i--;
                }

            TextBox textBox1 = new TextBox() { Location = new Point(110, 10), Height = 24, Text = "Vector Div Num", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(textBox1);
            Button button1 = new Button() { Location = new Point(textBox1.Right + 10, 10), Height = 24, Text = "Operand 1", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button1);
            Button button2 = new Button() { Location = new Point(button1.Right + 10, 10), Height = 24, Text = "Operand 2", AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(button2);

            Size = new Size(button2.Right + 30, 95);

            Property<VectorFloat3D> operand1 = null;
            Property<int> operand2 = null;

            textBox1.Enter += (object sender, EventArgs e) =>
            {
                textBox1.ForeColor = Color.Black;
            };

            button1.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.VectorFloat3D).ShowDialog() != DialogResult.OK) return;

                operand1 = PropertySelecter.SelectedProperty as Property<VectorFloat3D>;
                button1.Text = operand1.Name;
                button1.ForeColor = Color.Black;
                CheckValues();
            };
            button2.Click += (object sender, EventArgs e) =>
            {
                if (new PropertySelecter(TypesEnum.Int).ShowDialog() != DialogResult.OK) return;

                operand2 = PropertySelecter.SelectedProperty as Property<int>;
                button2.Text = operand2.Name;
                button2.ForeColor = Color.Black;
                CheckValues();
            };

            void CheckValues()
            {
                if (operand1 == null ||
                    operand2 == null)
                    return;

                result = new ExpressionVf3DivInt(textBox1.Text,
                    operand1,
                    operand2);

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
