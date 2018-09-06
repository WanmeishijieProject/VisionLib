using System.Collections.ObjectModel;
using System.Windows;

namespace JPT_TosaTest.Classes
{
    public class ObservableCollectionThreadSafe<T>: ObservableCollection<T>
    {
        protected override void ClearItems()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.ClearItems();
            });

        }

        protected override void InsertItem(int index, T item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.InsertItem(index, item);
            });
        }
    }
}
