// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Data.Syntax;
using Fireasy.Common.Configuration;
using Fireasy.Data.Configuration;

namespace Fireasy.Data
{
    /// <summary>
    /// 实用类。
    /// </summary>
    public sealed class DbUtility
    {
        /// <summary>
        /// 解析字符串中的带目录的参数值。
        /// </summary>
        /// <param name="urlExpression">数据库文件名表达式。</param>
        /// <example>如 |datadirectory|..\data\db1.mdf &lt;br&gt; |system|db1.mdb 等等。</example>
        public static void ParseDataDirectory(ref string urlExpression)
        {
            int dirIndex;
            if ((dirIndex = urlExpression.LastIndexOf("|")) != -1)
            {
                string directory;
                var file = urlExpression.Substring(dirIndex + 1);
                if (urlExpression.IndexOf("|datadirectory|", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    directory = AppDomain.CurrentDomain.BaseDirectory;
                }
                else
                {
                    var folderName = urlExpression.Substring(1, dirIndex - 1);
#if NET35
                    var folder = Environment.SpecialFolder.System;
                    try
                    {
                        folder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), folderName);
                    }
                    catch { }
#else
                    Environment.SpecialFolder folder;
                    Enum.TryParse(folderName, out folder);
#endif
                    directory = Environment.GetFolderPath(folder);
                }

                if (!directory.EndsWith("\\"))
                {
                    directory += "\\";
                }

                urlExpression = new Uri(new Uri(directory), file).LocalPath;
            }
        }

        /// <summary>
        /// 使用引号标识符格式化表名称或列名称。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatByQuote(ISyntaxProvider syntax, string name)
        {
            if (syntax == null)
            {
                return name;
            }

            var section = ConfigurationUnity.GetSection<GlobalConfigurationSection>();

            if ((section == null || 
                (!section.Options.AttachQuote && name.IndexOf(' ') == -1) || name.Length == 0 || syntax.Quote == null || syntax.Quote.Length != 2))
            {
                return name;
            }

            if (name.Length > 1 && 
                (name[0].ToString() == syntax.Quote[0] || name[name.Length - 1].ToString() == syntax.Quote[1]))
            {
                return name;
            }

            return string.Format("{0}{1}{2}", syntax.Quote[0], name, syntax.Quote[1]);
        }
    }
}
