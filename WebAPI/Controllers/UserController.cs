﻿using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Repository.Database;
using WebAPI.Filters;
using WebAPI.Libraries;
using WebAPI.Models.Shared;
using WebAPI.Models.User;

namespace WebAPI.Controllers
{


    /// <summary>
    /// 用户数据操作控制器
    /// </summary>
    [Route("[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {


        private readonly DatabaseContext db;

        private readonly IDistributedCache distributedCache;

        private readonly HttpClient httpClient;


        private readonly long userId;



        public UserController(DatabaseContext db, IDistributedCache distributedCache, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            this.distributedCache = distributedCache;
            httpClient = httpClientFactory.CreateClient();

            var userIdStr = httpContextAccessor.HttpContext?.GetClaimByAuthorization("userId");
            if (userIdStr != null)
            {
                userId = long.Parse(userIdStr);
            }
        }




        /// <summary>
        /// 通过 UserId 获取用户信息 
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpGet]
        [CacheDataFilter(TTL = 60, IsUseToken = true)]
        public DtoUser? GetUser(long? userId)
        {

            userId ??= this.userId;

            var user = db.TUser.Where(t => t.Id == userId).Select(t => new DtoUser
            {
                Name = t.Name,
                UserName = t.UserName,
                Phone = t.Phone,
                Email = t.Email,
                Roles = string.Join(",", db.TUserRole.Where(r => r.UserId == t.Id).Select(r => r.Role.Name).ToList()),
                CreateTime = t.CreateTime
            }).FirstOrDefault();

            return user;
        }



        /// <summary>
        /// 通过短信验证码修改账户手机号
        /// </summary>
        /// <param name="keyValue">key 为新手机号，value 为短信验证码</param>
        /// <returns></returns>
        [HttpPost]
        public bool EditUserPhoneBySms([FromBody] DtoKeyValue keyValue)
        {

            string phone = keyValue.Key!.ToString()!;

            string key = "VerifyPhone_" + phone;

            var code = distributedCache.GetString(key);


            if (string.IsNullOrEmpty(code) == false && code == keyValue.Value!.ToString())
            {

                var checkPhone = db.TUser.Where(t => t.Id != userId && t.Phone == phone).Count();

                var user = db.TUser.Where(t => t.Id == userId).FirstOrDefault();

                if (user != null)
                {
                    if (checkPhone == 0)
                    {
                        user.Phone = phone;

                        db.SaveChanges();

                        return true;
                    }
                    else
                    {
                        HttpContext.SetErrMsg("手机号已被其他账户绑定");

                        return false;
                    }
                }
                else
                {
                    HttpContext.SetErrMsg("账户不存在");

                    return false;
                }
            }
            else
            {
                HttpContext.SetErrMsg("短信验证码错误");

                return false;
            }
        }


    }
}