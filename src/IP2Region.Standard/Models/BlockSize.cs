using System.Runtime.CompilerServices;

namespace IP2Region.Models
{
    internal class BlockSize
    {
        internal readonly static int SuperBlockSize = Unsafe.SizeOf<HeaderBlock>();

        internal readonly static int HeaderBlockSize = Unsafe.SizeOf<HeaderBlock>();

        internal readonly static int IndexBlockSize = Unsafe.SizeOf<IndexBlock>();
    }
}
