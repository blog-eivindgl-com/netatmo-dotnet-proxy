﻿namespace NetatmoProxy.Services
{
    public interface IAccessTokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
