using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.tuppleware
{

    public class TuppleWareWebClient
    {
        public async Task<string> Request(string endPoint, string query)
        {
            bool atBrown = false;
            if (atBrown)
            {
                var httpClient = new HttpClient();
                var content = await httpClient.GetStringAsync(endPoint + "/" + query);
                return content;
            }
            else
            {
                if (query.StartsWith("files"))
                {
                    return "{\"files\":[{\"id\":0,\"names\":[\"a0\",\"a1\"],\"types\":[\"float\",\"float\"]}]}";
                }
                else if (query.StartsWith("sample"))
                {
                    return "{\"samples\":[[0.5,0.5],[0.9,0.9],[2.1,2.1]]}";
                }
                else if (query.StartsWith("job"))
                {
                    return "{\"k\":[[0,0],[1,1],[2,2]],\"samples\":[[0.5,0.5],[0.9,0.9],[2.1,2.1],[2.5,2.5]]}";
                }
            }
            return "";
        }
    }
}
