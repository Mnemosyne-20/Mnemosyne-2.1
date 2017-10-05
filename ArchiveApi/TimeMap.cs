using CoAP;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArchiveApi
{
    public class TimeMap
    {
        Uri uri;
        public TimeMap(Uri uri)
        {
            this.uri = uri;
        }
        public async Task<IEnumerable<WebLink>> GetLinksForSiteAsync(Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                return LinkFormat.Parse((await client.GetAsync(new Uri(this.uri.ToString() + uri.ToString()))).ToString());
            }
        }
    }
}