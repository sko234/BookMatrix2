namespace BookMatrix.Services;

/// <summary>
/// Gives views the UI text in the visitor's language. The language is remembered in the
/// "bm_lang" cookie (written by the language toggle) and defaults to English.
/// </summary>
public class UiText
{
    public const string CookieName = "bm_lang";

    public UiText(IHttpContextAccessor accessor)
    {
        var value = accessor.HttpContext?.Request.Cookies[CookieName];
        Language = value == "bg" ? "bg" : "en";
    }

    public string Language { get; }

    public string this[string key] => Translations.Get(Language, key);
}
