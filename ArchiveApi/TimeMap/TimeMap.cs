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
        /// <summary>
        /// <see cref="DateTime"/> of the first saved memento
        /// </summary>
        public DateTime From { get; private set; }
        /// <summary>
        /// <see cref="DateTime"/> of the last saved memento
        /// </summary>
        public DateTime Until { get; private set; }
        /// <summary>
        /// <see cref="Uri"/> of the timemap
        /// </summary>
        public Uri TimeMapUri { get; private set; }
        #region Constructors
        public TimeMap(Uri TimeMapUri) => this.TimeMapUri = TimeMapUri;
        public TimeMap(string TimeMapUri) : this(new Uri(TimeMapUri)) { }
        public TimeMap(Uri TimeMapUri, DateTime From) : this(TimeMapUri) => this.From = From;
        public TimeMap(string TimeMapUri, DateTime From) : this(new Uri(TimeMapUri), From) { }
        public TimeMap(Uri TimeMapUri, DateTime From, DateTime Until) : this(TimeMapUri, From) => this.Until = Until;
        public TimeMap(string TimeMapUri, DateTime From, DateTime Until) : this(new Uri(TimeMapUri), From, Until) { }
        public TimeMap(WebLink web) : this(new Uri(web.Uri))
        {
            if (web.GetAttrRelValues().Contains("self"))
            {
                Until = DateTime.Parse(string.Join(" ", web.GetAttrValues("until")));
                From = DateTime.Parse(string.Join(" ", web.GetAttrValues("from")));
            }
            else
            {
                throw new Exception("Non timemap WebLink given");
            }
        }
        #endregion
        /// <summary>
        /// Gets <see cref="Mementos"/> that an <see cref="IArchiveService"/> has made
        /// </summary>
        /// <param name="service">A <see cref="IArchiveService"/> that is used for getting mementos from a service</param>
        /// <param name="url">A <see cref="Uri"/> to get mementos for</param>
        /// <returns>A <see cref="Mementos"/> list for a <see cref="Uri"/></returns>
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