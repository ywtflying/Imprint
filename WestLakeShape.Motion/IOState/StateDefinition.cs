using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Motion
{
    /// <summary>
    /// 状态定义，包含名称和值类型。该类型在判断等同性时具有值语义
    /// </summary>
    public sealed class StateDefinition : IEquatable<StateDefinition>
    {
        public StateDefinition(string name, bool type)
        {
            Name = name;
            Type = type;
        }


        /// <summary>
        /// 状态名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 值类型
        /// </summary>
        public bool Type { get; }


        public bool Equals(StateDefinition other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(other, this))
                return true;

            return Name == other.Name && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StateDefinition);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }


        public static bool operator ==(StateDefinition a, StateDefinition b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Name == b.Name && a.Type == b.Type;
        }

        public static bool operator !=(StateDefinition a, StateDefinition b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"{Name}({Type})";
        }
    }
}
