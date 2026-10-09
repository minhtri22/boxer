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
            CoachModule.Guard => new("PHÒNG THỦ", "Giữ bình tĩnh\nHƯỚNG DẪN", "Ngừng chạm: tay trở về thủ sau khi đòn kết thúc. Quan sát rồi ra đòn tiếp.\n\nThủ không chặn mọi đòn; vẫn cần né đầu và di chuyển.\n\nĐây là hướng dẫn, không nâng chỉ số hoặc bật AI trong bài tập."),
            CoachModule.Conditioning => new("THỂ LỰC", "Quản lý nhịp đánh\nHƯỚNG DẪN", "Stamina = sức bền. Capacity = năng lượng bùng nổ.\n\nĐánh hụt vẫn tốn tài nguyên. Năng lượng thấp làm đòn yếu và thu tay chậm; ngừng đấm để hồi phục.\n\nHướng dẫn này không tăng HP, Stamina hay sức mạnh."),
            _ => throw new ArgumentOutOfRangeException(nameof(module))
        };
    }
}
