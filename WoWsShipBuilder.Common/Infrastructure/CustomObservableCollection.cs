using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WoWsShipBuilder.Core;

public class CustomObservableCollection<T> : ObservableCollection<T>
{
    public CustomObservableCollection()
    {
    }

    public CustomObservableCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    public CustomObservableCollection(List<T> list)
        : base(list)
    {
    }

    public void AddRange(IEnumerable<T> items)
    {
        List<T> itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return;
        }

        if (itemList.Count > 1)
        {
            foreach (var item in itemList.SkipLast(1))
            {
                Items.Add(item);
            }
        }

        Add(itemList.Last());
    }

    public void RemoveRange(IEnumerable<T> items)
    {
        List<T> itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return;
        }

        if (itemList.Count > 1)
        {
            foreach (var item in itemList.SkipLast(1))
            {
                Items.Remove(item);
            }
        }

        Remove(itemList.Last());
    }

    public int FindIndex(Predicate<T> match)
    {
        int endIndex = Count;
        for (var i = 0; i < endIndex; i++)
        {
            if (match(this[i]))
            {
                return i;
            }
        }

        return -1;
    }

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        NotifyCountChanged();
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        NotifyCountChanged();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        NotifyCountChanged();
    }

    private void NotifyCountChanged()
    {
        base.OnPropertyChanged(new(nameof(CustomObservableCollection<object>.Count)));
    }
}
