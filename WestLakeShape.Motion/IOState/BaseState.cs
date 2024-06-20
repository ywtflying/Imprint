using System;
using System.ComponentModel;

namespace WestLakeShape.Motion
{
    public abstract class BaseState
    {
        private readonly string _name;
        private bool _value;
        private bool _dirty;

       

        public string Name => _name;

        public virtual bool Dirty
        {
            get => _dirty;
            set => _dirty = value;
        }

        public BaseState(string name)
        {
            _name = name;
        }

        public abstract bool ReadOnly { get; }


        public virtual bool Get()
        {
            return _value;
        }


        public virtual void Set(bool value)
        {
            if (ReadOnly)
                throw new InvalidOperationException("只读状态不能写入：" + Name);

            _value = value;
            Dirty = true;
        }
      


        protected void Refresh(bool value)
        {
            if (_dirty) return;

            _value = value;
        }


        public void HasChanged()
        {
            _dirty = false;
        }

        public override string ToString()
        {
            return $"{_name}:{_value}";
        }
    }


    public enum IOActiveLevel
    {
        Lower,
        High
    }


    public enum IOType
    {
        Input,
        Output,
    }

}
