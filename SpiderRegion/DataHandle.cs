using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpiderRegion
{
    public class DataHandle
    {

        public static void OutputData()
        {
            var path = @"G:\project\region_info\total_info.json";
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
            foreach (var info in infos)
            {
                strBuilder.AppendLine(info);
            }
            File.WriteAllText(csvPath, strBuilder.ToString(), Encoding.UTF8);
        }

        public static void SetShortName()
        {
            var path = @"G:\project\region_info\total_info.json";
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
