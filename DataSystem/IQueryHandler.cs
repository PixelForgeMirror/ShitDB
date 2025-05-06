using ShitDB.Util;

namespace ShitDB.DataSystem;

public interface IQueryHandler
{
    public Task<Result<List<string>, Exception>> Execute(string query);
}