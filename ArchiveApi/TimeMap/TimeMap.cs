using ArchiveApi.Interfaces;
using CoAP;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
namespace ArchiveApi
{
    public class TimeMap
    {
        public DateTime From { get; private set; }
        public DateTime Until { get; private set; }
        public Uri TimeMapUri { get; private set; }
        #region Constructors
        public TimeMap(Uri TimeMapUri) => this.TimeMapUri = TimeMapUri;
        public TimeMap(string TimeMapUri) : this(new Uri(TimeMapUri)) { }
        public TimeMap(Uri TimeMapUri, DateTime From) : this(TimeMapUri) => this.From = From;
        public TimeMap(string TimeMapUri, DateTime From) : this(new Uri(TimeMapUri), From) { }
        public TimeMap(Uri TimeMapUri, DateTime From, DateTime Until) : this(TimeMapUri, From) => this.Until = Until;
        public TimeMap(string TimeMapUri, DateTime From, DateTime Until) : this(new Uri(TimeMapUri), From, Until) { }
        public TimeMap(WebLink web)
        {
            if (web.Attributes.GetValues("rel").Contains("self"))
            {
                Until = DateTime.Parse(string.Join(" ", web.Attributes.GetValues("until")));
                From = DateTime.Parse(string.Join(" ", web.Attributes.GetValues("from")));
                TimeMapUri = new Uri(web.Uri);
            }
            else
            {
                throw new Exception("Non timemap WebLink given");
            }
        }
        #endregion
        public static async Task<Mementos> GetMementosAsync(IArchiveService service, Uri url)
        {
            if (service == null || url == null)
            {
                throw new ArgumentNullException(service == null ? nameof(service) : nameof(url));
            }

            Mementos mementos = null;
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri(service.TimeMapEndpoint, url.ToString()));
                mementos = new Mementos(LinkFormat.Parse(await response.Content.ReadAsStringAsync()));
            }
            return mementos;
        }
        public static Mementos GetMementos(IArchiveService service, Uri url) => GetMementosAsync(service, url).Result;
    }
}