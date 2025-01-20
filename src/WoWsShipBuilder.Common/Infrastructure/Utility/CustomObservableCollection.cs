using System.Collections.ObjectModel;

namespace WoWsShipBuilder.Infrastructure.Utility;

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
                this.Items.Add(item);
            }
        }

        this.Add(itemList[^1]);
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
                this.Items.Remove(item);
            }
        }

        this.Remove(itemList[^1]);
    }

    public int FindIndex(Predicate<T> match)
    {
        int endIndex = this.Count;
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
        this.NotifyCountChanged();
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        this.NotifyCountChanged();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        this.NotifyCountChanged();
    }

    private void NotifyCountChanged()
    {
        base.OnPropertyChanged(new(nameof(CustomObservableCollection<object>.Count)));
    }
}
