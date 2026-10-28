/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;
using System.Collections.Generic;

namespace TeachingDeviceManagement
{
    class Program
    {
        private static int passed;
        private static int total;

        // Hàm kiểm tra và in kết quả PASS/FAIL ra màn hình.
        static void Check(string testName, bool condition)
        {
            total++;
            if (condition)
            {
                passed++;
                Console.WriteLine($"=> PASS: {testName}");
            }
            else
            {
                Console.WriteLine($"=> FAIL: {testName}");
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            passed = 0;
            total = 0;
            Console.WriteLine("\n=== KIỂM THỬ HỆ THỐNG QUẢN LÝ CÁC THIẾT BỊ TRONG PHÒNG DẠY ===\n");

            // 1. Thay đổi trạng thái: chỉ phát event khi thực sự thay đổi.
            Console.WriteLine("1. Kiểm thử StatusChanged, Borrow() và Return()");
            var statusDevice = new ProjectorRemote("W08-P1", "Máy chiếu W08", "Tủ 1", 80);
            int statusCount = 0;
            bool correctOldNew = true;
            statusDevice.StatusChanged += (sender, e) =>
            {
                statusCount++;
                if (statusCount == 1)
                    correctOldNew = e.OldStatus == DeviceStatus.Available && e.NewStatus == DeviceStatus.Borrowed;
                if (statusCount == 2)
                    correctOldNew = correctOldNew && e.OldStatus == DeviceStatus.Borrowed && e.NewStatus == DeviceStatus.Available;
            };
            statusDevice.ChangeStatus(DeviceStatus.Available);
            statusDevice.Borrow();
            statusDevice.ChangeStatus(DeviceStatus.Borrowed);
            statusDevice.Return();
            Check("2 lần thay đổi, 2 event; old/new đúng", statusCount == 2 && correctOldNew
                && statusDevice.Status == DeviceStatus.Available);

            // 2. Máy chiếu: Warning dưới 20%, Critical ở 0%, không lặp lại cùng vùng.
            Console.WriteLine("\n2. Kiểm thử các vùng pin máy chiếu");
            var projector = new ProjectorRemote("W08-P2", "Máy chiếu 2", "Tủ 1", 80);
            int projectorWarnings = 0;
            int projectorCriticals = 0;
            projector.AlertRaised += (sender, e) =>
            {
                if (e.Severity == AlertSeverity.Warning) projectorWarnings++;
                if (e.Severity == AlertSeverity.Critical) projectorCriticals++;
            };
            projector.UpdateBatteryLevel(19);  // Warning 1
            projector.UpdateBatteryLevel(15);  // Không lặp
            projector.UpdateBatteryLevel(0);   // Critical 1
            projector.UpdateBatteryLevel(0);   // Không lặp
            projector.UpdateBatteryLevel(40);  // Trở về bình thường
            projector.UpdateBatteryLevel(18);  // Warning 2
            Check("Máy chiếu: Warning=2, Critical=1", projectorWarnings == 2 && projectorCriticals == 1);

            // 3. Micro: hai chỉ số pin và tín hiệu phát event độc lập.
            Console.WriteLine("\n3. Kiểm thử pin và tín hiệu micro");
            var mic = new WirelessMicrophone("W08-M1", "Micro W08", "Tủ 3", 90, 90);
            int micWarnings = 0;
            int micCriticals = 0;
            mic.AlertRaised += (sender, e) =>
            {
                if (e.Severity == AlertSeverity.Warning) micWarnings++;
                if (e.Severity == AlertSeverity.Critical) micCriticals++;
            };
            mic.UpdateBatteryLevel(29);
            mic.UpdateBatteryLevel(28);
            mic.UpdateSignalQuality(39);
            mic.UpdateSignalQuality(38);
            mic.UpdateBatteryLevel(0);
            mic.UpdateSignalQuality(0);
            Check("Micro: Warning=2, Critical=2, không trùng vùng", micWarnings == 2 && micCriticals == 2);

            // 4. Điều hòa: không phát event lặp lại khi vẫn mất phản hồi.
            Console.WriteLine("\n4. Kiểm thử mất phản hồi điều hòa");
            var ac = new AirConditionerRemote("W08-A1", "Điều hòa W08", "Tủ 2",
                new[] { "A203", "A204" }, assignedRoom: "A203");
            int acCriticals = 0;
            ac.AlertRaised += (sender, e) =>
            {
                if (e.Severity == AlertSeverity.Critical) acCriticals++;
            };
            ac.UpdateResponseState(false);
            ac.UpdateResponseState(false);
            ac.UpdateResponseState(true);
            ac.UpdateResponseState(false);
            Check("Điều hòa: 2 lần chuyển sang mất phản hồi", acCriticals == 2 && !ac.IsResponsive
                && ac.AssignedRoom == "A203");

            // 5. Delegate tự định nghĩa và callback: chỉ xử lý cảnh báo Critical.
            Console.WriteLine("\n5. Kiểm thử AlertFilter delegate và callback");
            var filteredDevice = new ProjectorRemote("W08-P3", "Máy chiếu 3", "Tủ 1", 50);
            int filteredCount = 0;
            var criticalOnly = new FilteredAlertSubscriber(
                alert => alert.Severity == AlertSeverity.Critical,
                alert =>
                {
                    filteredCount++;
                    Console.WriteLine($"CRITICAL CALLBACK: {alert.DeviceId} - {alert.Message}");
                });
            filteredDevice.AlertRaised += criticalOnly.HandleAlert;
            filteredDevice.UpdateBatteryLevel(10); // Warning -> bỏ qua
            filteredDevice.UpdateBatteryLevel(0);  // Critical -> callback
            filteredDevice.AlertRaised -= criticalOnly.HandleAlert;
            Check("Delegate chỉ gọi callback ở mức Critical", filteredCount == 1);

            // 6. Một event có nhiều subscriber; coordinator quản lý += / -=.
            Console.WriteLine("\n6. Kiểm thử các subscriber và coordinator");
            var monitored = new ProjectorRemote("W08-P4", "Máy chiếu 4", "Tủ 1", 80);
            var notificationService = new ConsoleNotificationService();
            var auditLogger = new DeviceAuditLogger();
            var maintenanceService = new MaintenanceService();

            using (var coordinator = new DeviceMonitoringCoordinator(
                new List<TeachingDevice> { monitored, monitored },
                notificationService, auditLogger, maintenanceService))
            {
                coordinator.Subscribe();
                coordinator.Subscribe(); // Không được đăng ký trùng
                monitored.Borrow();       // Thêm 1 dòng log trạng thái
                monitored.UpdateBatteryLevel(19); // Warning
                monitored.UpdateBatteryLevel(0);  // Critical -> bảo trì

                Check("Nhiều subscriber: 3 log, 1 yêu cầu bảo trì",
                    auditLogger.Entries.Count == 3 && maintenanceService.Requests.Count == 1);

                coordinator.Unsubscribe();
                monitored.UpdateBatteryLevel(40);
                monitored.UpdateBatteryLevel(0); // Không còn subscriber
                Check("Unsubscribe hủy đúng các handler",
                    auditLogger.Entries.Count == 3 && maintenanceService.Requests.Count == 1);

                coordinator.Subscribe();
                monitored.UpdateBatteryLevel(40);
                monitored.UpdateBatteryLevel(0);
                Check("Đăng ký lại hoạt động, không bị nhân đôi handler",
                    auditLogger.Entries.Count == 4 && maintenanceService.Requests.Count == 2);
            }
            monitored.UpdateBatteryLevel(40);
            monitored.UpdateBatteryLevel(0);
            Check("Dispose() tự gọi Unsubscribe()",
                auditLogger.Entries.Count == 4 && maintenanceService.Requests.Count == 2);

            // 7. Kiểm tra dữ liệu ngoài khoảng 0..100.
            Console.WriteLine("\n7. Kiểm thử ngoại lệ khi dữ liệu không hợp lệ");
            bool correctProjectorException = false;
            try
            {
                projector.UpdateBatteryLevel(101);
            }
            catch (InvalidDeviceReadingException ex)
            {
                correctProjectorException = ex.DeviceId == "W08-P2" && ex.InvalidValue == 101;
            }

            bool correctMicException = false;
            try
            {
                mic.UpdateSignalQuality(-1);
            }
            catch (InvalidDeviceReadingException ex)
            {
                correctMicException = ex.DeviceId == "W08-M1" && ex.InvalidValue == -1;
            }
            Check("InvalidDeviceReadingException chứa đúng thông tin", correctProjectorException && correctMicException
                && projector.BatteryLevel == 18 && mic.SignalQuality == 0);

            Console.WriteLine($"\nKẾT QUẢ: {passed}/{total} kiểm thử PASS.\n");
            Console.WriteLine("=== KẾT THÚC KIỂM THỬ HỆ THỐNG QUẢN LÝ CÁC THIẾT BỊ TRONG PHÒNG DẠY ===\n");
        }
    }
}