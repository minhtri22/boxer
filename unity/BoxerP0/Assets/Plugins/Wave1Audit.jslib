mergeInto(LibraryManager.library, {
  Wave1AuditEnabled: function () {
    var query = new URLSearchParams(window.location.search);
    return query.get('desktop') === '1' && query.get('metrics') === '1' ? 1 : 0;
  },
  PublishWave1Snapshot: function (payload) {
    // Read-only, explicit synthetic-test instrumentation. No gameplay mutation API.
    window.boxerWave1Snapshot = JSON.parse(UTF8ToString(payload));
  }
});
