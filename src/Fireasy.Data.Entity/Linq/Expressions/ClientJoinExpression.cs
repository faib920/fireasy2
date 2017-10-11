// <copyright company="Faib Studio"
//      email="faib920@126.com"
//      qq="55570729"
//      date="2011-3-7">
//   (c) Copyright Faib Studio 2011. All rights reserved.
// </copyright>
// ---------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 一个表达式，表示客户端的连接查询。
    /// </summary>
    public class ClientJoinExpression : DbExpression
    {
        /// <summary>
        /// 初始化 
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="outerKey"></param>
        /// <param name="innerKey"></param>
        public ClientJoinExpression(ProjectionExpression projection, IEnumerable<Expression> outerKey, IEnumerable<Expression> innerKey)
            : base(DbExpressionType.ClientJoin, projection.Type)
        {
            OuterKey = outerKey.ToReadOnly();
            InnerKey = innerKey.ToReadOnly();
            Projection = projection;
        }

        public ReadOnlyCollection<Expression> OuterKey
        {
            get;
            private set;
        }

        public ReadOnlyCollection<Expression> InnerKey
        {
            get;
            private set;
        }

        public ProjectionExpression Projection
        {
            get;
            private set;
        }

        /// <summary>
        /// 更新 <see cref="ClientJoinExpression"/> 对象。
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="innerKey"></param>
        /// <param name="outerKey"></param>
        /// <returns></returns>
        public ClientJoinExpression Update(ProjectionExpression projection, IEnumerable<Expression> outerKey, IEnumerable<Expression> innerKey)
        {
            if (projection != Projection || outerKey != OuterKey || innerKey != InnerKey)
            {
                return new ClientJoinExpression(projection, outerKey, innerKey);
            }
            return this;
        }

    }
}
