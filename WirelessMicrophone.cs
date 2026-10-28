/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;

namespace TeachingDeviceManagement
{
    public class WirelessMicrophone : TeachingDevice
    {
        public int BatteryLevel { get; private set; }
        public int SignalQuality { get; private set; }

        public WirelessMicrophone(string id, string name, string storageLocation,
            int batteryLevel, int signalQuality,
            DeviceStatus status = DeviceStatus.Available)
            : base(id, name, storageLocation, status)
        {
            ValidateReading(batteryLevel);
            ValidateReading(signalQuality);
            BatteryLevel = batteryLevel;
            SignalQuality = signalQuality;
        }

        private void ValidateReading(int value)
        {
            if (value < 0 || value > 100)
                throw new InvalidDeviceReadingException(Id, value);
        }

        // Trả về vùng: 0 = lỗi nghiêm trọng, 1 = cảnh báo, 2 = bình thường.
        private static int GetZone(int value, int warningLimit)
        {
            if (value == 0) return 0;
            if (value < warningLimit) return 1;
            return 2;
        }

        public void UpdateBatteryLevel(int newLevel)
        {
            ValidateReading(newLevel);
            int oldZone = GetZone(BatteryLevel, 30);
            int newZone = GetZone(newLevel, 30);
            BatteryLevel = newLevel;

            if (oldZone == newZone) return;

            // Cảnh báo Warning được phát ra khi mức pin ở vùng nguy hiểm (< 30%).
            // Hệ thống cố tình giữ cảnh báo cả ở chiều sạc tăng lên từ 0%, 
            // vì mức pin này vẫn chưa đủ an toàn để đảm bảo duy trì trọn vẹn một ca dạy học liên tục.
            if (newZone == 0)
                RaiseAlert("Pin micro đã hết (0%).", AlertSeverity.Critical);
            else if (newZone == 1)
                RaiseAlert($"Pin micro thấp ({newLevel}%).", AlertSeverity.Warning);
        }

        public void UpdateSignalQuality(int newQuality)
        {
            ValidateReading(newQuality);
            int oldZone = GetZone(SignalQuality, 40);
            int newZone = GetZone(newQuality, 40);
            SignalQuality = newQuality;

            if (oldZone == newZone) return;

            // Phát cảnh báo Warning khi tín hiệu < 40%. Ngay cả khi tín hiệu 
            // vừa phục hồi từ 0 (mất kết nối hoàn toàn) lên mức yếu, hệ thống vẫn cảnh báo 
            // để giảng viên biết âm thanh lúc này có thể bị chập chờn, ảnh hưởng đến bài giảng.
            if (newZone == 0)
                RaiseAlert("Micro mất tín hiệu (0%).", AlertSeverity.Critical);
            else if (newZone == 1)
                RaiseAlert($"Tín hiệu micro yếu ({newQuality}%).", AlertSeverity.Warning);
        }

        public override bool IsReadyForUse()
        {
            return Status == DeviceStatus.Available
                && BatteryLevel >= 30
                && SignalQuality >= 40;
        }

        public override double GetDefaultPriorityScore()
        {
            return (BatteryLevel + SignalQuality) / 2.0;
        }
    }
}