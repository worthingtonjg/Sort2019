using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public static class SessionStorage
    {
        public static Task<T> GetAsync<T>(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<T>("blazorSessionStorage.get", key);

        public static Task SetAsync(IJSRuntime jsRuntime, string key, object value)
            => jsRuntime.InvokeAsync<object>("blazorSessionStorage.set", key, value);

        public static Task DeleteAsync(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<object>("blazorSessionStorage.delete", key);
    }
}