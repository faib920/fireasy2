// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Data.Configuration;
using Fireasy.Data.Syntax;
using System;
using System.Text.RegularExpressions;

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
        public static string ResolveFullPath(string urlExpression)
        {
            int dirIndex;
            if ((dirIndex = urlExpression.LastIndexOf("|")) != -1)
            {
                string directory;
                var file = urlExpression.Substring(dirIndex + 1);
                if (urlExpression.IndexOf("|datadirectory|", StringComparison.OrdinalIgnoreCase) != -1 ||
                    urlExpression.IndexOf("|appdir|", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    directory = AppDomain.CurrentDomain.BaseDirectory;
                }
                else
                {
                    var folderName = urlExpression.Substring(1, dirIndex - 1);
                    Environment.SpecialFolder folder;
                    Enum.TryParse(folderName, out folder);
                    directory = Environment.GetFolderPath(folder);
                }

                if (!directory.EndsWith("\\"))
                {
                    directory += "\\";
                }

                var uri = new Uri(new Uri(directory), file);
                if (uri.IsFile)
                {
                    return uri.LocalPath.Replace("file:", string.Empty);
                }
                else
                {
                    return uri.OriginalString;
                }
            }

            return urlExpression;
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

        /// <summary>
        /// 在命令文本中查找 Order By 子句。
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        internal static string FindOrderBy(string commandText)
        {
            var regx = new Regex(@"\border\s*by", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = regx.Match(commandText);
            if (match.Groups.Count > 0 && match.Groups[0].Success)
            {
                var index = match.Groups[match.Groups.Count - 1].Index;
                var start = index;
                var len = commandText.Length;
                var count = 0;
                var finded = false;
                while (index < len - 1)
                {
                    if (commandText[index] == '(')
                    {
                        count++;
                        finded = true;
                    }
                    else if (commandText[index] == ')')
                    {
                        count--;
                        finded = true;
                    }
                    if (finded && count == -1)
                    {
                        break;
                    }

                    index++;
                }

                return commandText.Substring(start, index - start + 1);
            }

            return string.Empty;
        }

        /// <summary>
        /// 在命令文本中查找 Order By 子句。
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        internal static string CullingOrderBy(string commandText)
        {
            var regx = new Regex(@"order\s*by", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match match;
            while ((match = regx.Match(commandText)).Success)
            {
                var index = match.Groups[match.Groups.Count - 1].Index;
                var start = index;
                var len = commandText.Length;
                var count = 0;
                var finded = false;
                while (index < len)
                {
                    if (commandText[index] == '(')
                    {
                        count++;
                        finded = true;
                    }
                    else if (commandText[index] == ')')
                    {
                        count--;
                        finded = true;
                    }
                    if (finded && count == -1)
                    {
                        break;
                    }

                    index++;
                }

                commandText = commandText.Replace(commandText.Substring(start, index - start), "");
            }

            return commandText;
        }

    }
}
