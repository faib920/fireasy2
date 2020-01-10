// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Data.Entity.Linq.Translators;
using Fireasy.Data.Entity.Linq.Translators.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 为实体提供 LINQ 查询的支持。无法继承此类。
    /// </summary>
    public sealed class EntityQueryProvider : IEntityQueryProvider
    {
        private IContextService service;
        private IDatabase database;

        /// <summary>
        /// 使用一个 <see cref="IDatabase"/> 对象初始化 <see cref="EntityQueryProvider"/> 类的新实例。
        /// </summary>
        /// <param name="service">一个 <see cref="IContextService"/> 对象。</param>
        public EntityQueryProvider(IContextService service)
        {
            Guard.ArgumentNull(service, nameof(service));


            if (service is IDatabaseAware aware)
            {
                database = aware.Database;
            }
            else
            {
                throw new InvalidOperationException(SR.GetString(SRKind.NotFoundDatabaseAware));
            }

            this.service = service;
        }

        /// <summary>
        /// 执行 <see cref="Expression"/> 的查询，返回查询结果。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <returns>单值对象。</returns>
        /// <exception cref="TranslateException">对 LINQ 表达式解析失败时抛出此异常。</exception>
        public object Execute(Expression expression)
        {
            var efn = TranslateCache.TryGetDelegate(expression, () => (LambdaExpression)GetExecutionPlan(expression));

            var attrs = GetMethodAttributes(efn.Method);

            if (!attrs.Item2)
            {
                return efn.DynamicInvoke(database);
            }

            var segment = SegmentFinder.Find(expression);

            return efn.DynamicInvoke(database, segment);
        }

        /// <summary>
        /// 执行 <see cref="Expression"/> 的查询，返回查询结果。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <returns>单值对象。</returns>
        /// <exception cref="TranslateException">对 LINQ 表达式解析失败时抛出此异常。</exception>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        /// <summary>
        /// 执行 <see cref="Expression"/> 的查询，返回查询结果。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <returns>单值对象。</returns>
        /// <exception cref="TranslateException">对 LINQ 表达式解析失败时抛出此异常。</exception>
        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var efn = TranslateCache.TryGetDelegate(expression, () => (LambdaExpression)GetExecutionPlan(expression));

            var attrs = GetMethodAttributes(efn.Method);

            object result;
            if (attrs.Item1)
            {
                if (attrs.Item2)
                {
                    var segment = SegmentFinder.Find(expression);
                    result = efn.DynamicInvoke(database, segment, cancellationToken);
                }
                else
                {
                    result = efn.DynamicInvoke(database, cancellationToken);
                }
            }
            else
            {
                if (attrs.Item2)
                {
                    var segment = SegmentFinder.Find(expression);
                    result = efn.DynamicInvoke(database, segment);
                }
                else
                {
                    result = efn.DynamicInvoke(database);
                }
            }

            return await (Task<TResult>)result;
        }

#if !NETFRAMEWORK && !NETSTANDARD2_0
        public IAsyncEnumerable<TResult> ExecuteEnumerableAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var efn = TranslateCache.TryGetDelegate(expression, () => (LambdaExpression)GetExecutionPlan(expression, true));

            var attrs = GetMethodAttributes(efn.Method);

            object result;
            if (attrs.Item1)
            {
                if (attrs.Item2)
                {
                    var segment = SegmentFinder.Find(expression);
                    result = efn.DynamicInvoke(database, segment, cancellationToken);
                }
                else
                {
                    result = efn.DynamicInvoke(database, cancellationToken);
                }
            }
            else
            {
                if (attrs.Item2)
                {
                    var segment = SegmentFinder.Find(expression);
                    result = efn.DynamicInvoke(database, segment);
                }
                else
                {
                    result = efn.DynamicInvoke(database);
                }
            }

            return (IAsyncEnumerable<TResult>)result;
        }
#endif

        /// <summary>
        /// 获取表达式的执行计划。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Expression GetExecutionPlan(Expression expression, bool? isAsync = null)
        {
            try
            {
                if (expression is LambdaExpression lambda)
                {
                    expression = lambda.Body;
                }

                var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
                var options = GetTranslateOptions();

                using (var scope = new TranslateScope(service, options))
                {
                    var translation = scope.TranslateProvider.Translate(expression);
                    var translator = scope.TranslateProvider.CreateTranslator();

                    var buildOptions = new ExecutionBuilder.BuildOptions { IsAsync = isAsync, IsNoTracking = !options.TraceEntityState };
                    return ExecutionBuilder.Build(translation, e => translator.Translate(e), buildOptions);
                }
            }
            catch (Exception ex)
            {
                throw new TranslateException(expression, ex);
            }
        }

        /// <summary>
        /// 执行表达式的翻译。
        /// </summary>
        /// <param name="expression">表示 LINQ 查询的表达式树。</param>
        /// <returns>一个 <see cref="TranslateResult"/>。</returns>
        /// <param name="options">指定解析的选项。</param>
        /// <exception cref="TranslateException">对 LINQ 表达式解析失败时抛出此异常。</exception>
        public TranslateResult Translate(Expression expression, TranslateOptions options = null)
        {
            try
            {
                if (expression is LambdaExpression lambda)
                {
                    expression = lambda.Body;
                }

                options = options ?? GetTranslateOptions();

                using (var scope = new TranslateScope(service, options))
                {
                    var translation = scope.TranslateProvider.Translate(expression);
                    var translator = scope.TranslateProvider.CreateTranslator();

                    TranslateResult result;
                    var selects = SelectGatherer.Gather(translation).ToList();
                    if (selects.Count > 0)
                    {
                        result = translator.Translate(selects[0]);
                        if (selects.Count > 1)
                        {
                            var list = new List<TranslateResult>();
                            for (var i = 1; i < selects.Count; i++)
                            {
                                list.Add(translator.Translate((selects[i])));
                            }

                            result.NestedResults = list.AsReadOnly();
                        }
                    }
                    else
                    {
                        result = translator.Translate(expression);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new TranslateException(expression, ex);
            }
        }

        private TranslateOptions GetTranslateOptions()
        {
            var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
            return (section != null ? section.Options : Translators.TranslateOptions.Default).Clone();
        }

        /// <summary>
        /// 返回一个元组，Item1 表示是否为异步方法，Item2 表示是否有分页参数。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private Tuple<bool, bool> GetMethodAttributes(MethodInfo method)
        {
            var isAsync = false;
            var isSegment = false;
            foreach (var par in method.GetParameters())
            {
                if (isAsync && isSegment)
                {
                    break;
                }

                if (par.ParameterType == typeof(CancellationToken))
                {
                    isAsync = true;
                }
                else if (typeof(IDataSegment).IsAssignableFrom(par.ParameterType))
                {
                    isSegment = true;
                }
            }

            return Tuple.Create(isAsync, isSegment);
        }
    }
}
