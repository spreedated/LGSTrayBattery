using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;

namespace LGSTrayUI;

public class MainTaskBarIcon : TaskbarIcon
{
    public MainTaskBarIcon() : base()
    {
        ContextMenu = (System.Windows.Controls.ContextMenu) Application.Current.FindResource("SysTrayMenu");
        BatteryIconDrawing.DrawUnknown(this);
    }
}

public class MainTaskbarIconWrapper : IDisposable
{
    #region IDisposable
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _taskbarIcon?.Dispose();
                LogiDeviceIcon.RefCountChanged -= this.OnRefCountChanged;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    private TaskbarIcon? _taskbarIcon = new MainTaskBarIcon();

    public MainTaskbarIconWrapper()
    {
        LogiDeviceIcon.RefCountChanged += OnRefCountChanged;
        OnRefCountChanged(LogiDeviceIcon.RefCount);
    }

    private void OnRefCountChanged(int refCount)
    {
        if (refCount == 0)
        {
            _taskbarIcon ??= new MainTaskBarIcon();
        }
        else
        {
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
