﻿using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Repository.Database;
using WebAPI.Models.Shared;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 系统基础方法控制器
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {


        private readonly DatabaseContext db;
        private readonly IDHelper idHelper;
        private readonly IDistributedCache distributedCache;



        public BaseController(DatabaseContext db, IDHelper idHelper, IDistributedCache distributedCache)
        {
            this.db = db;
            this.idHelper = idHelper;
            this.distributedCache = distributedCache;
        }



        /// <summary>
        /// 获取省市级联地址数据
        /// </summary>
        /// <param name="provinceId">省份ID</param>
        /// <param name="cityId">城市ID</param>
        /// <returns></returns>
        /// <remarks>不传递任何参数返回省份数据，传入省份ID返回城市数据，传入城市ID返回区域数据</remarks>
        [HttpGet("GetRegion")]
        public List<DtoKeyValue> GetRegion(int provinceId, int cityId)
        {
            List<DtoKeyValue> list = new();

            if (provinceId == 0 && cityId == 0)
            {
                list = db.TRegionProvince.Select(t => new DtoKeyValue { Key = t.Id, Value = t.Province }).ToList();
            }

            if (provinceId != 0)
            {
                list = db.TRegionCity.Where(t => t.ProvinceId == provinceId).Select(t => new DtoKeyValue { Key = t.Id, Value = t.City }).ToList();
            }

            if (cityId != 0)
            {
                list = db.TRegionArea.Where(t => t.CityId == cityId).Select(t => new DtoKeyValue { Key = t.Id, Value = t.Area }).ToList();
            }

            return list;
        }



        /// <summary>
        /// 获取全部省市级联地址数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRegionAll")]
        public List<DtoKeyValueChild> GetRegionAll()
        {

            var list = db.TRegionProvince.Select(t => new DtoKeyValueChild
            {
                Key = t.Id,
                Value = t.Province,
                ChildList = t.TRegionCity!.Select(c => new DtoKeyValueChild
                {
                    Key = c.Id,
                    Value = c.City,
                    ChildList = c.TRegionArea!.Select(a => new DtoKeyValueChild
                    {
                        Key = a.Id,
                        Value = a.Area
                    }).ToList()
                }).ToList()
            }).ToList();

            return list;
        }



        /// <summary>
        /// 二维码生成
        /// </summary>
        /// <param name="text">数据内容</param>
        /// <returns></returns>
        [HttpGet("GetQrCode")]
        public FileResult GetQrCode(string text)
        {
            var image = ImgHelper.GetQrCode(text);
            return File(image, "image/png");
        }




        /// <summary>
        /// 图像验证码生成
        /// </summary>
        /// <param name="sign">标记</param>
        /// <returns></returns>
        [HttpGet("GetVerifyCode")]
        public FileResult GetVerifyCode(Guid sign)
        {
            var cacheKey = "VerifyCode" + sign.ToString();
            Random random = new();
            string text = random.Next(1000, 9999).ToString();

            var image = ImgHelper.GetVerifyCode(text);

            distributedCache.Set(cacheKey, text, TimeSpan.FromMinutes(5));

            return File(image, "image/png");
        }



        /// <summary>
        /// 获取指定组ID的KV键值对
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet("GetValueList")]
        public List<DtoKeyValue> GetValueList(long groupId)
        {

            var list = db.TAppSetting.Where(t => t.Module == "Dictionary" && t.GroupId == groupId).Select(t => new DtoKeyValue
            {
                Key = t.Key,
                Value = t.Value
            }).ToList();

            return list;
        }



        /// <summary>
        /// 获取一个雪花ID
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSnowflakeId")]
        public long GetSnowflakeId()
        {
            return idHelper.GetId();
        }


    }
}