namespace CVBuilder.Core.Interfaces
{
    public interface IDownloadTicketService
    {
        string Issue(int userId, int cvId, TimeSpan ttl);
        bool TryConsume(string token, out (int userId, int cvId) payload);
    }
}
