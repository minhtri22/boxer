mergeInto(LibraryManager.library, {
  BoxerProfilePublish: function (payload) {
    if (window.boxerProfileView) window.boxerProfileView.render(JSON.parse(UTF8ToString(payload)));
  }
});
