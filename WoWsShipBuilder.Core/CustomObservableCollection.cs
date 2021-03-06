using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;

namespace WoWsShipBuilder.Core
{
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
                Items.AddRange(itemList.SkipLast(1));
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
                Items.RemoveMany(itemList.SkipLast(1));
            }

            Remove(itemList.Last());
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
}
