using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerSide.Models.ParcelService;
using ServerSide.OptionsModels;

namespace ServerSide.Infrastructure.ParcelService
{
    public interface IParcelRestClient
    {
        Task<ParcelResponse> CheckUserCode(string code);

        Task<ParcelWithdrawResponse> WithdrawParcels(string code, List<string> parcelList);

        Task<AuthenticationToken> GetToken();
    }


    public class ParcelRestClient : IParcelRestClient
    {
        private readonly ParcelOptions _parcelOptions;
        private readonly IFlurlClient _flurlClient;
        protected readonly IMemoryCache _memoryCache;
        private readonly ILogger<ParcelRestClient> _logger;

        public ParcelRestClient(
            IOptions<ParcelOptions> options,
            HttpClient httpClient,
            IMemoryCache memoryCache,
            ILogger<ParcelRestClient> logger)
        {
            _parcelOptions = options.Value;

            _flurlClient = new FlurlClient(httpClient)
            {
                BaseUrl = _parcelOptions.BaseURL
            };

            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<ParcelResponse> CheckUserCode(string code)
        {
            var response = new ParcelResponse();
            try
            {
                var token = await GetToken();
                var responseJsonString = await _flurlClient.Request("code")
                    .SetQueryParam("code", code)
                    .SetQueryParam("branchID", _parcelOptions.BranchID)
                    .WithHeader("Authorization", "Bearer " + token.AaccessToken)
                    .GetStringAsync();

                var responseModel = JsonConvert.DeserializeObject<ParcelResponse>(responseJsonString);

                if (responseModel != null)
                    return responseModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return response;
        }

        public async Task<ParcelWithdrawResponse> WithdrawParcels(string code, List<string> parcelList)
        {
            var response = new ParcelWithdrawResponse() { Code = -1 };
            try
            {
                var token = await GetToken();

                var request = new ParcelWithdrawRequest()
                {
                    Code = code,
                    BranchID = 10,
                    ItemIDs = parcelList.ToArray()
                };

                var responseJsonString = await _flurlClient.Request("item/IssueParcels")
                    .WithHeader("Authorization", "Bearer " + token.AaccessToken)
                    .PostJsonAsync(request)
                    .ReceiveString();

                var responseModel = JsonConvert.DeserializeObject<ParcelWithdrawResponse>(responseJsonString);

                if (responseModel != null)
                {
                    if (responseModel.Code != 100)
                        _logger.LogError($"Code: {responseModel.Code}, Message: {responseModel.Message}");

                    return responseModel;
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return response;
        }


        #region Authorize

        public async Task<AuthenticationToken> GetToken()
        {
            var cacheKey = "Token";

            if (!_memoryCache.TryGetValue(cacheKey, out AuthenticationToken token))
            {
                var result = await AuthorizeAsync();

                _memoryCache.Set(cacheKey, result);

                token = result;

                return token;
            }
            else
            {
                if (token.Expires <= DateTime.Now)
                {
                    _logger.LogInformation("Token Outdated, Refreshing...");
                    //Refresh
                    var result = await RefreshAuthorizeAsync(token.RefreshToken);

                    _memoryCache.Set(cacheKey, result);
                    return result;
                }
                else
                    return token;
            }
        }
        private async Task<AuthenticationToken> AuthorizeAsync()
        {
            try
            {
                var response = await _flurlClient.Request("token")
                                           .PostUrlEncodedAsync(
                                              new
                                              {
                                                  username = _parcelOptions.UserName,
                                                  password = _parcelOptions.Password,
                                                  grant_type = "password",
                                              }
                                           )
                                           .ReceiveString();

                AuthenticationToken? token = JsonConvert.DeserializeObject<AuthenticationToken>(response);
                if (token == null)
                {
                    _logger.LogError("Cant Get Token");
                    throw new Exception("Cant Get Token!");
                }

                //Expire In UTC
                token.Expires += TimeZoneInfo.Local.GetUtcOffset(token.Expires);
                token.Issued += TimeZoneInfo.Local.GetUtcOffset(token.Issued);

                return token;
            }
            catch (FlurlHttpException ex)
            {
                var message = await ex.GetResponseStringAsync();
                _logger.LogError(message ?? ex.Message ?? "Can't Get Token");
                throw new Exception("Can't Get Token!");
            }

            catch (Exception ex)
            {
                _logger.LogError($"Get Token Exception: {ex.Message}");
                throw new Exception("Can't Get Token!");
            }
        }
        private async Task<AuthenticationToken> RefreshAuthorizeAsync(string refreshToken)
        {
            AuthenticationToken? token;
            try
            {
                var response = await _flurlClient.Request("token")
                                           .PostUrlEncodedAsync(
                                              new
                                              {
                                                  refresh_token = refreshToken,
                                                  grant_type = "refresh_token",
                                              }
                                           )
                                           .ReceiveString();

                token = JsonConvert.DeserializeObject<AuthenticationToken>(response);
                if (token == null)
                {
                    _logger.LogError("Cant Refresh Token");

                    token = await AuthorizeAsync();
                    return token;
                }

                //Dates In UTC               
                token.Expires += TimeZoneInfo.Local.GetUtcOffset(token.Expires);
                token.Issued += TimeZoneInfo.Local.GetUtcOffset(token.Issued);

                return token;
            }
            catch (FlurlHttpException ex)
            {
                var message = await ex.GetResponseStringAsync();
                _logger.LogError(message ?? "Cant Refresh Token");

                token = await AuthorizeAsync();
                return token;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Refresh Token Exception: {ex.Message}");
                token = await AuthorizeAsync();
                return token;
            }
        }

        #endregion
    }
}
