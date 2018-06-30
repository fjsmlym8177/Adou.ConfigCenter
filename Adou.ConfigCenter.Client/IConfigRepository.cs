using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Adou.ConfigCenter.Client
{
    public interface IConfigRepository
    {
        Task<T> Get<T>();

        Task<bool> Set<T>(T value);
    }
}
