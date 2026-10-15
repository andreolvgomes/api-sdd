namespace Erp.Application.Abstractions.Common.AppResults;

public record struct ApplicationValidation(string Mensagem) : IApplicationError { }
