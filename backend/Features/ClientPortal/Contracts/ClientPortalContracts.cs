namespace EnterpriseKnowledgeAssistant.Api.Features.ClientPortal;

public sealed record ClientMatterResponse(Guid Id, string MatterNumber, string Title, string PracticeArea, string Status);

public sealed record ClientDocumentResponse(Guid Id, string FileName, string DocumentType, DateTimeOffset UploadedAtUtc, string ReviewStatus);
