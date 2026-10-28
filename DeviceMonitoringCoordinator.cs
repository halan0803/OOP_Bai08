/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;
using System.Collections.Generic;

namespace TeachingDeviceManagement
{
    // Tập trung việc đăng ký và hủy đăng ký event tại một nơi.
    public class DeviceMonitoringCoordinator : IDisposable
    {
        private readonly List<TeachingDevice> devices;
        private readonly ConsoleNotificationService notificationService;
        private readonly DeviceAuditLogger auditLogger;
        private readonly MaintenanceService maintenanceService;
        private bool isSubscribed;

        public DeviceMonitoringCoordinator(IEnumerable<TeachingDevice> devices,
            ConsoleNotificationService notificationService,
            DeviceAuditLogger auditLogger,
            MaintenanceService maintenanceService)
        {
            if (devices == null) throw new ArgumentNullException(nameof(devices));
            this.devices = new List<TeachingDevice>();
            foreach (TeachingDevice device in devices)
            {
                if (device == null)
                    throw new ArgumentException("Danh sách có thiết bị null.", nameof(devices));
                if (!this.devices.Contains(device))
                    this.devices.Add(device);
            }
            this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            this.maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
        }

        public void Subscribe()
        {
            // Tránh đăng ký một handler hai lần do gọi Subscribe nhiều lần.
            if (isSubscribed) return;

            foreach (TeachingDevice device in devices)
            {
                device.StatusChanged += auditLogger.HandleStatusChanged;
                device.AlertRaised += notificationService.HandleAlert;
                device.AlertRaised += auditLogger.HandleAlert;
                device.AlertRaised += maintenanceService.HandleCriticalAlert;
            }

            isSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (!isSubscribed) return;

            foreach (TeachingDevice device in devices)
            {
                device.StatusChanged -= auditLogger.HandleStatusChanged;
                device.AlertRaised -= notificationService.HandleAlert;
                device.AlertRaised -= auditLogger.HandleAlert;
                device.AlertRaised -= maintenanceService.HandleCriticalAlert;
            }

            isSubscribed = false;
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}