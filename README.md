# IP2Region.Standard
IP2Region.Standard

1.使用Unsafe类优化底层，不需要勾选“允许不安全代码”。单IP检索速度为100+ns，1000W次检索，耗时在4.5秒左右。

2.修正某些IP在Btree模式下搜索的结果为乱码的问题。

3.不再使用DbSearcher对象，使用MemorySearcher和FileSearcher对象进行IP检索。

# 安装方式

  1.使用NUGET：使用Install-Package IP2Region.Standard。
  
  2.git clone https://github.com/chaosact/IP2Region.Standard.git ，然后在项目中引用

# 使用方式

  内存检索：MemorySearcher searcher = new MemorySearcher("数据库路径");
  
  文件检索：FileSearcher searcher = new FileSearcher("数据库路径");
  
  Btree IP字符串检索：searcher.BtreeSearch("168.56.58.91");
  
  Btree 网络字节序检索：searcher.BtreeSearch(65155812);
  
  Binary IP字符串检索：searcher.BinarySearch("168.56.58.91");
  
  Binary 网络字节序检索：searcher.BinarySearch(65155812);
  
# 性能测试

|             Method |         ip |         Mean |       Error |    StdDev | 
|------------------- |----------- |-------------:|------------:|----------:|
| MemoryBtreeSeacher | 2312435453 |     119.5 ns |     0.96 ns |   0.90 ns |
| MemoryBinarySearch | 2312435453 |     119.6 ns |     1.50 ns |   1.40 ns | 
|    FileBtreeSearch | 2312435453 |  12,065.4 ns |   126.70 ns |  98.92 ns | 
|   FileBinarySearch | 2312435453 | 100,248.0 ns |   930.73 ns | 726.65 ns | 

单次查询100+ns,1000W次查询4.5s(±0.15s)，对比原作者C#版本的测试数据，速度快100倍以上。

原作者C#项目测试数据 

https://github.com/lionsoul2014/ip2region/tree/master/binding/c%23#test-resultfrom-ip2regiontestbenchmark



# 参考源项目
  
  作者：lionsoul2014
  
  git url：https://github.com/lionsoul2014/ip2region
  
  C#版本：https://github.com/lionsoul2014/ip2region/tree/master/binding/c%23

