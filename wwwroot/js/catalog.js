// Library catalog: search, status filter, paging, borrow and return.
// Data comes from the server (window.BM_CATALOG for the first page, /api/books afterwards).
(function () {
  'use strict';

  var BM = window.BM;
  var t = BM.t;
  var cfg = window.BM_CATALOG;

  var state = {
    items: cfg.page.items,
    total: cfg.page.total,
    libraryTotal: cfg.page.libraryTotal,
    free: cfg.page.free,
    page: cfg.page.page,
    pages: cfg.page.pages,
    query: '',
    filter: 'all'
  };

  var rowsEl = document.getElementById('rows');

  function esc(value) {
    return String(value).replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  }

  function fmt(d) {
    return ('0' + d.getDate()).slice(-2) + '.' + ('0' + (d.getMonth() + 1)).slice(-2) + '.' + d.getFullYear();
  }

  function render() {
    rowsEl.innerHTML = state.items.length ? state.items.map(function (b) {
      var out = b.isBorrowed;
      var action = out
        ? (cfg.canBorrow ? '<button class="row-btn ghost" data-act="return" data-id="' + b.id + '">' + t('return') + '</button>' : '')
        : (cfg.canBorrow ? '<button class="row-btn" data-act="borrow" data-id="' + b.id + '">' + t('borrow') + '</button>'
                          : '<a class="row-btn" href="' + esc(cfg.loginUrl) + '">' + t('loginToBorrow') + '</a>');
      return '<tr><td class="title">' + esc(b.title) + '</td><td>' + esc(b.author) +
        '</td><td class="muted">' + esc(b.publisher) + '</td><td class="muted">' + esc(b.year) + '</td>' +
        '<td><span class="status' + (out ? ' out' : '') + '">' + (out ? t('borrowed') : t('available')) + '</span></td>' +
        '<td class="muted">' + (out ? fmt(new Date(b.borrowedOn)) : '') + '</td>' +
        '<td class="muted">' + (out ? esc(b.borrowerName) : '') + '</td>' +
        '<td>' + action + '</td></tr>';
    }).join('') : '<tr><td class="empty" colspan="8">' + t('noMatch') + '</td></tr>';

    var numberLocale = BM.language === 'bg' ? 'bg-BG' : 'en-US';
    document.getElementById('meta').textContent = t('booksMeta', {
      shown: state.total.toLocaleString(numberLocale),
      total: state.libraryTotal.toLocaleString(numberLocale),
      free: state.free.toLocaleString(numberLocale)
    });
    document.getElementById('page-info').textContent = t('pageInfo', { page: state.page, pages: state.pages });
    document.getElementById('prev').disabled = state.page <= 1;
    document.getElementById('next').disabled = state.page >= state.pages;
  }

  // ---- loading pages from the server ----
  var requestId = 0;
  var controller = null;

  function load(page) {
    var mine = ++requestId;
    if (controller) controller.abort();
    controller = new AbortController();
    var url = cfg.apiUrl + '?search=' + encodeURIComponent(state.query) +
      '&filter=' + encodeURIComponent(state.filter) + '&page=' + page;
    fetch(url, { signal: controller.signal, headers: { 'Accept': 'application/json' }, credentials: 'same-origin' })
      .then(function (r) { if (!r.ok) throw new Error('HTTP ' + r.status); return r.json(); })
      .then(function (p) {
        if (mine !== requestId) return;
        state.items = p.items; state.total = p.total; state.libraryTotal = p.libraryTotal;
        state.free = p.free; state.page = p.page; state.pages = p.pages;
        render();
      })
      .catch(function (err) {
        if (err && err.name === 'AbortError') return;
        console.error(err);
      });
  }

  function apply() { load(1); }

  var timer;
  document.getElementById('search').addEventListener('input', function (e) {
    clearTimeout(timer); timer = setTimeout(function () { state.query = e.target.value; apply(); }, 80);
  });
  document.querySelectorAll('.chip').forEach(function (c) {
    c.addEventListener('click', function () {
      document.querySelectorAll('.chip').forEach(function (x) { x.setAttribute('aria-pressed', x === c); });
      state.filter = c.dataset.filter; apply();
    });
  });
  document.getElementById('prev').addEventListener('click', function () { load(state.page - 1); });
  document.getElementById('next').addEventListener('click', function () { load(state.page + 1); });

  // ---- borrow / return ----
  var dlg = document.getElementById('borrow-dialog');
  var borrowForm = document.getElementById('borrow-form');
  var current = null;
  var busy = false;

  function token() {
    return borrowForm.querySelector('input[name="__RequestVerificationToken"]').value;
  }

  function post(url, body) {
    return fetch(url, {
      method: 'POST',
      credentials: 'same-origin',
      headers: { 'Content-Type': 'application/json', 'Accept': 'application/json', 'RequestVerificationToken': token() },
      body: JSON.stringify(body || {})
    });
  }

  // Replaces one row with the server's version (the list itself is not re-filtered, as in the prototype).
  function applyChange(result) {
    state.items = state.items.map(function (b) { return b.id === result.book.id ? result.book : b; });
    state.free = result.free;
    render();
  }

  function failed(response) {
    // 409 = somebody else changed the book in the meantime: just reload the current page.
    if (response && response.status === 409) { load(state.page); return; }
    window.alert(t('requestFailed'));
  }

  rowsEl.addEventListener('click', function (e) {
    var btn = e.target.closest('button[data-act]'); if (!btn || busy) return;
    var id = +btn.dataset.id;
    var b = state.items.filter(function (x) { return x.id === id; })[0];
    if (btn.dataset.act === 'return') {
      busy = true;
      post(cfg.apiUrl + '/' + id + '/return')
        .then(function (r) { if (!r.ok) { failed(r); return; } return r.json().then(applyChange); })
        .catch(function (err) { console.error(err); failed(null); })
        .then(function () { busy = false; });
      return;
    }
    current = b;
    document.getElementById('borrow-book').textContent = b.title + ', ' + b.author;
    dlg.showModal();
  });

  document.getElementById('borrow-cancel').addEventListener('click', function () { dlg.close(); });

  borrowForm.addEventListener('submit', function (e) {
    e.preventDefault();
    if (!current || busy) return;
    busy = true;
    post(cfg.apiUrl + '/' + current.id + '/borrow')
      .then(function (r) {
        dlg.close();
        if (!r.ok) { failed(r); return; }
        return r.json().then(applyChange);
      })
      .catch(function (err) { console.error(err); dlg.close(); failed(null); })
      .then(function () { busy = false; });
  });

  BM.onLanguageChange.push(function () {
    render();
  });

  render();
})();
