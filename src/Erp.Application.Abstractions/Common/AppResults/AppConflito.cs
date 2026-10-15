namespace Erp.Application.Abstractions.Common.AppResults;

public record struct AppConflito(string Mensagem) : IApplicationError;
