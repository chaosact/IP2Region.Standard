using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IP2Region.Models
{
    internal struct Int24
    {
        private byte Byte1;

        private byte Byte2;

        private byte Byte3;

        internal Int24(int value)
        {
            if (value > 0x00FFFFFF)
            {
                throw new ArgumentException("Int24 max value must be smaller than 0x00FFFFFF");
            }
            ref byte ptr = ref Unsafe.As<int, byte>(ref value);
            Byte1 = ptr;
            Byte2 = Unsafe.Add(ref ptr, 1);
            Byte3 = Unsafe.Add(ref ptr, 2);
        }

        internal void SetValue(int value)
        {
            if (value > 0x00FFFFFF)
            {
                throw new ArgumentException("Int24 max value must be smaller than 0x00FFFFFF");
            }
            ref byte ptr = ref Unsafe.As<int, byte>(ref value);
            Byte1 = ptr;
            Byte2 = Unsafe.Add(ref ptr, 1);
            Byte3 = Unsafe.Add(ref ptr, 2);
        }

        internal int GetValue()
        {
            int value = 0;
            ref byte ptr = ref Unsafe.As<int, byte>(ref value);
            ptr = Byte1;
            Unsafe.Add(ref ptr, 1) = Byte2;
            Unsafe.Add(ref ptr, 2) = Byte3;
            return value;
        }

        public static int operator +(Int24 a, Int24 b)
        {
            return a.GetValue() + b.GetValue();
        }

        public static Int24 operator -(Int24 a, Int24 b)
        {
            return new Int24(a.GetValue() - b.GetValue());
        }

        public static int operator *(Int24 a, Int24 b)
        {
            return a.GetValue() * b.GetValue();
        }

        public static int operator /(Int24 a, Int24 b)
        {
            return new Int24(a.GetValue() / b.GetValue());
        }

        public static bool operator ==(int a, Int24 b)
        {
            return a == b.GetValue();
        }

        public static bool operator !=(int a, Int24 b)
        {
            return a == b.GetValue();
        }

        public static implicit operator int (Int24 operand)
        {
            return operand.GetValue();
        }

        public static explicit operator Int24(int operand)
        {
            return new Int24(operand);
        }

        public override bool Equals(object obj)
        {
            int value = (int)obj;
            return value == GetValue();
        }

        public override int GetHashCode()
        {
            return GetValue();
        }
    }
}
