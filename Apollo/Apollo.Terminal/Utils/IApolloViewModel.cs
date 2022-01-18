using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Terminal.Utils
{
    public delegate void ApolloViewModelShow();
    public interface IApolloViewModel
    {
        Task BeforeShowViewModel() { return Task.CompletedTask; }
    }
}
