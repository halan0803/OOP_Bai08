/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;

namespace TeachingDeviceManagement
{
    public class ProjectorRemote : TeachingDevice
    {
        public int BatteryLevel { get; private set; }

        public ProjectorRemote(string id, string name, string storageLocation,
            int batteryLevel, DeviceStatus status = DeviceStatus.Available)
            : base(id, name, storageLocation, status)
        {
            ValidateReading(batteryLevel);
            BatteryLevel = batteryLevel;
        }

        // 0: hết pin; 1: pin thấp; 2: pin bình thường.
        private static int GetBatteryZone(int level)
        {
            if (level == 0) return 0;
            if (level < 20) return 1;
            return 2;
        }

        private void ValidateReading(int level)
        {
            if (level < 0 || level > 100)
                throw new InvalidDeviceReadingException(Id, level);
        }

        public void UpdateBatteryLevel(int newLevel)
        {
            ValidateReading(newLevel);
            int oldZone = GetBatteryZone(BatteryLevel);
            int newZone = GetBatteryZone(newLevel);
            BatteryLevel = newLevel;

            // Không cảnh báo lặp lại khi mức pin vẫn cùng vùng.
            if (oldZone == newZone) return;

            if (newZone == 0)
                RaiseAlert("Pin điều khiển máy chiếu đã hết (0%).", AlertSeverity.Critical);
            else if (newZone == 1)
                RaiseAlert($"Pin điều khiển máy chiếu thấp ({newLevel}%).", AlertSeverity.Warning);
        }

        public override bool IsReadyForUse()
        {
            return Status == DeviceStatus.Available && BatteryLevel >= 20;
        }

        public override double GetDefaultPriorityScore()
        {
            return BatteryLevel;
        }
    }
}