using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Documents;

/// <summary>
/// Document binaries, versions, and preservation holds (gaps G32, G33; Markdown/modules/documents-and-evidence.md).
/// Not implemented: matter document metadata still lives in <c>Features/Matters</c>. Every route must go through
/// <c>IMatterAccessService</c> and <c>IDocumentStorage</c>.
/// </summary>
public sealed class DocumentsEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/documents-v2").WithTags("Documents").RequireAuthorization();
    }
}
