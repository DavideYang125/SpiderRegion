using System;

namespace SpiderRegion
{
    class Program
    {
        static void Main(string[] args)
        {
            //多线程采集           
            RegionDataSpider.GetSingleProvinces();
            return;
            Console.WriteLine("开始运行");
            RegionDataSpider.Run();
            Console.ReadKey();
        }
    }
}
