using CoAP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace ArchiveApi
{
    public class Memento
    {
        public bool IsFirst => webLink.Attributes.GetValues("rel").Contains("first");
        public bool IsLast => webLink.Attributes.GetValues("rel").Contains("last");
        public DateTime TimeArchived { get => DateTime.Parse(string.Join(" ", webLink.Attributes.GetValues("datetime"))); }
        public bool IsActuallyMemento => webLink.Attributes.GetValues("rel").Contains("memento");
        WebLink webLink;
        public Memento(WebLink memento)
        {
            webLink = memento;
        }
    }
    public class Mementos : IEnumerable
    {
        public string TimeGate { get; private set; }
        public TimeMap TimeMap { get; private set; }
        public string Original { get; private set; }
        Memento[] _mementos;
        public Mementos(IEnumerable<Memento> mementoList)
        {
            _mementos = (Memento[])mementoList;
            TimeGate = null;
            TimeMap = null;
            Original = null;
        }
        internal Mementos(IEnumerable<WebLink> mementoList)
        {
            foreach (var link in mementoList)
            {
                if (link.Attributes.GetValues("rel").Contains("timegate"))
                    TimeGate = link.Attributes.GetValues("rel").First();
                else if (link.Attributes.GetValues("rel").Contains("timemap"))
                    TimeMap = new TimeMap(link);
                else if (link.Attributes.GetValues("rel").Contains("original"))
                    Original = link.Uri;
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
