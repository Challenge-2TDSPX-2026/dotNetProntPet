namespace ProntPet.Common;

/// <summary>Link HATEOAS: para onde ir (Href), o que ele representa (Rel) e como chamar (Method).</summary>
public record LinkDto(string Href, string Rel, string Method);

/// <summary>Nomes padronizados das relações (rel) usadas nos links.</summary>
public static class LinkRelations
{
    public const string Self = "self";
    public const string FirstPage = "first_page";
    public const string PrevPage = "prev_page";
    public const string NextPage = "next_page";
    public const string LastPage = "last_page";
}