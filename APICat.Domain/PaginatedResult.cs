using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Domain
{
    public class PaginatedResult<TItem>
    {
        public IEnumerable<TItem> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => PageSize > 0
                                ? (int)Math.Ceiling(TotalCount / (double)PageSize)
                                : 0;
        public PaginatedResult(IEnumerable<TItem> items, int totalCount, int pageSize, int currentPage)
        {
            Items = items;
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
        }
    }
}
