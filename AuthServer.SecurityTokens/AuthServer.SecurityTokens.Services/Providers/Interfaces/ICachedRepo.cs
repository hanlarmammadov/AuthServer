using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.SecurityTokens.Services.Providers.Interfaces
{
    public interface ICachedRepo<TEntity>
    {
        Task Add(string key, TEntity entity, DateTime? expires = null);
        Task<bool> Exists(string key);
    }
}
