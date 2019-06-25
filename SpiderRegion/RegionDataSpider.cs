using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpiderRegion
{
    /// <summary>
    /// http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/index.html
    /// </summary>
    public class RegionDataSpider
    {
        private static string LogDir = @"G:\project\region_info";

        public static void Run()
        {
            var url = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/index.html";
            RegionModel regionModel = WriteProvince();
            var existProvince = regionModel.Provinces.Select(x => x.ProvinceName).ToList();
            var content = NetHandle.GetHtmlContent(url, referer: "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/").Item2;//referer: "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/"
            var logPath = Path.Combine(LogDir, "total_info.json");
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            //var doc =   //htmlWeb.Load(url);
            var proTrNodes = doc.DocumentNode.SelectNodes(@"//tr[@class='provincetr']");
            var provincePreUrl = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/";
            foreach (var singleProTrNodes in proTrNodes)
            {
                var proTdNodes = singleProTrNodes.Descendants("td");
                foreach (var singleProTdNodes in proTdNodes)
                {
                    Province province = new Province();
                    var proANode = singleProTdNodes.Descendants("a").FirstOrDefault();
                    var proHref = proANode.GetAttributeValue("href", "");
                    var proUrl = provincePreUrl + proHref;
                    var provinceName = proANode.InnerText.Trim();
                    if (existProvince.Contains(provinceName)) continue;
                    province.ProvinceName = provinceName;
                    province.Level = 1;

                    var proContent = NetHandle.GetHtmlContent(proUrl).Item2;
                    var proDoc = new HtmlDocument(); //htmlWeb.Load(proUrl);
                    proDoc.LoadHtml(proContent);

                    var cityTrNodes = proDoc.DocumentNode.SelectNodes(@"//tr[@class='citytr']");
                    foreach (var cityTrNode in cityTrNodes)
                    {
                        City city = new City();

                        var cityTdNodes = cityTrNode.Descendants("td");
                        var cityCode = cityTdNodes.FirstOrDefault().InnerText.Trim();
                        var cityName = cityTdNodes.ToArray()[1].InnerText.Trim();
                        if (cityName == "市辖区") cityName = provinceName;

                        Console.WriteLine(cityName);

                        var cityHref = cityTdNodes.FirstOrDefault().Descendants("a").FirstOrDefault().GetAttributeValue("href", "");
                        var cityUrl = provincePreUrl + cityHref;
                        city.CityName = cityName;
                        city.Code = cityCode;
                        city.Level = 2;

                        var cityContent = NetHandle.GetHtmlContent(cityUrl).Item2;
                        if (string.IsNullOrEmpty(cityContent))
                        {
                            Thread.Sleep(2000);
                            cityContent = NetHandle.GetHtmlContent(cityUrl).Item2;

                            if (string.IsNullOrEmpty(cityContent))
                            {
                                Thread.Sleep(2000);
                                cityContent = NetHandle.GetHtmlContent(cityUrl).Item2;
                                if (string.IsNullOrEmpty(cityContent))
                                {
                                    LogHelper.WriteLogs(cityUrl, "请求异常");
                                    continue;
                                }
                            }
                        }

                        var cityDoc = new HtmlDocument(); //htmlWeb.Load(proUrl);
                        cityDoc.LoadHtml(cityContent);

                        //var cityDoc = htmlWeb.Load(cityUrl);

                        var cityUrlExtension = cityUrl.Substring(0, cityUrl.LastIndexOf("."));//.Substring(cityUrl.Length - 2);
                        cityUrlExtension = cityUrlExtension.Substring(cityUrlExtension.Length - 2);
                        var preCityUrl = cityUrl.Substring(0, cityUrl.LastIndexOf("/") + 1);// + "/" + cityUrlExtension;
                        //http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/11/1101.html
                        //http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/11
                        //http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2018/11/01/110101.html
                        var countyTrNodes = cityDoc.DocumentNode.SelectNodes(@"//tr[@class='countytr']");
                        foreach (var countyTrNode in countyTrNodes)
                        {
                            District district = new District();

                            var countyTdNodes = countyTrNode.Descendants("td");
                            var countryCode = countyTdNodes.FirstOrDefault().InnerText.Trim();
                            var countryName = countyTdNodes.LastOrDefault().InnerText.Trim();

                            Console.WriteLine(countryName);

                            var testANode = countyTdNodes.FirstOrDefault().Descendants("a").FirstOrDefault();
                            if (testANode is null) continue;
                            var countryHref = countyTdNodes.FirstOrDefault().Descendants("a").FirstOrDefault().GetAttributeValue("href", "");
                            if (string.IsNullOrEmpty(countryHref) && countryName == "市辖区") continue;
                            var countryUrl = preCityUrl + countryHref;//   01/110101.html
                            district.Code = countryCode;
                            district.DistrictName = countryName;
                            district.Level = 3;


                            var districtContent = NetHandle.GetHtmlContent(countryUrl).Item2;
                            if (string.IsNullOrEmpty(districtContent))
                            {
                                Thread.Sleep(2000);
                                districtContent = NetHandle.GetHtmlContent(countryUrl).Item2;
                                if (string.IsNullOrEmpty(districtContent))
                                {
                                    Thread.Sleep(2000);
                                    districtContent = NetHandle.GetHtmlContent(countryUrl).Item2;
                                    if (string.IsNullOrEmpty(districtContent))
                                    {
                                        LogHelper.WriteLogs(countryUrl, "请求异常");
                                        continue;
                                    }
                                }
                            }

                            var districtDoc = new HtmlDocument(); //htmlWeb.Load(proUrl);
                            districtDoc.LoadHtml(districtContent);

                            //var districtDoc = htmlWeb.Load(countryUrl);
                            var townTrNodes = districtDoc.DocumentNode.SelectNodes(@"//tr[@class='towntr']");
                            var preTownUrl = countryUrl.Substring(0, countryUrl.LastIndexOf("/") + 1);
                            foreach (var townTrNode in townTrNodes)
                            {
                                Town town = new Town();

                                var townTdNodes = townTrNode.Descendants("td");
                                var townCode = townTdNodes.FirstOrDefault().InnerText.Trim();
                                var townName = townTdNodes.LastOrDefault().InnerText.Trim();

                                Console.WriteLine(provinceName + "--" + cityName + "--" + townName);

                                var townHref = townTdNodes.FirstOrDefault().Descendants("a").FirstOrDefault().GetAttributeValue("href", "");
                                var townUrl = preTownUrl + townHref;

                                town.Code = townCode;
                                town.ShortName = townName;
                                town.Level = 4;
                                //var townDoc = htmlWeb.Load(townUrl);

                                var townContent = NetHandle.GetHtmlContent(townUrl).Item2;
                                if (string.IsNullOrEmpty(townContent))
                                {
                                    Thread.Sleep(2000);
                                    townContent = NetHandle.GetHtmlContent(townUrl).Item2;
                                    if (string.IsNullOrEmpty(townContent))
                                    {
                                        Thread.Sleep(2000);
                                        townContent = NetHandle.GetHtmlContent(townUrl).Item2;
                                        if (string.IsNullOrEmpty(townContent))
                                        {
                                            LogHelper.WriteLogs(townUrl, "请求异常");
                                            continue;
                                        }
                                    }
                                }
                                var townDoc = new HtmlDocument(); //htmlWeb.Load(proUrl);
                                townDoc.LoadHtml(townContent);

                                var villageTrNodes = townDoc.DocumentNode.SelectNodes(@"//tr[@class='villagetr']");
                                foreach (var villageTrNode in villageTrNodes)
                                {
                                    Village village = new Village();

                                    var villageTdNodes = villageTrNode.Descendants("td");
                                    var villageCode = villageTdNodes.FirstOrDefault().InnerText.Trim();
                                    var villageName = villageTdNodes.LastOrDefault().InnerText.Trim();

                                    Console.WriteLine(villageName);
                                    Console.WriteLine(provinceName + "--" + cityName + "--" + townName + "--" + villageName);

                                    village.Code = villageCode;
                                    village.VillageName = villageName;
                                    village.Level = 5;

                                    town.Villages.Add(village);
                                }
                                district.Towns.Add(town);
                            }
                            city.Districts.Add(district);
                        }
                        province.Citys.Add(city);

                        LogHelper.WriteLogs(province.ProvinceName, "finish_province");
                    }

                    regionModel.Provinces.Add(province);
                    File.WriteAllText(logPath, JsonConvert.SerializeObject(regionModel), Encoding.UTF8);
                }
            }


            File.WriteAllText(logPath, JsonConvert.SerializeObject(regionModel), Encoding.UTF8);
            Console.WriteLine("采集完成");
        }

        public static RegionModel WriteProvince()
        {
            var path = @"G:\project\region_info\total_info.json";
            var content = File.ReadAllText(path, Encoding.UTF8);
            var regionModel = new RegionModel();
            if (!string.IsNullOrEmpty(content)) regionModel = JsonConvert.DeserializeObject<RegionModel>(content);
            return regionModel;
        }
    }

    public class RegionModel
    {
        public List<Province> Provinces { get; set; } = new List<Province>();
    }

    /// <summary>
    /// 省/自治区
    /// </summary>
    public class Province
    {
        public string ProvinceName { get; set; }

        public string ShortName { get; set; } = "";

        public int Level { get; set; }

        public List<City> Citys { get; set; } = new List<City>();
    }

    public class City
    {
        public string CityName { get; set; }

        public string ShortName { get; set; } = "";

        public int Level { get; set; }

        public string Code { get; set; }
        public List<District> Districts { get; set; } = new List<District>();
    }
    /// <summary>
    /// 区县
    /// </summary>
    public class District
    {
        public string DistrictName { get; set; }

        public string ShortName { get; set; } = "";

        public int Level { get; set; }
        public string Code { get; set; }

        public List<Town> Towns { get; set; } = new List<Town>();

    }
    /// <summary>
    /// 乡镇/街道办事处
    /// </summary>
    public class Town
    {
        public string Code { get; set; }

        public string ShortName { get; set; } = "";

        public int Level { get; set; }
        public string TownName { get; set; }

        public List<Village> Villages { get; set; } = new List<Village>();
    }

    /// <summary>
    /// 村庄/居委会
    /// </summary>
    public class Village
    {
        public string VillageName { get; set; }

        public string ShortName { get; set; } = "";

        public int Level { get; set; }

        public string Code { get; set; }
    }
}
