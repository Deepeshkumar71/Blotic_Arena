using System.Windows.Media;

namespace BloticArena.Models
{
    public class AppInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string LaunchCommand { get; set; } = string.Empty;
        public string Icon { get; set; } = "📦";
        public string Type { get; set; } = "Application";
        public string Category { get; set; } = "Other";
        public ImageSource? IconImage { get; set; }
    }
}
