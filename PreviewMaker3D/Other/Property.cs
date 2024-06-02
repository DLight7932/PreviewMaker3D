using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PreviewMaker3D
{
    public abstract class PropertyBase
    {
        static int CurrentID = 0;
        public int id;

        public static int Time;

        public string Name = "Unnamed";

        public PropertyBase()
        {
            id = CurrentID;
            CurrentID++;
        }

        public PropertyBase(string name_) : this()
        {
            Name = name_;
        }
    }

    public abstract class Property<T> : PropertyBase
    {
        public delegate bool DelegateCheckValue(T newValue);
        [JsonIgnore]
        public DelegateCheckValue checkValue = (T newValue) => true;

        public T defaultValue;

        public Property() { }
        public Property(string name_) : base(name_) { }

        [JsonIgnore]
        public abstract T Value { get; }
        public virtual void SetValue(T newValue) { }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Variable<T> : Property<T>
    {
        T value;

        public Variable(T value_)
        {
            value = value_;
            defaultValue = value_;
        }

        public Variable(string name_, T value_) : base(name_)
        {
            value = value_;
            defaultValue = value_;
        }

        public override T Value => value;

        public override void SetValue(T newValue)
        {
            if (!checkValue(newValue))
                return;

            value = newValue;
        }
    }

    public interface IAnimatedProperty
    {
        List<ITime> Keys { get; }

        void AddKey(int time);
        void RemoveKey(int time);
        void MoveKey(int time, int destinyTime);
    }

    public class ITime
    {
        public int time;
    }

    public class Key<T> : ITime
    {
        public T value;

        public Key(T value_, int time_)
        {
            value = value_;
            time = time_;
        }
    }

    public class AnimatedProperty<T> : Property<T>, IAnimatedProperty
    {
        public List<Key<T>> TimeLine = new List<Key<T>>();

        public List<ITime> Keys
        {
            get
            {
                List<ITime> result = new List<ITime>();
                foreach (Key<T> key in TimeLine)
                    result.Add(key);
                return result;
            }
        }

        public AnimatedProperty(T value_)
        {
            defaultValue = value_;
        }

        public AnimatedProperty(string name_, T value_) : base(name_)
        {
            defaultValue = value_;
        }

        public override T Value
        {
            get
            {
                if (TimeLine.Count == 0)
                    return defaultValue;
                if (TimeLine[0].time >= Time)
                    return TimeLine[0].value;
                int i = 0;
                for (; i < TimeLine.Count && TimeLine[i].time <= Time; i++) ;
                return TimeLine[i - 1].value;
            }
        }

        public override void SetValue(T newValue)
        {
            if (!checkValue(newValue))
                return;

            for (int i = 0; i < TimeLine.Count; i++)
                if (TimeLine[i].time == Time)
                {
                    TimeLine[i].value = newValue;
                    return;
                }
            AddKey(Time);
            SetValue(newValue);
        }

        public void AddKey(int time)
        {
            TimeLine.Add(new Key<T>(Value, time));
            TimeLine.Sort((key1, key2) => key1.time.CompareTo(key2.time));
        }

        public void RemoveKey(int time)
        {
            for (int i = 0; i < TimeLine.Count; i++)
                if (TimeLine[i].time == time)
                {
                    TimeLine.RemoveAt(i);
                    break;
                }
        }

        public void MoveKey(int time, int destinyTime)
        {
            foreach (Key<T> key in TimeLine)
                if (key.time == destinyTime)
                    return;

            foreach (Key<T> key in TimeLine)
                if (key.time == time)
                    key.time = destinyTime;
        }
    }

    public class AnimatedPropertyVectorFloat3D : AnimatedProperty<VectorFloat3D>
    {
        public AnimatedPropertyVectorFloat3D(VectorFloat3D value_) : base(value_) { }
        public AnimatedPropertyVectorFloat3D(string name_, VectorFloat3D value_) : base(name_, value_) { }

        public override VectorFloat3D Value
        {
            get
            {
                if (TimeLine.Count == 0)
                    return defaultValue;
                if (Time <= TimeLine[0].time)
                    return TimeLine[0].value;
                if (Time >= TimeLine[TimeLine.Count - 1].time)
                    return TimeLine[TimeLine.Count - 1].value;
                int i = 0;
                for (; i < TimeLine.Count; i++)
                    if (TimeLine[i].time > Time)
                        return VectorFloat3D.Interpolate(
                            TimeLine[i - 1].value,
                            TimeLine[i].value,
                            TimeLine[i].time - TimeLine[i - 1].time,
                            Time - TimeLine[i - 1].time);
                return defaultValue;
            }
        }

    }

    public class AnimatedPropertyPixel32bppRGBA : AnimatedProperty<Pixel32bppRGBA>
    {
        public AnimatedPropertyPixel32bppRGBA(Pixel32bppRGBA value_) : base(value_) { }
        public AnimatedPropertyPixel32bppRGBA(string name_, Pixel32bppRGBA value_) : base(name_, value_) { }

        public override Pixel32bppRGBA Value
        {
            get
            {
                if (TimeLine.Count == 0)
                    return defaultValue;
                if (Time <= TimeLine[0].time)
                    return TimeLine[0].value;
                if (Time >= TimeLine[TimeLine.Count - 1].time)
                    return TimeLine[TimeLine.Count - 1].value;
                int i = 0;
                for (; i < TimeLine.Count; i++)
                    if (TimeLine[i].time > Time)
                        return Pixel32bppRGBA.Interpolate(
                            TimeLine[i - 1].value,
                            TimeLine[i].value,
                            TimeLine[i].time - TimeLine[i - 1].time,
                            Time - TimeLine[i - 1].time);
                return defaultValue;
            }
        }

    }


    public class ListOf<T> : PropertyBase
    {
        List<Property<T>> List = new List<Property<T>>();

        public ListOf() { }
        public ListOf(string name_) : base(name_) { }

        public int Count => List.Count;

        public void Add(Property<T> element) => List.Add(element);
        public void Add(T element) => List.Add(new Variable<T>("", element));

        public Property<T> this[int i] => List[i];
    }


    public class ExpressionVf3PlusVf3 : Property<VectorFloat3D>
    {
        public Property<VectorFloat3D> Operand1;
        public Property<VectorFloat3D> Operand2;

        public ExpressionVf3PlusVf3(
            Property<VectorFloat3D> Operand1_,
            Property<VectorFloat3D> Operand2_)
        {
            Operand1 = Operand1_;
            Operand2 = Operand2_;
        }

        public ExpressionVf3PlusVf3(
            string name_,
            Property<VectorFloat3D> Operand1_,
            Property<VectorFloat3D> Operand2_) : base(name_)
        {
            Operand1 = Operand1_;
            Operand2 = Operand2_;
        }

        public override VectorFloat3D Value => Operand1.Value + Operand2.Value;

        public override string ToString()
        {
            return $"{Operand1.Name} + {Operand2.Name}";
        }
    }

    public class ExpressionVf3DivInt : Property<VectorFloat3D>
    {
        public Property<VectorFloat3D> Operand1;
        public Property<int> Operand2;

        public ExpressionVf3DivInt(
            Property<VectorFloat3D> Operand1_,
            Property<int> Operand2_)
        {
            Operand1 = Operand1_;
            Operand2 = Operand2_;
        }

        public ExpressionVf3DivInt(
            string name_,
            Property<VectorFloat3D> Operand1_,
            Property<int> Operand2_) : base(name_)
        {
            Operand1 = Operand1_;
            Operand2 = Operand2_;
        }

        public override VectorFloat3D Value => Operand1.Value / Operand2.Value;

        public override string ToString()
        {
            return $"{Operand1.Name} + {Operand2.Name}";
        }
    }

    public class ExpressionVf3MulFloat : Property<VectorFloat3D>
    {
        public Property<VectorFloat3D> Operand1;
        public Property<float> Operand2;

        public ExpressionVf3MulFloat(
            Property<VectorFloat3D> Operand1_,
            Property<float> Operand2_)
        {
            Operand1 = Operand1_;
            Operand2 = Operand2_;
        }

        public ExpressionVf3MulFloat(
            string name_,
            Property<VectorFloat3D> Operand1_,
            Property<float> Operand2_) : base(name_)
        {
            Operand1 = Operand1_;
            Operand2 = Operand2_;
        }

        public override VectorFloat3D Value => Operand1.Value * Operand2.Value;

        public override string ToString()
        {
            return $"{Operand1.Name} + {Operand2.Name}";
        }
    }

    public class ExpressionVf3DGP : Property<VectorFloat3D>
    {
        public Property<VectorFloat3D> ParentPosition;
        public Property<VectorFloat3D> ParentRotation;
        public Property<VectorFloat3D> ParentScale;
        public Property<VectorFloat3D> Position;

        public ExpressionVf3DGP(
            Property<VectorFloat3D> ParentPosition_,
            Property<VectorFloat3D> ParentRotation_,
            Property<VectorFloat3D> ParentScale_,
            Property<VectorFloat3D> Position_)
        {
            ParentPosition = ParentPosition_;
            ParentRotation = ParentRotation_;
            ParentScale = ParentScale_;
            Position = Position_;
        }

        public ExpressionVf3DGP(
            string name_,
            Property<VectorFloat3D> ParentPosition_,
            Property<VectorFloat3D> ParentRotation_,
            Property<VectorFloat3D> ParentScale_,
            Property<VectorFloat3D> Position_) : base(name_)
        {
            ParentPosition = ParentPosition_;
            ParentRotation = ParentRotation_;
            ParentScale = ParentScale_;
            Position = Position_;
        }

        public override VectorFloat3D Value =>
            ParentPosition.Value + Position.Value * ParentScale.Value % ParentRotation.Value;

        public override string ToString()
        {
            return $"{ParentPosition.Name} + {Position.Name} * {ParentScale.Name} % {ParentRotation.Name}";
        }
    }
}
