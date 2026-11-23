// Client-side checks complement the server-side account validation.
(function () {
  'use strict';

  var t = window.BM.t;
  var EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  function setError(id, errorKey) {
    var error = document.getElementById(id + '-error');
    error.dataset.errorKey = errorKey;
    error.textContent = errorKey ? t(errorKey) : '';
    document.getElementById(id).classList.toggle('invalid', !!errorKey);
    return !errorKey;
  }

  // ---- Sign up ----
  var form = document.getElementById('signup-form');
  if (form) {
    var fields = ['name', 'email', 'password', 'confirm'];

    form.addEventListener('submit', function (e) {
      var v = {};
      fields.forEach(function (f) { v[f] = document.getElementById(f).value; });

      var ok = true;
      ok = setError('name', v.name.trim() ? '' : 'nameError') && ok;
      ok = setError('email', EMAIL.test(v.email) ? '' : 'emailError') && ok;

      var strong = v.password.length >= 6 && /[A-Z]/.test(v.password) &&
                   /[a-z]/.test(v.password) && /\d/.test(v.password) && /[^A-Za-z0-9]/.test(v.password);
      ok = setError('password', strong ? '' : 'passwordError') && ok;
      ok = setError('confirm', v.confirm === v.password && v.confirm ? '' : 'confirmError') && ok;

      if (!ok) e.preventDefault();
    });
  }

  // ---- Log in ----
  var loginForm = document.getElementById('login-form');
  if (loginForm) {
    loginForm.addEventListener('submit', function (e) {
      var email = document.getElementById('login-email').value;
      var pass = document.getElementById('login-password').value;
      var ok = true;
      ok = setError('login-email', EMAIL.test(email) ? '' : 'emailError') && ok;
      ok = setError('login-password', pass ? '' : 'loginPasswordError') && ok;
      if (!ok) e.preventDefault();
    });
  }
})();
