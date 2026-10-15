namespace Erp.Application.Abstractions.Common.AppResults;

public record struct AppValidation(string Mensagem) : IApplicationError { }
