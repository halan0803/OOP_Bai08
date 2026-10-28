/********************************
Mã sinh viên: 202418931
Họ tên: Trần Thị Hà Lan
********************************/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TeachingDeviceManagement
{
    public class AirConditionerRemote : TeachingDevice
    {
        // Giữ thuộc tính phòng tương thích từ lab trước.
        public IReadOnlySet<string> CompatibleRooms { get; }
        public string AssignedRoom { get; }
        public bool IsResponsive { get; private set; }

        public AirConditionerRemote(string id, string name, string storageLocation,
            IEnumerable<string> compatibleRooms,
            DeviceStatus status = DeviceStatus.Available,
            string assignedRoom = "")
            : base(id, name, storageLocation, status)
        {
            if (compatibleRooms == null)
                throw new ArgumentNullException(nameof(compatibleRooms));

            HashSet<string> rooms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string room in compatibleRooms)
            {
                if (!string.IsNullOrWhiteSpace(room))
                    rooms.Add(room.Trim());
            }

            CompatibleRooms = rooms.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(assignedRoom) && !IsCompatibleWith(assignedRoom))
                throw new ArgumentException("Phòng được giao không nằm trong danh sách phòng tương thích.");

            AssignedRoom = assignedRoom?.Trim() ?? string.Empty;
            IsResponsive = true;
        }

        public bool IsCompatibleWith(string roomId)
        {
            return !string.IsNullOrWhiteSpace(roomId)
                && CompatibleRooms.Contains(roomId.Trim());
        }

        public void UpdateResponseState(bool isResponsive)
        {
            bool oldState = IsResponsive;
            IsResponsive = isResponsive;

            // Chỉ cảnh báo khi chuyển từ có phản hồi sang mất phản hồi.
            if (oldState && !isResponsive)
                RaiseAlert("Điều khiển điều hòa không phản hồi.", AlertSeverity.Critical);
        }

        public override bool IsReadyForUse()
        {
            return Status == DeviceStatus.Available && IsResponsive;
        }

        public override double GetDefaultPriorityScore()
        {
            return CompatibleRooms.Count > 0 ? 100 : 0;
        }
    }
}