using CoAP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace ArchiveApi
{
    public class Memento : IEquatable<Uri>, IEquatable<string>
    {
        public bool IsFirst => webLink.Attributes.GetValues("rel").Contains("first");
        public bool IsLast => webLink.Attributes.GetValues("rel").Contains("last");
        public DateTime TimeArchived => DateTime.Parse(string.Join(" ", webLink.Attributes.GetValues("datetime")));
        public bool IsActuallyMemento => webLink.Attributes.GetValues("rel").Contains("memento");
        public Uri Url => new Uri(webLink.Uri);
        WebLink webLink;
        public Memento(WebLink memento)
        {
            webLink = memento;
        }
        public override bool Equals(object other)
        {
            return (other is WebLink) && ((other as WebLink) == webLink);
        }
        public static bool operator ==(Memento mem1, Memento mem2) { return mem1.Equals(mem2); }
        public static bool operator !=(Memento mem1, Memento mem2) { return !mem1.Equals(mem2); }
        public static bool operator ==(Memento mem1, string mem2) { return mem1.Equals(mem2); }
        public static bool operator !=(Memento mem1, string mem2) { return !mem1.Equals(mem2); }
        public bool Equals(Uri other) { return Url == other; }
        public bool Equals(string other) { return Url.ToString() == other; }
        public override int GetHashCode() => base.GetHashCode();
    }
    public class Mementos : IEnumerable
    {
        public string TimeGate { get; private set; }
        public TimeMap TimeMap { get; private set; }
        public string Original { get; private set; }
        public Memento FirstMemento => _mementos[0];
        public Memento LastMemento => _mementos[_mementos.Length - 1];
        Memento[] _mementos;
        public Mementos(IEnumerable<Memento> mementoList)
        {
            _mementos = (Memento[])mementoList;
            TimeGate = null;
            TimeMap = null;
            Original = null;
        }
        public Mementos(IEnumerable<WebLink> mementoList)
        {
            int mementoCount = (mementoList.Where(a => a.Attributes.GetValues("rel").Contains("memento"))).Count();
            _mementos = new Memento[mementoCount];
            for(int i = 0, i2 = 0; i < mementoList.Count(); i++)
            {
                var link = mementoList.ElementAt(i);
                if (link.Attributes.GetValues("rel").Contains("timegate"))
                    TimeGate = link.Attributes.GetValues("rel").First();
                else if (link.Attributes.GetValues("rel").Contains("timemap"))
                    TimeMap = new TimeMap(link);
                else if (link.Attributes.GetValues("rel").Contains("original"))
                    Original = link.Uri;
                else if (link.Attributes.GetValues("rel").Contains("memento"))
                {
                    _mementos[i2] = new Memento(link);
                    i2++;
                }
            }
        }
        public IEnumerator GetEnumerator()
        {
            return new MementoEnumerator(_mementos);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    public class MementoEnumerator : IEnumerator
    {
        public Memento[] mementos;
        int position = -1;
        public MementoEnumerator(Memento[] mementos)
        {
            this.mementos = mementos;
        }
        public Memento Current
        {
            get
            {
                try
                {
                    return mementos[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            position++;
            return (position < mementos.Length);
        }

        public void Reset()
        {
            position = -1;
        }
    }
}
