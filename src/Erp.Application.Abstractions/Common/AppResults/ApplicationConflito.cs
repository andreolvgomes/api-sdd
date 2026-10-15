namespace Erp.Application.Abstractions.Common.AppResults;

public record struct ApplicationConflito(string Mensagem) : IApplicationError;
