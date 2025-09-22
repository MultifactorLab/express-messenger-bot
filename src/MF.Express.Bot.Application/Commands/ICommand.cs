namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Базовый интерфейс для команд CQRS
/// </summary>
/// <typeparam name="TRequest">Тип запроса команды</typeparam>
/// <typeparam name="TResponse">Тип ответа команды</typeparam>
public interface ICommand<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

