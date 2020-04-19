using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Bitbucket
{
    class JObjectParser
    {
        public static List<string> JObjectToSlugList(JObject response)
        {
            // Convert response to dynamic type
            dynamic responseDynamic = response;

            // Get number of pages in paginated response
            int resposneSize = responseDynamic.size;

            // Build list of values from response.
            var list = new List<string>();

            int i = 0;
            while (i < resposneSize)
            {
                list.Add((string)responseDynamic.values[i].slug);
                i++;
            }

            return list;
        }

        public static List<string> JObjectToUUIDList(JObject response)
        {
            // Convert response to dynamic type
            dynamic responseDynamic = response;

            // Get number of pages in paginated response
            int resposneSize = responseDynamic.size;

            // Build list of values from response.
            var list = new List<string>();

            int i = 0;
            while (i < resposneSize)
            {
                list.Add((string)responseDynamic.values[i].uuid);
                i++;
            }

            return list;
        }
    }
}
