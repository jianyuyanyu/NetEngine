﻿using Repository.Database.Bases;

namespace Repository.Database
{

    /// <summary>
    /// 任务编排记录
    /// </summary>
    public class TTask : CUD_User
    {


        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }



        /// <summary>
        /// 任务种类["QueueTask","ScheduleTask"]
        /// </summary>
        public string Category { get; set; }



        /// <summary>
        /// 并发值
        /// </summary>
        public int? Semaphore { get; set; }



        /// <summary>
        /// Cron 表达式
        /// </summary>
        public string? Cron { get; set; }



        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }



        /// <summary>
        /// 备注
        /// </summary>
        public string? Remarks { get; set; }

    }
}
