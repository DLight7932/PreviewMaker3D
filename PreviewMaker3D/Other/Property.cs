using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PreviewMaker3D
{
    public abstract class Property
    {
        public string Name;
    }

    public class Property<T> : Property
    {
        public T Value;

        public Property(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
