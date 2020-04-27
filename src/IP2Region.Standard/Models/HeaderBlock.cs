namespace IP2Region.Models
{
    internal struct HeaderBlock
    {

        internal uint StartIP;

        internal int IPIndexPtr;

        public HeaderBlock(uint indexStartIp, int indexPtr)
        {
            StartIP = indexStartIp;
            IPIndexPtr = indexPtr;
        }
    }
}