using System;
using System.Collections.Generic;
using System.Linq;

namespace WebLoader.ConsoleApp
{
    public class ProductApiVariantsSpecification : BasicRunSpecification
    {
        private static readonly IEnumerable<int> ValidVariantIds = new List<int>
        {
            509398,
            1000065,
            1027456,
            1082828,
            1086506,
            1086507,
            1103324,
            1125629,
            1127440,
            1137002,
            1141051,
            1152952,
            1156908,
            1157616,
            1171103,
            1181535,
            1184810,
            1254370,
            1263339,
            1353520,
            1372535,
            1384760,
            1386226,
            1387080,
            1449827,
            1467384,
            1496655,
            1512940,
            1536716,
            1588588,
            1601024,
            1601026,
            1601027,
            1630247,
            1633860,
            1641656,
            1641657,
            1644753,
            1659065,
            1662830,
            1662842,
            1715365,
            1716933,
            1718094,
            1722554,
            1731844,
            1731848,
            1734261,
            1770895,
            1787257,
            1833220,
            1843754,
            1843765,
            1845358,
            1860508,
            1860642,
            1860781,
            1860783,
            1860784,
            1868349,
            1877546,
            1879580,
            1882739,
            1902016,
            1929859,
            1953144,
            1959069,
            1960013,
            2033971,
            2037135,
            2047253,
            2047260,
            2061792,
            2062534,
            2094566,
            2166551,
            2174346,
            2192963,
            2235627,
            2238309,
            2249863,
            2254955,
            2264144,
            2272296,
            2292039,
            2314965,
            2315932,
            2330819,
            2334299,
            2354069,
            2362759,
            2369002,
            2373644,
            2374283,
            2374420,
            2374469,
            2374480,
            2374496,
            2374515,
            2379432,
            2381377,
            2383507,
            2384475,
            2386432,
            2404845,
            2406584,
            2429016,
            2476774
        };

        public ProductApiVariantsSpecification(
            int numberOfSeconds,
            int startingRequestCount,
            int maxRequestCount,
            string baseUrl,
            int requestTimeout,
            string verb,
            string content,
            IDictionary<string, string> requestHeaders = null)
            : base(
                numberOfSeconds, startingRequestCount, maxRequestCount, baseUrl, requestTimeout, verb, content,
                requestHeaders)
        {
        }


        public string GenerateRelativeUrl(string store = "COM", string currency = "GBP", string sizeSchema = "UK")
        {
            var variantIds = Generate(2);
            return "/product/catalogue/v1/variants/" +
                   $"?variantIds={string.Join(",", variantIds)}" +
                   $"&store={store}&currency={currency}&sizeSchema={sizeSchema}";
        }

        private static IEnumerable<int> Generate(int count)
        {
            return ValidVariantIds.OrderBy(i => Guid.NewGuid()).Take(count);
        }
    }
}