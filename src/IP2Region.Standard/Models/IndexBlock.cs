namespace IP2Region.Models
{
    /*
     * +------------+-----------+-----------+-----------+
     * | 4bytes     | 4bytes    | 3bytes    |  1bytes  |
     * +------------+-----------+-----------+-----------+
     *  start ip      end ip      data ptr     data len
    */
    internal struct IndexBlock
    {
        public const int LENGTH = 12;

        public uint StartIP;

        public uint EndIP;

        public Int24 DataPtr;

        public byte DataLen;
    }
}