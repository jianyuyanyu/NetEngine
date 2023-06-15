﻿namespace WebAPI.Models.Authorize
{
    public class GetTokenByWeiXinCode
    {

        public string APPId { get; set; }



        /// <summary>
        /// 登录时获取的 code，可通过wx.login获取
        /// </summary>
        public string Code { get; set; }    

    }
}
