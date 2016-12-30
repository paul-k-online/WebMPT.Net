using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using PagedList;

    
namespace WebMpt
{
    public static class PagedListExtension
    {
        public static IPagedList<TDestination> ToMappedPagedList<TSource, TDestination>(this IPagedList<TSource> list)
        {
            var sourceList = Mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(list);
            IPagedList<TDestination> pagedResult = new StaticPagedList<TDestination>(sourceList, list.GetMetaData());
            return pagedResult;
        }
    }
}
