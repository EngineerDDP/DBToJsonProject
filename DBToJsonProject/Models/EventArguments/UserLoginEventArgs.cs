using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 用于传递用户登录输入信息的消息参数类
    /// </summary>
    public class UserLoginEventArgs : EventArgs
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public String Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public String Password { get; set; }
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public Boolean RememberPassword { get; set; }
        /// <summary>
        /// 下次自动登录？
        /// </summary>
        public Boolean AutomaticLogin { get; set; }
    }
}
