# Handoff: agent backup / thử nghiệm — cách ly nhánh

Ngày: 2026-09-19. Người dùng đã yêu cầu tách hai luồng công việc.
Vai trò của nhánh này: BACKUP / EXPERIMENT ONLY. Hình nhân vật hiện tại chưa được
người dùng chấp nhận. Các nhãn PASS trong tài liệu cũ không phải Human UAT PASS.

## Nơi agent này được làm việc

- Branch: `experiment/other-agent-backup`
- Worktree: `D:/WORK/RESEARCH/POVGame/boxer-other-agent-experiments`
- Remote branch: `origin/experiment/other-agent-backup`
- Mốc gốc: `8c2d590484a8dfc67438c3359994f601b4ec08bd`.
  Sáu commit Blender riêng của luồng cũ được giữ nguyên trong lịch sử.
- Snapshot gồm 330 file chỉnh sửa/chưa commit từ `boxer-ramirez-art`.
- Các file chưa commit ở checkout dùng chung được lưu riêng trong
  `evidence/branch-isolation-20260919/shared-checkout-untracked.zip`.
- Danh sách và SHA-256: `evidence/branch-isolation-20260919/backup-manifest.json`.

## Thao tác đầu tiên khi nhận handoff

Dừng dùng checkout cũ; chuyển working directory của task sang worktree trên.
Đọc `AGENTS.md` tại root. Trước mỗi lượt sửa, commit hoặc push, chạy:

```powershell
Set-Location 'D:\WORK\RESEARCH\POVGame\boxer-other-agent-experiments'
git rev-parse --show-toplevel
git branch --show-current
git status --short --branch
```

Chỉ tiếp tục nếu đường dẫn và branch đúng hoàn toàn. Nếu khác, dừng và báo người
dùng; không tự reset/switch một checkout có thay đổi để sửa tình trạng này.

## Phạm vi được phép

- Tiếp tục thử nghiệm Blender/asset hoặc bảo trì thử nghiệm trên nhánh riêng này.
- Giữ nguyên reference đã duyệt; lưu nguồn, render và kết quả đánh giá trung thực.
- Commit công việc và evidence riêng, ghi rõ hạn chế hoặc chất lượng chưa đạt.
- Nếu cần push, dùng đúng refspec:

```powershell
git push origin HEAD:refs/heads/experiment/other-agent-backup
```

## Ranh giới bắt buộc

- Không ghi file trong `boxer-ramirez-realistic`, `boxer`, hoặc worktree khác.
- Không commit, merge, cherry-pick, rebase, reset hoặc push vào
  `art/ramirez-realistic-blender`, `p1/whole-body-mechanics` hay `main`.
- Không dùng `git push --all`, `--mirror`, hoặc force push.
- Không tự đưa kết quả vào nhánh chính; không mở PR nhằm tự nhập nhánh.
- Không chạy workflow GitHub Pages hoặc thay thế UAT đang công bố.
- Không thay đổi upstream/hook/config dùng chung để mở rộng quyền ghi nhánh.
- Chỉ chuyển giao file/commit để tham khảo khi người dùng yêu cầu; việc tiếp nhận
  vào luồng chính cần một chỉ thị riêng của người dùng.

## Luồng chính để tránh nhầm

Luồng chính mới: `art/ramirez-realistic-blender`, tại
`D:/WORK/RESEARCH/POVGame/boxer-ramirez-realistic`.
Luồng này khôi phục WIP của agent chính từ baseline `b0eeed2`, ưu tiên hoàn thiện
nhân vật giống người thật trong Blender trước khi làm tiếp mã Unity.
Không lấy nhánh này làm đích push của agent backup.

## Tình trạng bàn giao

Đây là chuyển giao nhánh và bảo toàn công việc, không phải phê duyệt chất lượng
nhân vật, không phải bàn giao build UAT. Handoff này thay thế chỉ dẫn đích nhánh
trong các tài liệu kế thừa trước ngày 2026-09-19. Các checkout cũ vẫn còn nguyên
để đối chiếu; chúng không phải nơi agent này tiếp tục triển khai.
