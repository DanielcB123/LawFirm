namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Tenancy;

/// <summary>
/// The law firm resolved for the current request (ADR-002, plan step T1). Set once by firm-resolution
/// middleware from a verified domain and active placement; never from headers, body, query, or route.
/// Not implemented or registered yet: the application is single-firm.
/// </summary>
public interface IFirmContext
{
    bool IsResolved { get; }

    Guid FirmId { get; }

    int PlacementRoutingVersion { get; }
}
