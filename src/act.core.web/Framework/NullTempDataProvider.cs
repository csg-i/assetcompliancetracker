using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace act.core.web.Framework
{
    /// <summary>
    /// An implementation of the Temp Data Provider that basically will always break session state, to prevent its use.
    /// </summary>
   
    public class NullTempDataDictionary : ITempDataDictionary
    {
       public static readonly IDictionary<string, object> OnlyDict = new Dictionary<string, object>();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return OnlyDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
        }

        public void Clear()
        {
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return false;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {

        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return true;
        }

        public int Count
        {
            get { return 0; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(string key, object value)
        {
        }

        public bool ContainsKey(string key)
        {
            return false;
        }

        public bool Remove(string key)
        {
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            return false;
        }

        public object this[string key]
        {
            get { return null;}
            set {  }
        }

        public ICollection<string> Keys => OnlyDict.Keys;

        public ICollection<object> Values => OnlyDict.Values;

        public void Load()
        {
        }

        public void Save()
        {
        }

        public void Keep()
        {
        }

        public void Keep(string key)
        {
        }

        public object Peek(string key)
        {
            return null;
        }
    }
}