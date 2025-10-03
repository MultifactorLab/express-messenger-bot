namespace MF.Express.Bot.Application.Commands;

public interface ICommand<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

