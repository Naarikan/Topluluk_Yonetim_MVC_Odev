using System;
using System.Collections.Generic;
using System.Linq;

namespace Topluluk_Yonetim.MVC.Models.ViewModels;

public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public PagedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    }

    public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var list = source.ToList();
        var totalCount = list.Count;
        var skip = (pageNumber - 1) * pageSize;
        var items = list.Skip(skip).Take(pageSize);
        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }
}

