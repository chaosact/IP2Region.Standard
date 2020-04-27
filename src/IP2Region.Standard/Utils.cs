using IP2Region.Models;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IP2Region
{
    public static class Utils
    {
        public static void Write<T>(byte[] b, int offset, T value)
            where T : unmanaged
        {
            ref byte buffer = ref Unsafe.As<T, byte>(ref value);
            Marshal.Copy((IntPtr)buffer, b, offset, Unsafe.SizeOf<T>());
        }

        /// <summary>
        /// Get a int from a byte array start from the specifiled offset.
        /// </summary>
        public static uint GetUint(byte[] b, int offset)
        {
            return Unsafe.As<byte, uint>(ref b[offset]);
        }

        public static int GetInt(byte[] b, int offset)
        {
            return Unsafe.As<byte, int>(ref b[offset]);
        }

        /// <summary>
        /// Get a int from a byte array start from the specifield offset.
        /// </summary>
        public static int GetInt24(byte[] b, int offset)
        {
            return Unsafe.As<byte, Int24>(ref b[offset]);
        }


        public static ushort GetUshort(byte[] b, int offset)
        {
            return Unsafe.As<byte, ushort>(ref b[offset]);
        }

        public static byte GetByte(byte[] b, int offset)
        {
            return b[offset];
        }

        /// <summary>
        /// String ip to long ip.
        /// </summary>
        public static uint String2NetworkIP(string ip)
        {
            string[] p = ip.Split('.');

            if (p.Length != 4) throw new IPInValidException();

            uint ipVal = 0;

            ref byte ipRef = ref Unsafe.As<uint, byte>(ref ipVal);

            for (int i = 3; i > -1; i--)
            {
                if (byte.TryParse(p[i], out byte part))
                {
                    ref byte byteRef = ref Unsafe.Add(ref ipRef, i);
                    byteRef = part;
                }
                else
                {
                    throw new IPInValidException();
                }
            }

            return ipVal;
        }

        /// <summary>
        /// Int to ip string.
        /// </summary>
        public static string IP2String(uint ipVal)
        {
            ref byte ipRef = ref Unsafe.As<uint, byte>(ref ipVal);
            StringBuilder ipString = new StringBuilder(15);
            for(int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ipString.Append('.');
                    ipString.Append(Unsafe.Add(ref ipRef, i));
                }
                else
                {
                    ipString.Append(ipRef);
                }
            }
            return ipString.ToString();
        }

    }
}
