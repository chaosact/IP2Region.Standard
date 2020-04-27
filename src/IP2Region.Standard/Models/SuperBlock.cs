namespace IP2Region.Models
{
    internal struct SuperBlock
    {
        /// <summary>
        /// 指向INDEX起始位置的index block
        /// </summary>
        internal int FirstIndexPtr;

        /// <summary>
        /// 指向最后一个index block的地址
        /// </summary>
        internal int LastIndexPtr;
    }
}
