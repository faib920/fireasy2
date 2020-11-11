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
        private readonly AP.IMapper _mapper;

        public ObjectMapper()
        {
            _mapper = MapperUnity.GetMapper();
        }

#if NETSTANDARD
        public ObjectMapper(AP.IMapper mapper)
        {
            _mapper = mapper;
        }
#endif

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return _mapper.Map(source, destination);
        }
    }
}
