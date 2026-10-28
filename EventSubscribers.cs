/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;
using System.Collections.Generic;
namespace TeachingDeviceManagement
{
    // Subscriber 1: In cảnh báo lên console.
    public class ConsoleNotificationService
    {
        public void HandleAlert(object? sender, DeviceAlertEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message}");
        }
    }

    // Subscriber 2: Lưu lịch sử thay đổi trạng thái và cảnh báo.
    public class DeviceAuditLogger
    {
        private readonly List<string> entries = new List<string>();
        public IReadOnlyList<string> Entries => entries.AsReadOnly();

        public void HandleStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
        {
            entries.Add($"{e.ChangedAt:dd/MM/yyyy HH:mm:ss}: {e.DeviceId}, " +
                $"{e.OldStatus} -> {e.NewStatus}");
        }

        public void HandleAlert(object? sender, DeviceAlertEventArgs e)
        {
            entries.Add($"{e.OccurredAt:dd/MM/yyyy HH:mm:ss}: {e.DeviceId}, " +
                $"[{e.Severity}] {e.Message}");
        }
    }

    // Subscriber 3: Chỉ tạo yêu cầu sửa chữa khi cảnh báo Critical.
    public class MaintenanceService
    {
        private readonly List<string> requests = new List<string>();
        public IReadOnlyList<string> Requests => requests.AsReadOnly();

        public void HandleCriticalAlert(object? sender, DeviceAlertEventArgs e)
        {
            if (e.Severity != AlertSeverity.Critical)
                return;

            requests.Add($"Maintenance request for {e.DeviceId}: {e.Message}");
        }
    }
}