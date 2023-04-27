﻿using AdminAPI.Filters;
using AdminAPI.Libraries;
using AdminShared.Models;
using AdminShared.Models.Link;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Database;

namespace AdminAPI.Controllers
{
    [SignVerifyFilter]
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class LinkController : ControllerBase
    {

        private readonly DatabaseContext db;
        private readonly IDHelper idHelper;

        private readonly long userId;



        public LinkController(DatabaseContext db, IDHelper idHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            this.idHelper = idHelper;

            var userIdStr = httpContextAccessor.HttpContext?.GetClaimByAuthorization("userId");
            if (userIdStr != null)
            {
                userId = long.Parse(userIdStr);
            }
        }



        /// <summary>
        /// 获取友情链接列表
        /// </summary>
        /// <param name="pageNum">页码</param>
        /// <param name="pageSize">单页数量</param>
        /// <param name="searchKey">搜索关键词</param>
        /// <returns></returns>
        [HttpGet("GetLinkList")]
        public DtoPageList<DtoLink> GetLinkList(int pageNum, int pageSize, string? searchKey)
        {
            DtoPageList<DtoLink> data = new();

            int skip = (pageNum - 1) * pageSize;

            var query = db.TLink.AsQueryable();

            if (!string.IsNullOrEmpty(searchKey))
            {
                query = query.Where(t => t.Name.Contains(searchKey));
            }

            data.Total = query.Count();

            data.List = query.OrderByDescending(t => t.CreateTime).Select(t => new DtoLink
            {
                Id = t.Id,
                Name = t.Name,
                Url = t.Url,
                Sort = t.Sort,
                CreateTime = t.CreateTime
            }).Skip(skip).Take(pageSize).ToList();

            return data;
        }



        /// <summary>
        /// 获取友情链接
        /// </summary>
        /// <param name="linkId">链接ID</param>
        /// <returns></returns>
        [HttpGet("GetLink")]
        public DtoLink? GetLink(long linkId)
        {
            var link = db.TLink.Where(t => t.Id == linkId).Select(t => new DtoLink
            {
                Id = t.Id,
                Name = t.Name,
                Url = t.Url,
                Sort = t.Sort,
                CreateTime = t.CreateTime
            }).FirstOrDefault();

            return link;
        }




        /// <summary>
        /// 创建友情链接
        /// </summary>
        /// <param name="createLink"></param>
        /// <returns></returns>
        [HttpPost("CreateLink")]
        public long CreateLink(DtoEditLink createLink)
        {
            TLink link = new()
            {
                Id = idHelper.GetId(),
                Name = createLink.Name,
                Url = createLink.Url,
                CreateTime = DateTime.UtcNow,
                CreateUserId = userId,

                Sort = createLink.Sort
            };

            db.TLink.Add(link);

            db.SaveChanges();

            return link.Id;
        }




        /// <summary>
        /// 更新友情链接
        /// </summary>
        /// <param name="linkId"></param>
        /// <param name="updateLink"></param>
        /// <returns></returns>
        [HttpPost("UpdateLink")]
        public bool UpdateLink(long linkId, DtoEditLink updateLink)
        {
            var link = db.TLink.Where(t => t.Id == linkId).FirstOrDefault();

            if (link != null)
            {
                link.Name = updateLink.Name;
                link.Url = updateLink.Url;
                link.Sort = updateLink.Sort;

                db.SaveChanges();
            }

            return true;
        }



        /// <summary>
        /// 删除友情链接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteLink")]
        public bool DeleteLink(long id)
        {
            var link = db.TLink.Where(t => t.Id == id).FirstOrDefault();

            if (link != null)
            {
                link.IsDelete = true;
                link.DeleteTime = DateTime.UtcNow;
                link.DeleteUserId = userId;

                db.SaveChanges();

                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
