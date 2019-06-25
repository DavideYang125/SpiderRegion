using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderRegion
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始运行");
            RegionDataSpider.Run();
            Console.ReadKey();
        }
    }
}
