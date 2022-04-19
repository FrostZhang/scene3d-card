
using System.Collections;
using System.Linq;

namespace LitJson
{
    public class LitjsonIgnore : System.Attribute
    {

    }

    public static class JsonEx
    {
        public static JsonData First(this JsonData jd)
        {
            if (jd.IsObject)
            {
                var p = ((IDictionary)jd).GetEnumerator();
                p.MoveNext();
                return (JsonData)p.Value;
            }
            return null;
        }

        public static bool Contains(this JsonData jd, string prop)
        {
            if (jd.IsObject)
            {
                return ((IDictionary)jd).Contains(prop);
            }
            else if(jd.IsArray)
            {
                return ((IList)jd).Contains(prop);
            }
            return false;
        }
    }
}