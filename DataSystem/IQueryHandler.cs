using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public interface IQueryHandler
{
    public Task<Result<List<TableRow>, Exception>> Execute(string query);
}