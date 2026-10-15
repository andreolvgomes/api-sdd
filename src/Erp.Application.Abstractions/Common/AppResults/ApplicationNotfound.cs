namespace Erp.Application.Abstractions.Common.AppResults;

public record struct ApplicationNotfound(string Mensagem) : IApplicationError;
