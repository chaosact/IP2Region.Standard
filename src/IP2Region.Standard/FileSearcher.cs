using IP2Region.Models;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IP2Region
{
    /// <summary>
    /// 使用文件流的形式进行IP检索
    /// </summary>
    public class FileSearcher : IDisposable
    {
        /// <summary>
        ///  文件流
        /// </summary>
        private readonly Stream _Stream;

        /// <summary>
        /// 头部索引区块缓存
        /// </summary>
        private readonly byte[] _HeaderBuffer;

        /// <summary>
        /// IP数据块索引描述
        /// </summary>
        private ref SuperBlock SuperBlock => ref Unsafe.As<byte, SuperBlock>(ref _HeaderBuffer[0]);

        /// <summary>
        /// HaderIndex总数量
        /// </summary>
        private readonly int _HeaderMaxIndex;

        /// <summary>
        /// IndexBlock总数量
        /// </summary>
        private readonly int _IndexBlockCcount;

        /// <summary>
        /// 使用文件流的形式进行IP检索
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        public FileSearcher(string dbPath)
        {
            _Stream = new FileStream(dbPath, FileMode.Open, FileAccess.Read);

            _HeaderBuffer = new byte[BlockSize.SuperBlockSize + 8192];
            _Stream.Read(_HeaderBuffer, 0, _HeaderBuffer.Length);

            ref SuperBlock superBlock = ref SuperBlock;
            _IndexBlockCcount = (superBlock.LastIndexPtr - superBlock.FirstIndexPtr) / BlockSize.IndexBlockSize + 1;

            int maxIndex = 0;
            ref HeaderBlock headerCurrent = ref Unsafe.As<byte, HeaderBlock>(ref _HeaderBuffer[8]);
            do
            {
                if (headerCurrent.IPIndexPtr == 0)
                {
                    break;
                }

                maxIndex++;
                if (maxIndex < 1024)
                {
                    headerCurrent = ref Unsafe.Add(ref headerCurrent, 1);
                }
                else
                {
                    break;
                }

            } while (true);
            _HeaderMaxIndex = maxIndex - 1;
        }

        /// <summary>
        /// 使用Btree进行IP检索
        /// </summary>
        /// <param name="ipStr">IP字符串</param>
        /// <returns></returns>
        public DataBlock BtreeSearch(string ipStr)
        {
            return BtreeSearch(Utils.String2NetworkIP(ipStr));
        }

        /// <summary>
        /// 使用Btree进行IP检索
        /// </summary>
        /// <param name="ipStr">网络字节序</param>
        /// <returns></returns>
        public DataBlock BtreeSearch(uint networkIP)
        {
            int index = _HeaderMaxIndex / 2, lower = 0, heighter = _HeaderMaxIndex;

            ref HeaderBlock headerStart = ref Unsafe.As<byte, HeaderBlock>(ref _HeaderBuffer[BlockSize.SuperBlockSize]);

            ref HeaderBlock headerCurrent = ref Unsafe.Add(ref headerStart, index);

            //二分法检索ipVal在HeaderIndex的命中的二级索引
            do
            {
                //Console.WriteLine("headerIndex:"+index);
                //IP与HeaderIndex的StartIP相同，直接命中
                if (headerCurrent.StartIP == networkIP)
                {
                    break;
                }

                //HeaderIndex的StartIP小于目标IP
                if (headerCurrent.StartIP < networkIP)
                {
                    //如果下一个HeaderIndex的StartIP大于等于目标IP，或者当前已是最后一个视为命中
                    //example 1: ipValue = 100.23.1.23，[100].StartIP = 100.0.0.0，[101].StartIP = 121.0.0.0 target!
                    //example 1: ipValue = 254.23.1.23，[1023].StartIP = 254.0.0.0，1023是最后一个索引 target!
                    if (index == _HeaderMaxIndex || (Unsafe.Add(ref headerCurrent, 1)).StartIP >= networkIP)
                    {
                        break;
                    }
                    lower = index + 1;
                }
                //HeaderIndex的StartIP大于目标IP
                else
                {
                    if (index == 0)
                    {
                        break;
                    }
                    heighter = index - 1;
                }

                index = (lower + heighter) >> 1;
                headerCurrent = ref Unsafe.Add(ref headerStart, index);
            } while (true);


            //计算2个HeaderIndex IPIndexPtr之间的IndexBlock的数量
            int partLen = index < _HeaderMaxIndex
                ? Unsafe.Add(ref headerCurrent, 1).IPIndexPtr - headerCurrent.IPIndexPtr
                : BlockSize.IndexBlockSize;
            int partCount = partLen / BlockSize.IndexBlockSize;

            index = partCount >> 1;

            heighter = partCount;

            lower = 0;

            //一次性加载整个HeaderIndex指向的IndexBlock块
            byte[] buffer = new byte[partLen];

            _Stream.Seek(headerCurrent.IPIndexPtr, SeekOrigin.Begin);
            _Stream.Read(buffer, 0, buffer.Length);

            ref IndexBlock blockStart = ref Unsafe.As<byte, IndexBlock>(ref buffer[0]);
            ref IndexBlock blockCurrent = ref Unsafe.Add(ref blockStart, index);

            //二分法检索ipVal在HeaderIndex的命中的二级索引
            do
            {

                //IP与IndexBlock的StartIP和EndIp区间，直接命中
                if (blockCurrent.StartIP <= networkIP && blockCurrent.EndIP >= networkIP)
                {
                    break;
                }

                //IndexBlock的EndIp小于目标IP
                if (blockCurrent.StartIP < networkIP)
                {
                    lower = index + 1;
                }
                //HeaderIndex的StartIP大于目标IP
                else
                {
                    heighter = index - 1;
                }

                index = (lower + heighter) >> 1;

                blockCurrent = ref Unsafe.Add(ref blockStart, index);

            } while (true);

            buffer = new byte[blockCurrent.DataLen];

            int ptr = blockCurrent.DataPtr;

            _Stream.Seek(ptr, SeekOrigin.Begin);

            _Stream.Read(buffer, 0, buffer.Length);

            int cityId = Unsafe.As<byte, int>(ref buffer[0]);

            string region = Encoding.UTF8.GetString(buffer, 4, buffer.Length - 4);

            return new DataBlock(cityId, region, ptr);
        }

        /// <summary>
        /// 使用Binary进行IP检索
        /// </summary>
        /// <param name="ipStr">IP字符串</param>
        /// <returns></returns>
        public DataBlock BinarySearch(string ipStr)
        {
            return BinarySearch(Utils.String2NetworkIP(ipStr));
        }

        /// <summary>
        /// 使用Binary进行IP检索
        /// </summary>
        /// <param name="networkIP">网络字节序</param>
        /// <returns></returns>
        public DataBlock BinarySearch(uint networkIP)
        {
            int index = _IndexBlockCcount >> 1,
                lower = 0,
                heighter = _IndexBlockCcount,
                ptr = SuperBlock.FirstIndexPtr,
                indexLen = BlockSize.IndexBlockSize;


            //一次性加载整个HeaderIndex指向的IndexBlock块
            byte[] buffer = new byte[indexLen];

            _Stream.Seek(ptr + index * indexLen, SeekOrigin.Begin);

            _Stream.Read(buffer, 0, buffer.Length);

            ref IndexBlock blockCurrent = ref Unsafe.As<byte, IndexBlock>(ref buffer[0]);

            //二分法检索ipVal在HeaderIndex的命中的二级索引
            do
            {
                //Console.WriteLine("File BinarySearch" + index);
                //IP与IndexBlock的StartIP和EndIp区间，直接命中
                if (blockCurrent.StartIP <= networkIP && blockCurrent.EndIP >= networkIP)
                {
                    break;
                }

                //IndexBlock的EndIp小于目标IP
                if (blockCurrent.StartIP < networkIP)
                {
                    lower = index + 1;
                }
                //HeaderIndex的StartIP大于目标IP
                else
                {
                    heighter = index - 1;
                }

                index = (lower + heighter) >> 1;

                _Stream.Seek(ptr + index * indexLen, SeekOrigin.Begin);

                _Stream.Read(buffer, 0, buffer.Length);

                blockCurrent = ref Unsafe.As<byte, IndexBlock>(ref buffer[0]);

            } while (true);

            buffer = new byte[blockCurrent.DataLen];

            ptr = blockCurrent.DataPtr;

            _Stream.Seek(ptr, SeekOrigin.Begin);

            _Stream.Read(buffer, 0, buffer.Length);

            int cityId = Unsafe.As<byte, int>(ref buffer[0]);

            string region = Encoding.UTF8.GetString(buffer, 4, buffer.Length - 4);

            return new DataBlock(cityId, region, ptr);
        }

        public void Dispose()
        {
            _Stream.Dispose();
        }
    }
}
