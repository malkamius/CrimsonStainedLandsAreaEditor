using Gtk;

public class CheckedListBox : ScrolledWindow
{
    private ListStore store;
    private TreeView treeView;
    private const int MaxVisibleItems = 10; // Maximum number of items to show without scrolling
    private const int RowHeight = 30; // Estimated height of each row
    public CheckedListBox()
    {
        store = new ListStore(typeof(bool), typeof(string));
        treeView = new TreeView(store);

        var toggleRenderer = new CellRendererToggle();
        toggleRenderer.Toggled += OnItemToggled;
        var toggleColumn = new TreeViewColumn("", toggleRenderer, "active", 0);
        treeView.AppendColumn(toggleColumn);

        var textRenderer = new CellRendererText();
        var textColumn = new TreeViewColumn("Item", textRenderer, "text", 1);
        treeView.AppendColumn(textColumn);

        treeView.HeadersVisible = false;

        Add(treeView);
        UpdateSizeRequest();

        // Connect to the size-allocate signal to update size when the widget is resized
        SizeAllocated += (o, args) => UpdateSizeRequest();
    }

    public void AddItem(string item, bool isChecked = false)
    {
        store.AppendValues(isChecked, item);
        UpdateSizeRequest();
    }

    public List<string> GetCheckedItems()
    {
        var checkedItems = new List<string>();
        TreeIter iter;
        if (store.GetIterFirst(out iter))
        {
            do
            {
                bool isChecked = (bool)store.GetValue(iter, 0);
                string item = (string)store.GetValue(iter, 1);
                if (isChecked)
                {
                    checkedItems.Add(item);
                }
            } while (store.IterNext(ref iter));
        }
        return checkedItems;
    }

    public void SetItemChecked(string item, bool isChecked = false) {
        TreeIter iter;
        if (store.GetIterFirst(out iter))
        {
            do
            {
                string selectitem = (string)store.GetValue(iter, 1);

                if(selectitem == item)
                store.SetValue(iter, 0, isChecked);
            } while (store.IterNext(ref iter));
        }
    }

    public void ClearItems() {
        store.Clear();
    }

    private void OnItemToggled(object sender, ToggledArgs args)
    {
        TreeIter iter;
        if (store.GetIterFromString(out iter, args.Path))
        {
            bool currentValue = (bool)store.GetValue(iter, 0);
            store.SetValue(iter, 0, !currentValue);
        }
    }

     private void UpdateSizeRequest()
    {
        int itemCount = store.IterNChildren();
        int visibleItems = Math.Min(itemCount, MaxVisibleItems);
        int height = visibleItems * RowHeight;

        // Set the size request for the TreeView
        treeView.SetSizeRequest(-1, height);

        this.SetSizeRequest(-1, height);
        // If there are more items than MaxVisibleItems, show scrollbars
        // if (itemCount > MaxVisibleItems)
        // {
        //     SetPolicy(PolicyType.Never, PolicyType.Automatic);
        // }
        // else
        // {
        //     SetPolicy(PolicyType.Never, PolicyType.Never);
        // }
    }
}