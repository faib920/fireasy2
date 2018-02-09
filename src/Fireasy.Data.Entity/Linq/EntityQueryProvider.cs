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
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 为实体提供 LINQ 查询的支持。无法继承此类。
    /// </summary>
    public sealed class EntityQueryProvider : IEntityQueryProvider,
        IEntityPersistentEnvironment,
        IEntityPersistentInstanceContainer
    {
        private string instanceName;
        private EntityPersistentEnvironment environment;
        private InternalContext context;

        /// <summary>
        /// 使用一个 <see cref="IDatabase"/> 对象初始化 <see cref="EntityQueryProvider"/> 类的新实例。
        /// </summary>
        /// <param name="context">一个 <see cref="InternalContext"/> 对象。</param>
        internal EntityQueryProvider(InternalContext context)
        {
            Guard.ArgumentNull(context, nameof(context));

            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public EntityQueryProvider(IDatabase database)
            : this(new InternalContext(database))
        {
        }

        /// <summary>
        /// 获取或设置持久化环境。
        /// </summary>
        EntityPersistentEnvironment IEntityPersistentEnvironment.Environment
        {
            get { return environment; }
            set { environment = value; }
        }

        /// <summary>
        /// 获取或设置实例名称。
        /// </summary>
        string IEntityPersistentInstanceContainer.InstanceName
        {
            get { return instanceName; }
            set { instanceName = value; }
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

            if (efn.Method.GetParameters().Length == 2)
            {
                return efn.DynamicInvoke(context.Database);
            }

            var segment = SegmentFinder.Find(expression);

            return efn.DynamicInvoke(context.Database, segment);
        }

        /// <summary>
        /// 获取表达式的执行计划。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public Expression GetExecutionPlan(Expression expression, TranslateOptions option = null)
        {
            try
            {
                LambdaExpression lambda = expression as LambdaExpression;
                if (lambda != null)
                {
                    expression = lambda.Body;
                }

                var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
                var package = GetTranslateProvider();
                var options = option ?? (section != null ? section.Options : TranslateOptions.Default);

                using (var scope = new TranslateScope(context, package))
                {
                    var translation = package.Translate(expression, options);
                    return package.BuildExecutionPlan(translation, options);
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
        /// <param name="option">指定解析的选项。</param>
        /// <exception cref="TranslateException">对 LINQ 表达式解析失败时抛出此异常。</exception>
        public TranslateResult Translate(Expression expression, TranslateOptions option = null)
        {
            try
            {
                LambdaExpression lambda = expression as LambdaExpression;
                if (lambda != null)
                {
                    expression = lambda.Body;
                }

                var section = ConfigurationUnity.GetSection<TranslatorConfigurationSection>();
                var package = GetTranslateProvider();
                var options = option ?? (section != null ? section.Options : TranslateOptions.Default);

                using (var scope = new TranslateScope(context, package))
                {
                    var translation = package.Translate(expression, options);
                    var translator = package.CreateTranslator();
                    translator.Syntax = context.Database.Provider.GetService<ISyntaxProvider>();
                    translator.Environment = environment;
                    translator.Options = options;

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

        private ITranslateProvider GetTranslateProvider()
        {
            var provider = context.Database.Provider;
            var package = provider.GetService<ITranslateProvider>();
            if (package != null)
            {
                return package;
            }

            var serviceType = typeof(ITranslateProvider);
            switch (provider.GetType().Name)
            {
                case "MsSqlProvider":
                    provider.RegisterService(serviceType, new MsSqlTranslateProvider());
                    break;
                case "OracleProvider":
                case "OracleAccessProvider":
                    provider.RegisterService(serviceType, new OracleTranslateProvider());
                    break;
                case "MySqlProvider":
                    provider.RegisterService(serviceType, new MySqlTranslateProvider());
                    break;
                case "SQLiteProvider":
                    provider.RegisterService(serviceType, new SQLiteTranslateProvider());
                    break;
                case "OleDbProvider":
                    provider.RegisterService(serviceType, new AccessTranslateProvider());
                    break;
                case "PostgreSqlProvider":
                    provider.RegisterService(serviceType, new PostgreSqlTranslateProvider());
                    break;
                case "FirebirdProvider":
                    provider.RegisterService(serviceType, new FirebirdTranslateProvider());
                    break;
                default:
                    throw new Exception(SR.GetString(SRKind.TranslatorNotSupported, provider.GetType().Name));
            }

            package = provider.GetService<ITranslateProvider>();
            return package;
        }

        /// <summary>
        /// <see cref="IDataSegment"/> 查找器。
        /// </summary>
        private class SegmentFinder : Common.Linq.Expressions.ExpressionVisitor
        {
            private IDataSegment dataSegment;

            /// <summary>
            /// <see cref="IDataSegment"/> 查找器。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static IDataSegment Find(Expression expression)
            {
                var replaer = new SegmentFinder();
                replaer.Visit(expression);
                return replaer.dataSegment;
            }

            protected override Expression VisitConstant(ConstantExpression constExp)
            {
                if (constExp.Value is IDataSegment)
                {
                    dataSegment = constExp.Value as IDataSegment;
                }

                return constExp;
            }
        }

    }
}
