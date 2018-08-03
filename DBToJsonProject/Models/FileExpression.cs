using DBToJsonProject.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 记录文件信息的模型类
    /// </summary>
    public class FileExpression
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="path">文件路径</param>
        public FileExpression(string fileName, string path)
        {
            FileName = fileName;
            Path = path;
        }
        /// <summary>
        /// 文件名称
        /// </summary>
        public String FileName { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public String Path { get; set; }
        /// <summary>
        /// 从控制台参数隐式转换到前端模型
        /// </summary>
        public static implicit operator FileExpression(FileEventArgs obj)
        {
            return new FileExpression(obj.FileName, obj.FilePath);
        }
    }
}
