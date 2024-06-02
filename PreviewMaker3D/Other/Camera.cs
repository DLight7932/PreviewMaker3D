using System;
using System.Collections.Generic;
using System.Text;
using SharpGL;

namespace PreviewMaker3D
{
    public class Camera
    {
        public VectorFloat3D Normal
        {
            get
            {
                VectorFloat3D Center = new VectorFloat3D(0.0f, 0.0f, 1.0f) % Rotation + Position;
                VectorFloat3D normal = Center - Position;
                float length = (float)Math.Sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
                normal.x /= length;
                normal.y /= length;
                normal.z /= length;
                return normal;
            }
        }

        public VectorFloat3D Position = default;
        public VectorFloat3D Rotation = default;

        public float speed = 0.1f;
        public float angle = 45.0f;

        public void LookAt(OpenGL GL)
        {
            VectorFloat3D Eye = Position;
            VectorFloat3D Center = new VectorFloat3D(0.0f, 0.0f, 1.0f) % Rotation + Position;
            VectorFloat3D Up = new VectorFloat3D(0.0f, 1.0f, 0.0f) % Rotation;

            GL.LookAt(Eye.x, Eye.y, Eye.z, Center.x, Center.y, Center.z, Up.x, Up.y, Up.z);
            GL.Perspective(angle, 0.0f, 1.0f, 100.0f);
        }

        public void MoveForward() => Position += new VectorFloat3D(0, 0, speed) % Rotation;
        public void MoveBack() => Position += new VectorFloat3D(0, 0, -speed) % Rotation;
        public void MoveRight() => Position += new VectorFloat3D(-speed, 0, 0) % Rotation;
        public void MoveLeft() => Position += new VectorFloat3D(speed, 0, 0) % Rotation;
        public void MoveUp() => Position += new VectorFloat3D(0, speed, 0);
        public void MoveDown() => Position += new VectorFloat3D(0, -speed, 0);

        public void MoveForward(float speed) => Position += new VectorFloat3D(0, 0, speed) % Rotation;
        public void MoveBack(float speed) => Position += new VectorFloat3D(0, 0, -speed) % Rotation;
    }
}
