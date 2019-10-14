// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

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
    }
}
