// Shared behaviour for every page: translations, language toggle, header navigation.
// window.BM = { language, translations } is written into the page by _Layout.cshtml.
(function () {
  'use strict';

  var BM = window.BM;
  var body = document.body;

  BM.t = function (key, values) {
    return BM.translations[BM.language][key].replace(/\{(\w+)\}/g, function (_, name) { return values[name]; });
  };

  // Other scripts (catalog.js) register callbacks here to refresh dynamic content on language change.
  BM.onLanguageChange = [];

  BM.applyLanguage = function (nextLanguage) {
    BM.language = nextLanguage;
    // Remember the choice so the next page is rendered by the server in the same language.
    document.cookie = 'bm_lang=' + nextLanguage + '; path=/; max-age=31536000; samesite=lax';
    document.documentElement.lang = BM.language;
    document.querySelectorAll('[data-i18n]').forEach(function (el) { el.textContent = BM.t(el.dataset.i18n); });
    document.querySelectorAll('[data-i18n-placeholder]').forEach(function (el) { el.placeholder = BM.t(el.dataset.i18nPlaceholder); });
    document.querySelectorAll('[data-i18n-aria-label]').forEach(function (el) { el.setAttribute('aria-label', BM.t(el.dataset.i18nAriaLabel)); });
    document.querySelectorAll('[data-error-key]').forEach(function (el) { el.textContent = el.dataset.errorKey ? BM.t(el.dataset.errorKey) : ''; });
    var languageToggle = document.getElementById('language-toggle');
    languageToggle.textContent = BM.language === 'en' ? 'БГ' : 'EN';
    languageToggle.setAttribute('aria-label', BM.t(BM.language === 'en' ? 'switchToBulgarian' : 'switchToEnglish'));
    document.title = BM.t(body.dataset.titleKey);
    BM.onLanguageChange.forEach(function (callback) { callback(); });
  };

  document.getElementById('language-toggle').addEventListener('click', function () {
    BM.applyLanguage(BM.language === 'en' ? 'bg' : 'en');
  });

  // On the home page the header/hero links scroll instead of reloading the page.
  if (body.dataset.page === 'home') {
    document.querySelectorAll('[data-nav="catalog"]').forEach(function (a) {
      a.addEventListener('click', function (e) {
        e.preventDefault();
        history.replaceState(null, '', location.pathname + location.search + '#catalog');
        document.getElementById('catalog').scrollIntoView({ behavior: 'smooth' });
      });
    });
    document.querySelectorAll('[data-nav="home"]').forEach(function (a) {
      a.addEventListener('click', function (e) {
        e.preventDefault();
        history.replaceState(null, '', location.pathname + location.search);
        window.scrollTo(0, 0);
      });
    });
  }
})();
