using CurlThin;
using CurlThin.Enums;
using CurlThin.Helpers;
using CurlThin.Native;
using CurlThin.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static CurlThin.CurlNative;

namespace Gambler.Bot.Core.Helpers
{
    internal class CurlThinHttpClient
    {
        private readonly CurlThin.SafeHandles.SafeEasyHandle _handle;
        private readonly StringBuilder _responseBuffer = new();

        public CurlThinHttpClient()
        {
            CurlNative.Init();
            _handle = CurlNative.Easy.Init();

            // Set options
            CurlNative.Easy.SetOpt(_handle, CURLoption.USERAGENT,
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:139.0) Gecko/20100101 Firefox/139.0");

            CurlNative.Easy.SetOpt(_handle, CURLoption.FORBID_REUSE, 0);
            CurlNative.Easy.SetOpt(_handle, CURLoption.FRESH_CONNECT, 0);
            CurlNative.Easy.SetOpt(_handle, CURLoption.CONNECTTIMEOUT, 10);
            CurlNative.Easy.SetOpt(_handle, CURLoption.TIMEOUT, 30);
            CurlNative.Easy.SetOpt(_handle, CURLoption.FOLLOWLOCATION, 1);

            // Enable in-memory cookie storage
            CurlNative.Easy.SetOpt(_handle, CURLoption.COOKIEFILE, "");

            // Setup write callback
            
        }

        public async Task<CurlResponse> GetAsync(string url)
        {
            _responseBuffer.Clear();

            CurlNative.Easy.SetOpt(_handle, CURLoption.URL, url);
            var dataCopier = new DataCallbackCopier();
            CurlNative.Easy.SetOpt(_handle, CURLoption.WRITEFUNCTION, dataCopier.DataHandler);
            return await Task.Run(() =>
            {
                var result = CurlNative.Easy.Perform(_handle);
                string content = Encoding.UTF8.GetString(dataCopier.Stream.ToArray());
                return new CurlResponse
                {
                    Content = _responseBuffer.ToString(),
                    StatusCode = CurlNative.Easy.GetInfo(_handle, CURLINFO.RESPONSE_CODE, out int tmp).ToString(),
                    IsSuccess = result == CURLcode.OK && (int)CurlNative.Easy.GetInfo(_handle, CURLINFO.RESPONSE_CODE, out int tmp2) < 400
                };
               

            });
        }

        public async Task<CurlResponse> PostAsync(string url, string postData)
        {
            _responseBuffer.Clear();
            // This one has to be called before setting COPYPOSTFIELDS.
            CurlNative.Easy.SetOpt(_handle, CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
            CurlNative.Easy.SetOpt(_handle, CURLoption.COPYPOSTFIELDS, postData);
            CurlNative.Easy.SetOpt(_handle, CURLoption.URL, url);
            var dataCopier = new DataCallbackCopier();
            CurlNative.Easy.SetOpt(_handle, CURLoption.WRITEFUNCTION, dataCopier.DataHandler);
            return await Task.Run(() =>
            {
                var result = CurlNative.Easy.Perform(_handle);
                string content = Encoding.UTF8.GetString(dataCopier.Stream.ToArray());
                return new CurlResponse
                {
                    Content = _responseBuffer.ToString(),
                    StatusCode = CurlNative.Easy.GetInfo(_handle, CURLINFO.RESPONSE_CODE, out int tmp).ToString(),
                    IsSuccess = result == CURLcode.OK && (int)CurlNative.Easy.GetInfo(_handle, CURLINFO.RESPONSE_CODE, out int tmp2) < 400
                };


            });
        }



        public void Dispose()
        {
            _handle?.Dispose();
            CurlNative.Cleanup();
        }
    }

    public class CurlResponse
    {
        public string Content { get; set; }
        public string StatusCode { get; set; }
        public bool IsSuccess { get; set; }
    }
}
