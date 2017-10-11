// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Fireasy.Data.Entity.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// Result from calling ColumnProjector.ProjectColumns
    /// </summary>
    internal class ProjectedColumns
    {
        public ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDeclaration> columns)
        {
            Projector = projector;
            Columns = columns;
        }

        public Expression Projector
        {
            get;
            private set;
        }

        public ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get;
            private set;
        }
    }
}
