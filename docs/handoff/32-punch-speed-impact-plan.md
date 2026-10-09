# Kế hoạch sửa tốc độ và sức nặng của cú đấm POV

Ngày: 2026-10-09. Trạng thái: PLAN_ONLY_PENDING_APPROVAL.

Checkout: `D:\WORK\RESEARCH\POVGame\boxer-wave1`.
Nhánh: `feature/boxer-product-loop-wave1`.
Baseline đọc trực tiếp: `4f50bf881a032bc62c8c1077a628a4a516beac29`.

Yêu cầu: tay phải phóng nhanh, có cảm giác truyền lực và va chạm, không giống khua tay. Turn này chỉ nghiên cứu/viết kế hoạch; chưa sửa gameplay, build hoặc deploy. Các con số bên dưới là mục tiêu thử nghiệm game, không phải mô phỏng sinh lý chính xác.

## 1. Chẩn đoán từ code hiện tại

| Quan sát đã xác minh | Ý nghĩa cần kiểm chứng bằng chuyển động |
| --- | --- |
| `PlayerBoxer.cs:31` dùng Commit 90 ms, Extend 140 ms, Recover cơ sở 280 ms cho mọi đòn. | Chu kỳ danh nghĩa 510 ms trước fatigue/footwork và lượng tử hóa theo frame; đây không phải thời gian tới contact. Jab và đòn nặng chưa có nhịp riêng. |
| `Round2Motion.cs:65` dùng cùng smoothstep cho lấy đà, vươn và thu tay. | Vận tốc tham số về 0 ở đầu/cuối từng pha; dễ tạo cảm giác mềm. Vận tốc găng thực tế còn phụ thuộc đường cong, vai, IK và chuyển động root, cần đo riêng. |
| `BoxerInput.UpdateTouchInput` phát yêu cầu lúc thả ngón; `Phase0Model.cs:175` từ chối swipe dưới 120 ms. | Vuốt dứt khoát có thể không tạo đòn; thời gian thao tác không được nhầm với độ trễ sau khi game nhận lệnh. Tap không có điều kiện tối thiểu 120 ms này. |
| `TimedActionState.Step` chỉ chuyển một pha mỗi frame, đặt PhaseTime về 0, bỏ thời gian dư. | Nhịp phụ thuộc FPS. Chỉ thêm vòng lặp chuyển pha sẽ không đủ: contact sampler hiện giả định quan sát được Extend hoặc Extend→Recover. |
| `BoxerFeedback` tạo HIT bằng sin 100 Hz/80 ms, BLOCK bằng sin 180 Hz/55 ms; API chỉ nhận outcome. | Thiếu transient/tiếng vật liệu, không có thông tin ai đánh ai, hướng hay vị trí va chạm cho phản ứng. Không phát MISS. |
| Haptics chỉ biên dịch cho iOS/Android native, không có nhánh WebGL trong `BoxerFeedback`. | Không được hứa rung trên bản Pages chỉ vì điện thoại có motor rung. |
| `RamirezAcceptedRig.Apply` đọc các anchor, khôi phục pose; chưa thấy lớp phản ứng HIT/BLOCK riêng trong đường chạy đã đọc. | Ramirez ít biểu lộ hấp thụ lực. Đây là nhận định về code đã xem, chưa phải kết quả video so sánh. |

Không đổ lỗi cho tay POV mới: `Blender3DVisualFollower` chạy sau combat rig và bám găng trực tiếp, không có smoothing trễ trong driver cẳng tay mới. Các hàm pose cũ còn trong PlayerBoxer không phải nguồn pose đang dùng; nguồn chính là Round2Motion/Round2CombatRig.

## 2. Tham khảo và cách áp dụng

- [Atha et al., The damaging punch, BMJ 1985](https://pmc.ncbi.nlm.nih.gov/articles/PMC1419171/?page=3): một thí nghiệm trên Frank Bruno với mục tiêu có thiết bị đo báo cáo 0,49 m trong 0,1 s, vận tốc tại va chạm 8,9 m/s, đỉnh lực sau contact 14 ms. Chỉ dùng để tham khảo tính bùng nổ và va chạm ngắn; không coi một võ sĩ/thiết bị là chuẩn cho mọi loại đòn, không chuyển Newton sang HP.
- [Liu et al., Frontiers 2023, DOI 10.3389/fphys.2022.1099682](https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2022.1099682/pdf): nghiên cứu 11 boxer và 16 vận động viên sanda phân biệt vận tốc cực đại, vận tốc contact và giảm tốc. Nhóm boxer có vận tốc cực đại trung bình 7,31 m/s nhưng tại contact 5,63 m/s. Nghiên cứu cũng bàn vai trò chi dưới. Hệ quả thiết kế: đo cả vận tốc contact, không chỉ tăng tốc clip; thể hiện liên kết vai/thân, không chỉ bàn tay.

Video tham khảo bổ sung được định danh: [Masahiro Sakurai — Stop for Big Moments!](https://www.youtube.com/watch?v=OdVkEOzdCPw). Công cụ web không cung cấp transcript/nội dung video để phân tích trong lượt này; không gán các giá trị ms bên dưới cho Sakurai, không tuyên bố đã xem video.

Suy luận thiết kế của kế hoạch: cảm giác nặng gồm phóng tay nhanh + tiếp xúc rõ + phản ứng đích + âm thanh đồng bộ. Damage cao hơn không tự giải quyết chuyển động mềm.

## 3. Phạm vi và những gì giữ nguyên

- Ưu tiên tay người chơi trong cả tập luyện và thi đấu. Chưa tăng tốc/giảm telegraph AI trong lượt thử đầu.
- Giữ asset Ramirez, găng đen, tay/khuỷu mới, bảng kính mờ, HUD, ring girl/chuông/crowd, camera FOV và mapping né/di chuyển.
- Giữ công thức damage, cost, Quality, fatigue RecoveryFactor và quy tắc HIT/BLOCK/MISS, HP=0 kết thúc trận. Không tăng damage để giả cảm giác nặng, không bỏ guard, không mở rộng hitbox/reach.
- Mọi đổi đường đi hoặc nhịp găng phải vào pose/timeline chung mà renderer và sweep cùng đọc. Không chỉ tăng animation speed ở Blender follower.
- Timing/cadence mới vẫn làm đổi HP/Stamina theo thời gian vì số đòn, thời gian hồi và cửa sổ bị phản công thay đổi. Không tuyên bố giữ nguyên cân bằng chỉ vì công thức damage không đổi.
- Sửa lớp thời gian chung nếu ảnh hưởng AI phải có báo cáo trước/sau; mục tiêu giữ profile AI hiện tại, không hứa kết quả va chạm AI tuyệt đối giống baseline khi lỗi thời gian được sửa.

## 4. Các bước triển khai đề xuất sau khi được duyệt

### P0 — Đo baseline và bảo đảm timeline/contact

1. Ghi timestamp touch-start/release → request → accepted → Commit/Extend → contact → Recover → Guard; tách rejected-fast-swipe, rejected-busy và menu release barrier.
2. Ghi đường đi/vận tốc găng sau IK, vị trí contact, pose vai/khuỷu, FPS/frame time; không dùng độ dài input swipe thay cho tốc độ găng.
3. Sửa carry-over thời gian theo các đoạn pha có thứ tự, có action ID. Cung cấp mọi đoạn Extend đi qua trong frame cho sampler, kể cả frame đi qua cả Commit và Extend. Không sweep giữa hai action khác nhau.
4. Tích phân hồi phục/timer theo các đoạn pha tương ứng: không lấy pha cuối frame cho toàn bộ dt khi trong frame đã đổi pha. HP=0 phải chặn các đoạn/action còn lại, không phát HIT sau KO.
5. Đường sweep vẫn kiểm tra tiếp xúc tương đối với các volume chuyển động, lấy mặt chạm sớm nhất, tối đa một receipt resolution/action. Mốc lấy mẫu 240 Hz hiện tại là điểm bắt đầu; kiểm tra hội tụ với bước nhỏ hơn, bổ sung adaptive subdivision nếu curve mới quá gắt.

Đầu ra: trace/đồ thị và clip baseline ở tốc độ thường, slow motion; không đánh giá feel từ screenshot tĩnh.

### P1 — Đường cong phóng/thu tay và profile riêng

Thử A trước: giữ duration cơ sở hiện tại, thay nhịp trong pha để cô lập tác động đường cong. Thử B: rút thời gian theo profile bên dưới. A vẫn cần kiểm thử va chạm/cân bằng vì đổi thời điểm tiếp xúc.

| Đòn người chơi | Commit thử B | Extend thử B | Recover cơ sở thử B | Tổng danh nghĩa |
| --- | ---: | ---: | ---: | ---: |
| Jab | 40 ms | 70 ms | 190 ms | 300 ms |
| Cross | 60 ms | 85 ms | 230 ms | 375 ms |
| Hook trái/phải | 70 ms | 95 ms | 260 ms | 425 ms |
| Uppercut | 80 ms | 100 ms | 275 ms | 455 ms |
| Overhand | 90 ms | 105 ms | 300 ms | 495 ms |

Đây là preset ban đầu ở Quality=1 và không có modifier footwork, chưa phải thông số chốt hay thời gian chạm mục tiêu. Fatigue/footwork vẫn nhân recovery theo mô hình hiện tại.

- Commit ngắn, không kéo tay ngược quá lớn với jab; hook/uppercut/overhand vẫn có dấu hiệu lấy đà đọc được.
- Extend bất đối xứng: tăng tốc nhanh, phần hành trình đánh chính ngắn, không cùng kiểu easing với thu tay. Tránh giảm tốc dài trước điểm chạm điển hình; không yêu cầu velocity cực đại trùng mọi contact vì khoảng cách/guard thay đổi.
- Thu tay có đoạn rút nhanh ban đầu rồi ổn định ở guard; phân biệt nhìn thấy tay đã về gần guard với thời điểm GuardActive thật. Chỉ chốt cả hai cùng nhau sau kiểm thử.
- Vai/thân dẫn động hợp lý theo loại đòn, tay không tách khỏi khuỷu/cổ tay. Trước hết giữ endpoint/reach/chiều dài xương; điều chỉnh gross trajectory chỉ khi A/B cho thấy cần và phải requalify geometry.
- Profile người chơi được mang qua Round2Frame và sampler; AI dùng profile/curve legacy ở lần thử đầu, không thay chung Smooth rồi vô tình tăng tốc cả Ramirez.
- Mốc thử: peak speed của găng tăng khoảng 1,5–2 lần baseline trong cùng tình huống đối với jab/cross, nhưng phải xem contact speed, clipping và hình dáng đòn. Không ép mọi family đạt cùng mức hay coi đây là vận tốc boxer thực.

### P2 — Va chạm có sức nặng

1. Phát event có actor/action ID, outcome, vị trí, hướng, vùng HEAD/BODY/GUARD và Quality từ receipt sau khi resolution hợp lệ; không phát feedback từ giao cắt giả hay từ UI.
2. HIT: tiếng tiếp xúc da/găng có transient sắc + body thud ngắn; chọn lớp âm theo vùng và đòn. BLOCK: tiếng găng chặn riêng, giảm nhấn lực. MISS: chỉ whoosh nhẹ theo vận tốc, không thud/giật đích giả.
3. Phản ứng đầu/thân Ramirez theo hướng HIT, recoil găng đỡ khi BLOCK. Ban đầu chỉ visual bounded, không stun/displace actor hay hủy đòn AI. Sai lệch mesh–volume không vượt tolerance visual/contact đã nghiệm thu; nếu cần chuyển head/root lớn phải làm thành thay đổi gameplay được duyệt riêng.
4. Nhấn tiếp xúc 20–40 ms bằng lớp compression/recoil nhỏ ở vùng vỏ găng/vật liệu, tâm găng authoritative vẫn đúng anchor. A/B bật/tắt; không dùng Time.timeScale=0, không đóng băng mesh trong khi collider tiếp tục chạy.
5. Camera kick rất nhỏ là tùy chọn sau khi tay/âm đã đủ tốt, có mức 0/tắt; không sửa tín hiệu nghiêng máy, không đổi hitbox đầu hoặc lắc mạnh gây khó né/say hình.
6. Haptics WebGL chỉ là nâng cấp tùy chọn, feature-detect và fallback im lặng; kiểm tra từng trình duyệt/thiết bị, không coi rung là điều kiện chơi được.
7. Training dùng cùng profile chuyển động; không tạo damage/receipt của Fight. Chỉ phát HIT thực khi có target tập luyện riêng; tập khua vào không khí không được giả phản ứng trúng Ramirez.

### P3 — Thao tác nhanh và nhịp liên hoàn

- Đo trước rồi A/B ngưỡng swipe nhanh 50–80 ms hoặc nhận theo displacement/độ chắc hướng thay vì bắt giữ đủ 120 ms. Giữ đúng UP→uppercut, DOWN→overhand, LEFT→rear hook, RIGHT→lead hook, TAP→straight. Cập nhật hướng dẫn/test tương ứng nếu chốt thay ngưỡng.
- Giữ phát đòn tại release trong vòng đầu; không đổi sang touch-down vì lúc đó chưa biết family. Cancel không tạo cú đấm; chạm nút/menu và thao tác di chuyển không được lọt thành punch.
- Chưa thêm combo buffer ở vòng đầu. Nếu trace cho thấy người chơi mất lệnh khi gần hết recovery: thử tối đa một lệnh trong 60–80 ms cuối Recover, hủy khi KO/đổi màn hình. Chỉ trừ stamina khi lệnh thực sự được accepted, không tích hàng đợi spam. Đây là nhánh cần duyệt vì thay nhịp gameplay.

## 5. Gate kiểm thử và nghiệm thu

- Timeline: 30/60/120 FPS và spike 100–200 ms; cùng lịch input theo timestamp. Không rơi pha, không mất/nhân đôi contact, không sweep xuyên hai action; vitals tính đúng thời gian từng pha.
- Input: đo độ trễ release→accepted→first visible change, tách thao tác có sẵn/busy. Mục tiêu ban đầu first visible change trong tối đa 2 frame khi idle; không diễn giải đó là độ trễ vật lý sensor/display. Ghi p50/p95 trên thiết bị thật.
- Contact: head/body HIT, closed-guard BLOCK, long-range MISS, guard đang chuyển động, hook pocket, đồng thời di chuyển/né, hai actor đấm cùng frame, cả hai tay. Giữ earliest-contact và đúng một debit/damage/feedback/action. Không đổi HIT/BLOCK để tăng tỷ lệ thắng.
- HP/Stamina: kiểm tra Quality 1 và thấp, stamina 100/50/20, hold-idle hồi phục, forward penalty, BLOCK không mất HP, MISS không có damage, KO dừng input/AI/timer và Result đúng. Khi attack bị hủy do KO/đổi màn thì không resolve thêm damage.
- Balance: so A/B với cùng seed, cùng lịch thao tác và cùng số đòn được accepted; báo cáo riêng cadence, đòn trúng/đỡ/trượt, HP/Stamina theo thời gian, counter/exposure window. Không yêu cầu outcome giống hệt khi timing thay đổi; mọi chênh lệch phải giải thích được bằng trace.
- Visual: clip jab/cross/hook/uppercut/overhand ở tốc độ thường + 0,25x, portrait/landscape, high/low stamina; nhìn rõ cẳng tay/khuỷu, không gập cổ tay, không xuyên găng, không snap. Cùng camera/khoảng cách để A/B công bằng.
- Audio/impact: thud/visual response cùng frame resolution (sai số phát âm thực tế đo trên thiết bị); MISS không HIT feedback, BLOCK khác HIT; không che tiếng chuông/crowd. Không hứa browser audio pipeline không có latency.
- Chạy lại bộ kiểm thử vitals/fairness/controller/geometry, các browser gate màn hình/media/contact hiện có. Thêm test mới cho timeline và impact; không nới assertion cũ chỉ để đạt PASS. Giữ báo cáo negative và lỗi tái hiện.
- Chạy audit contact độc lập trước, sau đó thêm tải/frame-spike test. Một lần pass riêng không chứng minh touch latency ổn định trên mọi máy.
- User UAT trên điện thoại: thấy đòn phóng dứt khoát, cảm được HIT/BLOCK/MISS khác nhau, jab nhanh hơn đòn nặng, stamina thấp làm recovery chậm đúng mô hình, không khó chịu do camera, không cần giữ vuốt lâu. Test số học không thay được gate này.

Trước Human UAT đạt: không tuyên bố HUMAN_PASS. Nếu triển khai/build/test đạt nhưng chưa có nghiệm thu thiết bị: `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT`.

## 6. Thứ tự bàn giao

1. Duyệt phạm vi/profile thử; P0 timeline + baseline trace.
2. P1 prototype A/B tay, chưa thêm rung/flash để người dùng đánh giá được chuyển động thật.
3. P2 phản hồi HIT/BLOCK/MISS + P3 sửa fast-swipe nếu số đo xác nhận cần.
4. Test hồi quy, clip A/B và UAT; cân nhắc buffer/camera kick chỉ khi có nhu cầu được chứng minh.
5. Khi được yêu cầu release và các gate kỹ thuật đạt: build WebGL → ghi provenance SHA → workflow/deploy → kiểm tra bản public. Báo cáo tách triển khai, build, deploy và Human UAT.

Không thực hiện build/push/deploy trong lượt viết kế hoạch này. Không tác động tới phiên Python được bảo vệ PID 9152 hoặc launcher của nó.
