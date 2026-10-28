/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;

namespace TeachingDeviceManagement
{
    public enum DeviceStatus
    {
        Available,
        Borrowed,
        UnderMaintenance
    }

    public abstract class TeachingDevice : IIdentifiable
    {
        public string Id { get; }
        public string Name { get; }
        public string StorageLocation { get; }
        public DeviceStatus Status { get; private set; }

        // Các lớp bên ngoài chỉ được đăng ký (+) hoặc hủy đăng ký (-) event.
        public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<DeviceAlertEventArgs>? AlertRaised;

        protected TeachingDevice(string id, string name, string storageLocation,
            DeviceStatus status = DeviceStatus.Available)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Mã thiết bị không được để trống.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tên thiết bị không được để trống.", nameof(name));

            if (string.IsNullOrWhiteSpace(storageLocation))
                throw new ArgumentException("Vị trí lưu trữ không được để trống.", nameof(storageLocation));

            Id = id.Trim();
            Name = name.Trim();
            StorageLocation = storageLocation.Trim();
            Status = status;
        }

        public abstract bool IsReadyForUse();
        public abstract double GetDefaultPriorityScore();

        // Chỉ công bố event nếu trạng thái thực sự thay đổi.
        public void ChangeStatus(DeviceStatus newStatus)
        {
            if (newStatus == Status)
                return;

            DeviceStatus oldStatus = Status;
            Status = newStatus;

            OnStatusChanged(new DeviceStatusChangedEventArgs(
                Id, oldStatus, newStatus, DateTime.Now));
        }

        // Giữ lại hai phương thức của lab trước, nhưng thay gán trực tiếp bằng ChangeStatus.
        public void Borrow()
        {
            if (!IsReadyForUse())
                throw new DeviceNotReadyException(Id, Status);

            ChangeStatus(DeviceStatus.Borrowed);
        }

        public void Return()
        {
            if (Status != DeviceStatus.Borrowed)
                throw new InvalidOperationException("Chỉ thiết bị đang được mượn mới có thể được trả.");

            ChangeStatus(DeviceStatus.Available);
        }

        // Lớp thiết bị phát hiện vấn đề, không gọi trực tiếp dịch vụ thông báo/bảo trì.
        protected void RaiseAlert(string message, AlertSeverity severity)
        {
            OnAlertRaised(new DeviceAlertEventArgs(Id, message, severity, DateTime.Now));
        }

        protected virtual void OnStatusChanged(DeviceStatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        protected virtual void OnAlertRaised(DeviceAlertEventArgs e)
        {
            AlertRaised?.Invoke(this, e);
        }

        public override string ToString()
        {
            return $"{Id} - {Name} | Vị trí: {StorageLocation} | Trạng thái: {Status}";
        }
    }
}