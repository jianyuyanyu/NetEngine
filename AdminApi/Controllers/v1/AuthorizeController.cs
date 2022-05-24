﻿using AdminApi.Actions.v1;
using AdminApi.Filters;
using AdminShared.Models;
using AdminShared.Models.v1.Authorize;
using Common;
using Common.DistributedLock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdminApi.Controllers.v1
{


    /// <summary>
    /// 系统访问授权模块
    /// </summary>
    [ApiVersion("1")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {

        private readonly long userId;

        private readonly DatabaseContext db;
        private readonly IDistributedLock distLock;
        private readonly SnowflakeHelper snowflakeHelper;


        public AuthorizeController(DatabaseContext db, IDistributedLock distLock, SnowflakeHelper snowflakeHelper)
        {
            this.db = db;
            this.distLock = distLock;
            this.snowflakeHelper = snowflakeHelper;

            var userIdStr = Libraries.Verify.JWTToken.GetClaims("userId");

            if (userIdStr != null)
            {
                userId = long.Parse(userIdStr);
            }
        }





        /// <summary>
        /// 获取Token认证信息
        /// </summary>
        /// <param name="login">登录信息集合</param>
        /// <returns></returns>
        [HttpPost("GetToken")]
        public string? GetToken(DtoLogin login)
        {
            var userList = db.TUser.Where(t => t.IsDelete == false && (t.Name == login.Name || t.Phone == login.Name || t.Email == login.Name)).Select(t => new { t.Id, t.PassWord }).ToList();

            var user = userList.Where(t => t.PassWord == Convert.ToBase64String(KeyDerivation.Pbkdf2(login.PassWord, Encoding.UTF8.GetBytes(t.Id.ToString()), KeyDerivationPrf.HMACSHA256, 1000, 32))).FirstOrDefault();

            if (user != null)
            {
                return AuthorizeAction.GetTokenByUserId(user.Id);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                HttpContext.Items.Add("errMsg", "用户名或密码错误");

                return default;
            }

        }





        /// <summary>
        /// 获取授权功能列表
        /// </summary>
        /// <param name="sign">模块标记</param>
        /// <returns></returns>
        [SignVerifyFilter]
        [Authorize]
        [CacheDataFilter(TTL = 60, UseToken = true)]
        [HttpGet("GetFunctionList")]
        public List<DtoKeyValue> GetFunctionList(string sign)
        {

            var roleIds = db.TUserRole.AsNoTracking().Where(t => t.IsDelete == false && t.UserId == userId).Select(t => t.RoleId).ToList();

            var kvList = db.TFunctionAuthorize.Where(t => t.IsDelete == false && (roleIds.Contains(t.RoleId!.Value) || t.UserId == userId) && t.Function.Parent!.Sign == sign).Select(t => new DtoKeyValue
            {
                Key = t.Function.Sign,
                Value = t.Function.Name
            }).ToList();

            return kvList;
        }



    }
}
