/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;

namespace TeachingDeviceManagement
{
    // Delegate tự định nghĩa: nhận cảnh báo và trả về có chấp nhận hay không.
    public delegate bool AlertFilter(DeviceAlertEventArgs alert);

    public class FilteredAlertSubscriber
    {
        private readonly AlertFilter filter;
        private readonly Action<DeviceAlertEventArgs> action;

        public FilteredAlertSubscriber(AlertFilter filter, Action<DeviceAlertEventArgs> action)
        {
            this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        // Method có đúng chữ ký EventHandler<DeviceAlertEventArgs> để đăng ký vào event.
        public void HandleAlert(object? sender, DeviceAlertEventArgs e)
        {
            if (filter(e))
                action(e);
        }
    }
}