namespace MF.Express.Bot.Application.Queries;

/// <summary>
/// Базовый интерфейс для запросов CQRS
/// </summary>
/// <typeparam name="TRequest">Тип запроса</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public interface IQuery<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

