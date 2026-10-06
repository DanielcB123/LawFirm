namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

public sealed record ApiErrorResponse(
    string CorrelationId,
    ApiError Error);

public sealed record ApiError(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
