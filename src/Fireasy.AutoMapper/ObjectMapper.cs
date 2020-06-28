// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Mapper;
using AP = AutoMapper;

namespace Fireasy.AutoMapper
{
    public class ObjectMapper : IObjectMapper
    {
        private readonly AP.IMapper mapper;

        public ObjectMapper()
            : this (MapperUnity.GetMapper())
        {
        }

        public ObjectMapper(AP.IMapper mapper)
        {
            this.mapper = mapper;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return mapper.Map<TSource, TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return mapper.Map(source, destination);
        }
    }
}
