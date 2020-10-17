// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Reflection;
using System.Text;

namespace Fireasy.Common.Extensions
{
    public static class ExceptionExtension
    {
        public static void ForEach(this Exception exception, Action<Exception> action)
        {
            Guard.ArgumentNull(exception, nameof(exception));
            Guard.ArgumentNull(action, nameof(action));

            if (exception is AggregateException aggExp && aggExp.InnerExceptions.Count > 0)
            {
                foreach (var e in aggExp.InnerExceptions)
                {
                    e.ForEach(action);
                }
            }
            else
            {
                var e = exception;
                while (e != null)
                {
                    action(e);
                    e = e.InnerException;
                }
            }

        }

        /// <summary>
        /// 递归方式输出异常的详细信息（调用堆栈）。
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string Output(this Exception exception)
        {
            return GetLogContent(exception);
        }

        private static string GetLogContent(Exception exception)
        {
            var sb = new StringBuilder();
            if (exception != null)
            {
                if (exception is AggregateException aggExp)
                {
                    foreach (var e in aggExp.InnerExceptions)
                    {
                        RecursiveWriteException(sb, e);
                    }
                }
                else if (exception is ReflectionTypeLoadException loadExp)
                {
                    foreach (var e in loadExp.LoaderExceptions)
                    {
                        RecursiveWriteException(sb, e);
                    }
                }
                else
                {
                    RecursiveWriteException(sb, exception);
                }
            }

            return sb.ToString();
        }

        private static void RecursiveWriteException(StringBuilder builder, Exception exception)
        {
            var curExp = exception;
            var ident = 0;
            while (curExp != null)
            {
                var prefix = new string(' ', ident++ * 2);
                builder.AppendLine(string.Concat(prefix, curExp.GetType().Name, " => ", curExp.Message));

                if (curExp.StackTrace != null)
                {
                    builder.AppendLine();
                    builder.AppendLine(string.Concat(prefix, "----Begin StackTrack----"));
                    builder.AppendLine(string.Concat(prefix, curExp.StackTrace));
                    builder.AppendLine(string.Concat(prefix, "----End StackTrack----"));
                }

                curExp = curExp.InnerException;
            }
        }
    }
}
