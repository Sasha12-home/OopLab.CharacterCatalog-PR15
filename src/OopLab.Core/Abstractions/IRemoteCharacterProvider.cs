using OopLab.Core.Models;

namespace OopLab.Core.Abstractions;

public interface IRemoteCharacterProvider
{
    string SourceName { get; }
    Task<IReadOnlyList<CharacterProfile>> FetchRemoteCharactersAsync(int? limit = null, CancellationToken cancellationToken = default);
    Task<CharacterProfile?> FetchCharacterByIdOrNameAsync(string idOrName, CancellationToken cancellationToken = default);
}
