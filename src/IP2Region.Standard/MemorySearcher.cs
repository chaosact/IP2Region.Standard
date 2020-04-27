using IP2Region.Models;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace IP2Region
{
    /// <summary>
    /// 一次性加载整块内存，然后进行搜索
    /// </summary>
    public class MemorySearcher
    {
        /// <summary>
        /// 数据缓存
        /// </summary>
        private readonly byte[] _Buffer;

        /// <summary>
        /// IP数据块索引描述
        /// </summary>
        private ref SuperBlock SuperBlock => ref Unsafe.As<byte, SuperBlock>(ref _Buffer[0]);

        /// <summary>
        /// HaderIndex总数量
        /// </summary>
        private readonly int _HeaderMaxIndex ;

        /// <summary>
        /// IndexBlock总数量
        /// </summary>
        private readonly int _IndexBlockCcount;



        /// <summary>
        /// 构造MemorySearcher，加载数据至内存中
        /// </summary>
        /// <param name="dbPath"></param>
        public MemorySearcher(string dbPath)
        {
            _Buffer = LoadToMemory(dbPath);
            ref SuperBlock superBlock = ref SuperBlock;
            _IndexBlockCcount = (superBlock.LastIndexPtr - superBlock.FirstIndexPtr) / BlockSize.IndexBlockSize + 1;

            int maxIndex = 0;
            ref HeaderBlock headerCurrent = ref Unsafe.As<byte, HeaderBlock>(ref _Buffer[8]);
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
        /// 加载数据到内存
        /// </summary>
        /// <param name="dbPath"></param>
        /// <returns></returns>
        private byte[] LoadToMemory(string dbPath)
        {
            using (FileStream stream = new FileStream(dbPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
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
            int index, lower = 0, heighter = _HeaderMaxIndex;

            ref HeaderBlock headerStart = ref Unsafe.As<byte, HeaderBlock>(ref _Buffer[BlockSize.SuperBlockSize]);

            ref HeaderBlock headerCurrent = ref headerStart;

            //二分法检索ipVal在HeaderIndex的命中的二级索引
            do
            {
                index = (heighter + lower) >> 1;

                headerCurrent = ref Unsafe.Add(ref headerStart, index);

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
               
            } while (true);

            //计算2个HeaderIndex IPIndexPtr之间的IndexBlock的数量
            int partLen = index < _HeaderMaxIndex
                ? Unsafe.Add(ref headerCurrent, 1).IPIndexPtr - headerCurrent.IPIndexPtr
                : BlockSize.IndexBlockSize;
            int partCount = partLen / BlockSize.IndexBlockSize;

            ref IndexBlock blockStart = ref Unsafe.As<byte, IndexBlock>(ref _Buffer[headerCurrent.IPIndexPtr]);

            return DichotomySearch(networkIP, ref blockStart, partCount);
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
            ref SuperBlock superBlock = ref SuperBlock;
            ref IndexBlock blockStart = ref Unsafe.As<byte, IndexBlock>(ref _Buffer[superBlock.FirstIndexPtr]);
            return DichotomySearch(networkIP, ref blockStart, _IndexBlockCcount);
        }

        /// <summary>
        /// 二分法检索ipVal在HeaderIndex的命中的二级索引
        /// </summary>
        /// <param name="networkIP"></param>
        /// <param name="blockStart"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private DataBlock DichotomySearch(uint networkIP, ref IndexBlock blockStart, int count)
        {
            int index = count >> 1,
                lower = 0,
                heighter = count;

            ref IndexBlock blockCurrent = ref Unsafe.Add(ref blockStart, index);

            do
            {
                //Console.WriteLine("Memory BinarySearch" + index);
                //IP与IndexBlock的StartIP和EndIp区间，直接命中
                if (blockCurrent.StartIP <= networkIP && blockCurrent.EndIP >= networkIP)
                {
                    break;
                }

                //IndexBlock的EndIp小于目标IP
                if (blockCurrent.StartIP < networkIP)
                {
                    //lower = index ;
                    //index += Math.Max((heighter - lower) / 2, 1);
                    lower = index + 1;
                }
                //HeaderIndex的StartIP大于目标IP
                else
                {
                    //heighter = index;
                    //index -= Math.Max((heighter - lower) / 2, 1);
                    count = index - 1;
                }

                index = (lower + count) >> 1;
                blockCurrent = ref Unsafe.Add(ref blockStart, index);

            } while (true);


            int dataPtr = blockCurrent.DataPtr;

            int cityId = Unsafe.As<byte, int>(ref _Buffer[dataPtr]);

            string region = Encoding.UTF8.GetString(_Buffer, blockCurrent.DataPtr + 4, blockCurrent.DataLen - 4);

            return new DataBlock(cityId, region, dataPtr);
        }
    }
}
