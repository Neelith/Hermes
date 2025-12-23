using Hermes.Requests;

namespace Hermes.Webapi.Commands;

/// <summary>
/// Command without response - demonstrates ICommand handler
/// </summary>
public record DeleteUserCommand(int UserId) : ICommand;
