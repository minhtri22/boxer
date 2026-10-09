using System;

namespace BoxerP0
{
    public enum CoachModule { Head, Footwork, Punches, Guard, Conditioning }
    public readonly struct CoachLesson
    {
        public readonly string Title, Summary, Instructions;
        public readonly OnboardingStage Stage;
        public bool Interactive => Stage != OnboardingStage.WaitingForCalibration;
        public CoachLesson(string title, string summary, string instructions, OnboardingStage stage = OnboardingStage.WaitingForCalibration)
        { Title = title; Summary = summary; Instructions = instructions; Stage = stage; }
    }
    // Product/navigation vocabulary only. Never modifies combat or awards attributes.
    public static class CoachCatalog
    {
        public const int Count = 5;
        public static bool Valid(CoachModule module) => (int)module >= 0 && (int)module < Count;
        public static int CompletionBit(CoachModule module) => Valid(module) && (int)module < 3 ? 1 << (int)module : 0;
        public static CoachLesson Get(CoachModule module) => module switch
        {
            CoachModule.Head => new("NÉ ĐẦU", "Nghiêng trái / phải\nTHỰC HÀNH", "Điện thoại là đầu. Giữ tư thế trung lập, nghiêng hoặc lắc sang trái rồi sang phải.\n\nCho phép Motion trên điện thoại; dùng đặt lại tư thế nếu cần.", OnboardingStage.HeadControl),
            CoachModule.Footwork => new("DI CHUYỂN", "Giữ + vuốt góc trái\nTHỰC HÀNH", "Ngón trái là chân. Giữ vùng dưới bên trái, vuốt lên / xuống để tiến / lùi; vuốt ngang để đổi góc.", OnboardingStage.Footwork),
            CoachModule.Punches => new("ĐÒN ĐẤM", "Chạm / vuốt dứt khoát\nTHỰC HÀNH", "Ngón phải là đòn đấm. Chạm để đấm thẳng; vuốt lên / xuống / ngang rồi thả để chọn họ đòn.", OnboardingStage.Punches),
            CoachModule.Guard => new("PHÒNG THỦ", "Giữ bình tĩnh\nHƯỚNG DẪN", "Không có hành động đang diễn ra: tay trở về thế thủ. Ngừng chạm để chờ hồi phục; quan sát đối thủ trước khi ra đòn tiếp.\n\nThủ không bảo đảm chặn mọi đòn. Vẫn cần né đầu và di chuyển.\n\nĐây là hướng dẫn quy tắc hiện tại, không phải nâng chỉ số hoặc bài đỡ có AI."),
            CoachModule.Conditioning => new("THỂ LỰC", "Quản lý nhịp đánh\nHƯỚNG DẪN", "Stamina là sức bền; vạch Capacity mảnh là khả năng bùng nổ. Đòn được chấp nhận tốn tài nguyên kể cả khi đánh hụt.\n\nNăng lượng thấp làm giảm hiệu quả và kéo dài thu tay. Ngừng đấm để hồi phục; di chuyển vẫn có chi phí.\n\nMàn này không tăng Stamina, HP hay sức mạnh. Cân bằng trận hiện tại đang được giữ nguyên."),
            _ => throw new ArgumentOutOfRangeException(nameof(module))
        };
    }
}
