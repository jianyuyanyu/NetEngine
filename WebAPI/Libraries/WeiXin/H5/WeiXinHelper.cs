﻿using Common;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Libraries.WeiXin.H5.Models;

namespace WebAPI.Libraries.WeiXin.H5
{

    public class WeiXinHelper
    {


        private readonly string appid;

        private readonly string appsecret;



        /// <summary>
        /// 微信H5网页开发帮助类
        /// </summary>
        /// <param name="in_appid">微信公众号APPID</param>
        /// <param name="in_appsecret">微信公众号密钥</param>
        public WeiXinHelper(string in_appid, string in_appsecret)
        {
            appid = in_appid;
            appsecret = in_appsecret;
        }



        /// <summary>
        /// 获取 AccessToken
        /// </summary>
        /// <returns></returns>
        public string? GetAccessToken(IDistributedCache distributedCache, IHttpClientFactory httpClientFactory)
        {

            string key = appid + appsecret + "accesstoken";

            var token = distributedCache.GetString(key);

            if (string.IsNullOrEmpty(token))
            {
                string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + appsecret;

                var returnJson = httpClientFactory.Post(url, "", "form");

                token = JsonHelper.GetValueByKey(returnJson, "access_token");

                if (token != null)
                {
                    distributedCache.SetString(key, token, TimeSpan.FromSeconds(6000));
                }
            }

            return token;
        }



        /// <summary>
        /// 获取 TicketID
        /// </summary>
        /// <returns></returns>
        private string GetTicketID(IDistributedCache distributedCache, IHttpClientFactory httpClientFactory)
        {

            string key = appid + appsecret + "ticketid";

            string? ticketid = distributedCache.GetString(key);

            if (string.IsNullOrEmpty(ticketid))
            {

                string getUrl = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=" + GetAccessToken(distributedCache, httpClientFactory) + "&type=jsapi";

                string returnJson = httpClientFactory.Post(getUrl, "", "form");

                ticketid = JsonHelper.GetValueByKey(returnJson, "ticket");

                if (ticketid != null)
                {
                    distributedCache.SetString(key, ticketid, TimeSpan.FromSeconds(6000));
                }
            }

            if (!string.IsNullOrEmpty(ticketid))
            {
                return ticketid;
            }
            else
            {
                throw new Exception("GetTicketID 失败");
            }

        }



        /// <summary>
        /// 获取 JsSDK 签名信息
        /// </summary>
        /// <param name="distributedCache"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="url">HttpContext.GetUrl()</param>
        /// <returns></returns>
        public DtoWeiXinJsSdkSign GetJsSDKSign(IDistributedCache distributedCache, IHttpClientFactory httpClientFactory, string url)
        {
            DtoWeiXinJsSdkSign sdkSign = new()
            {
                AppId = appid,
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                NonceStr = Guid.NewGuid().ToString().Replace("-", "")
            };

            string jsapi_ticket = GetTicketID(distributedCache, httpClientFactory);
            string strYW = "jsapi_ticket=" + jsapi_ticket + "&noncestr=" + sdkSign.NonceStr + "&timestamp=" + sdkSign.TimeStamp + "&url=" + url;
            byte[] bytes_sha1_in = Encoding.Default.GetBytes(strYW);
            byte[] bytes_sha1_out = SHA1.HashData(bytes_sha1_in);
            string str_sha1_out = Convert.ToHexString(bytes_sha1_out).ToLower();

            sdkSign.Signature = str_sha1_out;

            return sdkSign;
        }

    }

}
