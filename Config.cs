using Exiled.API.Interfaces;
using System.ComponentModel;
using static Broadcast;

namespace BroadcastStack
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Broadcast flags for stacked messages.")]
        public BroadcastFlags Flags { get; set; } = BroadcastFlags.Normal;

        [Description("Clear the stack when a broadcast requests shouldClearPrevious.")]
        public bool ClearOnShouldClearPrevious { get; set; } = true;

        [Description("Drop empty or whitespace messages.")]
        public bool IgnoreEmpty { get; set; } = true;

        [Description("Show staff chat as a stacked broadcast to RA users.")]
        public bool StackStaffChat { get; set; } = true;

        [Description("How long staff chat broadcasts stay on screen (seconds).")]
        public ushort StaffChatDuration { get; set; } = 5;

        [Description("Prefix for staff chat broadcasts.")]
        public string StaffChatPrefix { get; set; } = "<size=20>[Staff]</size>";

        [Description("Include the sender's display name in staff chat broadcasts.")]
        public bool ShowStaffChatSenderName { get; set; } = true;
    }
}
