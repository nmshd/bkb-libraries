using System;
using System.Linq;
using Enmeshed.BuildingBlocks.Application.Pagination;

namespace Enmeshed.BuildingBlocks.Application.Extensions
{
    public static class IQueryablePaginationExtensions
    {
        public static IQueryable<T> Paged<T>(this IQueryable<T> query, PaginationFilter paginationFilter)
        {
            if (paginationFilter == null) throw new Exception("A pagination filter has to be provided.");

            if (paginationFilter.PageSize != null)
                query = query
                    .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize.Value)
                    .Take(paginationFilter.PageSize.Value);

            return query;
        }
    }
}