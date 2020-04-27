# IP2Region.Standard
IP2Region.Standard

1.使用Unsafe类优化底层，不需要勾选“允许不安全代码”。单IP检索速度为100+ns，1000W次检索，耗时在4.5秒左右。

2.修正某些IP在Btree模式下搜索的结果为乱码的问题。

3.不再使用DbSearcher对象，使用MemorySearcher和FileSearcher对象进行IP检索。
