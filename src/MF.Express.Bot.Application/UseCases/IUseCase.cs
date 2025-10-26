namespace MF.Express.Bot.Application.UseCases;

public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest botRequest, CancellationToken cancellationToken = default);
}

