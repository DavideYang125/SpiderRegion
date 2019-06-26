using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpiderRegion
{
    public class DataHandle
    {
        private static List<string> provincesList = new List<string>() {  "新疆维吾尔自治区", "宁夏回族自治区","青海省", "甘肃省", "陕西省", "西藏自治区",
            "云南省", "贵州省", "四川省", "重庆市", "海南省",
        "广西壮族自治区","广东省","湖南省","湖北省","河南省","山东省","江西省","福建省","安徽省","浙江省","江苏省","上海市","北京市"
       ,"天津市","河北省","山西省","内蒙古自治区","辽宁省","吉林省","黑龙江省"};

        /// <summary>
        /// 合并数据
        /// </summary>
        public static void CombinData()
        {
            var path = @"G:\project\region_info\total_info.json";
            var newPath = @"G:\project\region_info\new_total_info.json";
            var content = File.ReadAllText(path, Encoding.UTF8);
            var regionModel = JsonConvert.DeserializeObject<RegionModel>(content);
            var logDir = @"G:\project\region_info\Single_info";
            var files = Directory.GetFiles(logDir);


            foreach (var filePath in files)
            {
                var singleContent = File.ReadAllText(filePath, Encoding.UTF8);
                var singleModel = JsonConvert.DeserializeObject<RegionModel>(singleContent);
                var pName = singleModel.Provinces.FirstOrDefault().ProvinceName;
                Console.WriteLine(pName);
                if (!regionModel.Provinces.Select(x => x.ProvinceName).Contains(singleModel.Provinces.FirstOrDefault().ProvinceName))
                {
                    regionModel.Provinces.Add(singleModel.Provinces.FirstOrDefault());
                }
            }
            File.WriteAllText(newPath, JsonConvert.SerializeObject(regionModel), Encoding.UTF8);
        }

        /// <summary>
        /// 输出csv文件
        /// </summary>
        public static void OutputData()
        {
            var path = @"G:\project\region_info\new_total_info_with_shortname.json";
            var csvPath = @"G:\project\region_info\region_info.csv";
            var content = File.ReadAllText(path, Encoding.UTF8);
            var regionModel = JsonConvert.DeserializeObject<RegionModel>(content);
            List<string> infos = new List<string>();
            foreach (var province in regionModel.Provinces)
            {
                //区划代码 名称  等级 简称
                foreach (var citys in province.Citys)
                {
                    foreach (var District in citys.Districts)
                    {
                        foreach (var town in District.Towns)
                        {
                            var townInfo = town.Code + "," + town.TownName + "," + town.Level.ToString() + "," + town.ShortName;
                            infos.Add(townInfo);
                            foreach (var village in town.Villages)
                            {
                                var villageInfo = village.Code + "," + village.VillageName + "," + village.Level.ToString() + "," + village.ShortName;
                                Console.WriteLine(villageInfo);
                                infos.Add(villageInfo);
                            }
                        }
                    }
                }
            }
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine("代码,名称,等级,简称");
            foreach (var info in infos)
            {
                strBuilder.AppendLine(info);
            }
            File.WriteAllText(csvPath, strBuilder.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 设置简称
        /// </summary>
        public static void SetShortName()
        {
            var path = @"G:\project\region_info\new_total_info.json";
            var newPath = @"G:\project\region_info\new_total_info_with_shortname.json";
            var content = File.ReadAllText(path, Encoding.UTF8);
            var regionModel = JsonConvert.DeserializeObject<RegionModel>(content);
            foreach (var province in regionModel.Provinces)
            {
                province.ShortName = ShortNameHelper.GetCleanProvinceName(province.ProvinceName);
                foreach (var city in province.Citys)
                {
                    city.ShortName = ShortNameHelper.GetCleanCityName(city.CityName);
                    foreach (var district in city.Districts)
                    {
                        district.ShortName = ShortNameHelper.GetCleanDistrictName(district.DistrictName);
                        foreach (var town in district.Towns)
                        {
                            if (string.IsNullOrEmpty(town.TownName)) town.TownName = town.ShortName;
                            town.ShortName = ShortNameHelper.GetCleanTownName(town.TownName);
                            foreach (var village in town.Villages)
                            {
                                Console.WriteLine(village.VillageName);
                                village.ShortName = ShortNameHelper.GetCleanVillageName(village.VillageName);
                            }
                        }
                    }
                }
            }

            File.WriteAllText(newPath, JsonConvert.SerializeObject(regionModel), Encoding.UTF8);
        }
    }
    public class ShortNameHelper
    {
        /// <summary>
        /// 获得清理的省份名称
        /// </summary>
        /// <returns></returns>
        public static string GetCleanProvinceName(string provinceName)
        {
            return provinceName.TrimEnd('省').Replace("壮族自治区", "").Replace("回族自治区", "").Replace("维吾尔自治区", "").Replace("特别行政区", "").Replace("自治区", "");
        }

        /// <summary>
        /// 清理市的名称
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public static string GetCleanCityName(string cityName)
        {
            //只有两个字的话就不再简称
            if (cityName.Trim().Length <= 2) return cityName;
            return cityName.TrimEnd('市');
        }

        /// <summary>
        /// 区县
        /// </summary>
        /// <param name="districtName"></param>
        /// <returns></returns>
        public static string GetCleanDistrictName(string districtName)
        {
            //只有两个字的话就不再简称
            if (districtName.Trim().Length <= 2) return districtName;

            return districtName.TrimEnd('县');
        }

        /// <summary>
        /// 乡镇/街道办事处 简称处理
        /// </summary>
        /// <param name="townName"></param>
        /// <returns></returns>
        public static string GetCleanTownName(string townName)
        {
            //只有两个字的话就不再简称
            if (townName.Trim().Length <= 2) return townName;

            //街道办事处改为街道，镇和乡保留.

            return townName.Replace("街道办事处", "街道");
        }

        /// <summary>
        ///  村庄/居委会 简称处理
        /// </summary>
        /// <param name="villageName"></param>
        /// <returns></returns>
        public static string GetCleanVillageName(string villageName)
        {
            //只有两个字的话就不再简称
            if (villageName.Trim().Length <= 2) return villageName;

            return villageName.Replace("村委会", "村").Replace("社区居委会", "社区");
        }
    }
}
