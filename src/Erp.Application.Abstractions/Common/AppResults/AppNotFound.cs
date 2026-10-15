namespace Erp.Application.Abstractions.Common.AppResults;

public record struct AppNotFound(string Mensagem) : IApplicationError;
