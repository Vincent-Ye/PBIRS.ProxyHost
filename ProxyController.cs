using Microsoft.IdentityModel.WindowsTokenService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SoftMatrix.PBIRS.ProxyHost
{
    /// <summary>
    /// Report Server 反向代理基类
    /// </summary>
    public class ProxyController : ApiController
    {

        /// <summary>
        /// 缺省模拟的AD账号，如果在accessToken没有传入，则缺省模拟这个账号
        /// </summary>
        const string DefaultUPN = "bi-user01@msftstack.com";


        /// <summary>
        /// 将应用的accessToken 转换为AD账号
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        protected string ConverTokenToUPN(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                //缺省的AD账号
                return DefaultUPN;
            }
            else
            {
                //TODO ,简化处理，假设 accessToken 的值就是AD账号
                //生产环境不能这样，要加上对accessToken 的校验
                return accessToken;
            }
        }

        /// <summary>
        /// Get Verb
        /// </summary>
        /// <param name="subPath">url中的路径</param>
        /// <param name="accessToken"> queryString中的accessToken</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Get(string subPath, string accessToken = null)
        {
            return await this.proxyRequest(subPath, accessToken);
        }

        /// <summary>
        /// Put Verb
        /// </summary>
        /// <param name="subPath">url中的路径</param>
        /// <param name="accessToken"> queryString中的accessToken</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Put(string subPath, string accessToken = null)
        {
            return await this.proxyRequest(subPath, accessToken);
        }

        /// <summary>
        /// Post Verb
        /// </summary>
        /// <param name="subPath"></param>
        /// <param name="accessToken"> queryString中的accessToken</param> 
        /// <returns></returns>
        public async Task<HttpResponseMessage> Post(string subPath, string accessToken = null)
        {
            return await proxyRequest(subPath, accessToken);
        }

        /// <summary>
        /// Delete Verb
        /// </summary>
        /// <param name="subPath">url中的路径</param>
        /// <param name="accessToken"> queryString中的accessToken</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Delete(string subPath, string accessToken = null)
        {
            return await this.proxyRequest(subPath, accessToken);
        }


        /// <summary>
        /// Head Verb
        /// </summary>
        /// <param name="subPath">url中的路径</param>
        /// <param name="accessToken"> queryString中的accessToken</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Head(string subPath, string accessToken = null)
        {
            return await this.proxyRequest(subPath, accessToken);
        }

        /// <summary>
        /// Options Verb
        /// </summary>
        /// <param name="subPath">url中的路径</param>
        /// <param name="accessToken"> queryString中的accessToken</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Options(string subPath, string accessToken = null)
        {
            return await this.proxyRequest(subPath, accessToken);
        }

        //不能启用 Trace Verb
        //否则 POST http://ow-win-04.msftstack.com:9000/powerbi/api/explore/reports/XXX/conceptualschema 会报下面的异常
        //Multiple actions were found that match the request: \r\nPost on type Ourway.PBIRS.ProxyHost.ProxyController\r\nTrace on type Ourway.PBIRS.ProxyHost.ProxyController
        ///// <summary>
        ///// Trace Verb
        ///// </summary>
        ///// <param name="subPath">url中的路径</param>
        ///// <param name="accessToken"> queryString中的accessToken</param>
        ///// <returns></returns>
        //public async Task<HttpResponseMessage> Trace(string subPath, string accessToken = null)
        //{
        //    return await this.proxyRequest(subPath, accessToken);
        //}

        /// <summary>
        /// 通用的请求代理处理
        /// </summary>
        /// <param name="subPath">url中的路径</param>
        /// <param name="accessToken">queryString中的accessToken</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> proxyRequest(string subPath, string accessToken)
        {
            var needSetCokkie = false;
            CookieHeaderValue accessTokenCookie = null;

            if (string.IsNullOrEmpty(accessToken))
            {
                CookieHeaderValue cookie = Request.Headers.GetCookies("accessToken").FirstOrDefault();
                if (cookie != null)
                {
                    accessToken = cookie["accessToken"].Value;
                }
            }
            else
            {
                needSetCokkie = true;

                accessTokenCookie = new CookieHeaderValue("accessToken", accessToken); ;
                accessTokenCookie.Expires = DateTimeOffset.Now.AddDays(1);
                accessTokenCookie.Domain = Request.RequestUri.Host;
                accessTokenCookie.Path = "/";
            }


            using (var id = S4UClient.UpnLogon(ConverTokenToUPN(accessToken)))
            {
                using (id.Impersonate())
                {
                    var handler = new System.Net.Http.HttpClientHandler() { UseDefaultCredentials = true };

                    using (HttpClient httpClient = new HttpClient(handler))
                    {
                        string url = ConfigurationManager.AppSettings["report-server-base-uri"] + this.Request.RequestUri.PathAndQuery;

                        this.Request.RequestUri = new Uri(url);

                        if (this.Request.Method == HttpMethod.Get)
                        {
                            this.Request.Content = null;
                        }
                        if (needSetCokkie)
                        {
                            var resp = httpClient.SendAsync(this.Request).Result;
                            resp.Headers.AddCookies(new CookieHeaderValue[] { accessTokenCookie });
                            return resp;
                        }
                        else
                        {
                            return await httpClient.SendAsync(this.Request);
                        }
                    }
                }
            }
        }
    }
}
