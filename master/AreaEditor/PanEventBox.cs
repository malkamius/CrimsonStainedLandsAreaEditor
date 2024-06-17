using Gtk;

public class PanEventBox : EventBox 
{

    private bool Panning;
    private bool Panned = false;

    private double StartX, StartY;
    private ScrolledWindow scrollParent;

    public delegate void MouseClickEvent(object sender, ButtonReleaseEventArgs args);
    public event MouseClickEvent? MouseClick;

    public PanEventBox(ScrolledWindow parent) : base() {

        scrollParent = parent;

        this.AddEvents((int)Gdk.EventMask.ButtonPressMask);
        this.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
        this.AddEvents((int)Gdk.EventMask.PointerMotionMask);

        this.ButtonPressEvent += handle_ButtonPress;
        this.ButtonReleaseEvent += handle_ButtonRelease;
        this.MotionNotifyEvent += handle_MotionNotifyEvent;

        parent.Add(this);
        
    }

    private void handle_MotionNotifyEvent(object o, MotionNotifyEventArgs args)
    {
        if(Panning) {
            var deltax = StartX - args.Event.XRoot;
            var deltay = StartY - args.Event.YRoot;
            
            scrollParent.Hadjustment.Value = Math.Max(scrollParent.Hadjustment.Lower, Math.Min(scrollParent.Hadjustment.Upper, scrollParent.Hadjustment.Value + deltax));
            scrollParent.Vadjustment.Value = Math.Max(scrollParent.Vadjustment.Lower, Math.Min(scrollParent.Vadjustment.Upper, scrollParent.Vadjustment.Value + deltay));
            Panned = true;
            StartX = args.Event.XRoot;
            StartY = args.Event.YRoot;
        }
    }

    private void handle_ButtonRelease(object o, ButtonReleaseEventArgs args)
    {
        Panning = false;
        if(!Panned) {
            MouseClick?.Invoke(this, args);
        }
    }
    private void handle_ButtonPress(object o, ButtonPressEventArgs args)
    {
        if(args.Event.Button == 1) {
            Panning = true;
            Panned = false;
            
            StartX = args.Event.XRoot;
            StartY = args.Event.YRoot;
        }
    }
}