/* Browser-native mobile text entry; only Unity owns identity validation and persistence. */
function createBoxerProfileView({send}) {
  const root = document.createElement('section');
  root.id = 'fighter-profile'; root.className = 'hidden'; root.lang = 'vi';
  root.setAttribute('aria-label', 'Hồ sơ võ sĩ');
  // Static markup only. User text always goes through value/textContent.
  root.innerHTML = `<form id="profile-form" novalidate>
    <p class="profile-kicker">BOXER · LOCAL IDENTITY</p>
    <h1>FIGHTER PROFILE</h1><p>HỒ SƠ VÕ SĨ</p>
    <p class="profile-note">Lưu trên trình duyệt / thiết bị này, không phải tài khoản.</p>
    <label for="profile-name">TÊN VÕ SĨ <span>1–24 ký tự</span></label>
    <input id="profile-name" name="name" type="text" maxlength="96" autocomplete="nickname" enterkeyhint="next" spellcheck="false">
    <label for="profile-nationality">QUỐC TỊCH TỰ KHAI <span>1–40 ký tự</span></label>
    <input id="profile-nationality" name="nationality" type="text" maxlength="160" list="profile-countries" autocomplete="off" enterkeyhint="done" spellcheck="false">
    <datalist id="profile-countries"><option value="Việt Nam"><option value="Mexico"><option value="Thailand"><option value="Japan"><option value="United States"><option value="United Kingdom"></datalist>
    <p class="profile-note">Gợi ý quốc tịch chỉ là ví dụ; bạn có thể tự nhập.</p>
    <p id="profile-error" role="alert"></p>
    <p id="profile-id" class="profile-note"></p>
    <p class="profile-note">Ngoại hình đang tạm hold. Ring phía sau là cảnh hiện tại, chưa có chân dung võ sĩ riêng.</p>
    <button id="profile-save" type="submit">CONFIRM · LƯU HỒ SƠ</button>
    <button id="profile-review" type="button">TRẬN GẦN NHẤT TRÊN THIẾT BỊ</button>
    <p id="profile-record" class="profile-note"></p>
    <button id="profile-cancel" type="button">CANCEL · HỦY</button>
  </form>`;
  document.body.appendChild(root);
  const field = id => root.querySelector('#profile-' + id);
  let token = null;
  function command(action) {
    if (!token) return;
    send('BrowserProfileCommand', JSON.stringify({token, action, name:field('name').value, nationality:field('nationality').value}));
  }
  field('form').addEventListener('submit', e => {e.preventDefault(); command('save');});
  field('cancel').addEventListener('click', () => command('cancel'));
  field('review').addEventListener('click', () => command('review'));
  function resize() {
    const viewport = window.visualViewport;
    root.style.height = `${viewport ? viewport.height : innerHeight}px`;
    root.style.top = `${viewport ? viewport.offsetTop : 0}px`;
  }
  window.addEventListener('resize', resize);
  if (window.visualViewport) {
    visualViewport.addEventListener('resize', resize);
    visualViewport.addEventListener('scroll', resize);
  }
  return {
    contains: target => token !== null && root.contains(target),
    render(view) {
      if (!view.open) {
        const wasOpen = token !== null; token = null;
        if (root.contains(document.activeElement)) document.activeElement.blur();
        root.classList.add('hidden'); root.inert = true;
        if (wasOpen) document.getElementById('unity-canvas').focus();
        return;
      }
      if (view.token !== token) {
        token = view.token;
        field('name').value = view.name || ''; field('nationality').value = view.nationality || '';
        root.scrollTop = 0;
      }
      root.inert = false; root.classList.remove('hidden'); resize();
      field('error').textContent = view.error || (view.invalidStored ? 'Dữ liệu hồ sơ cũ không hợp lệ; chưa thay thế cho đến khi bạn lưu.' : '');
      field('id').textContent = view.id ? `ID cục bộ · ${view.id.slice(0, 8)}` : 'Chưa lưu hồ sơ';
      field('review').disabled = !view.hasReview;
      field('record').textContent = view.hasReview ? 'Xem bản ghi trận đã hoàn tất gần nhất trên thiết bị. Không phải tổng thành tích của hồ sơ.' : 'Chưa có trận đã hoàn tất trên thiết bị. Không hiển thị thành tích giả.';
    }
  };
}
