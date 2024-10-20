using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Azure;

namespace DigitalTwinCore.Extensions
{
    // Use this class when we want a tri-state struct that
    //  distinguishes between being unset, being set with null, or being set with non-null.
    // (C# does not allow Nullable<Nullable<T>> or T??)
    [Serializable]
    public struct NullableNullable<T> : IEquatable<NullableNullable<T>> where T : struct, IEquatable<T>
    {
        private T? _value;

        public bool HasValue { get; private set; }
        public bool IsExplicitlyNull() => HasValue && _value == null;

        public bool Equals([AllowNull] NullableNullable<T> other)
        {
            return this.HasValue == other.HasValue && this.Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            if (!HasValue || !_value.HasValue)
            {
                throw new KeyNotFoundException("NullableNullable without a value should not be used as a key");
            }
            return HashCode.Combine( _value.Value);
        }

        public T? Value
        {
            get => _value;

            set
            {
                HasValue = true;
                _value = value;
            }
        }
    }


    public static class StringExtensions
    {
        public static IDictionary<string, string> DefaultLangPropery(this string s)
        {
            var d = new Dictionary<string, string>();
            d.Add("en", s);
            return d;
        }
    }

    public static class JsonPatchDocumentExtensions
    { 
        public static Azure.JsonPatchDocument ConvertToAzureJsonPatchDocument(this Microsoft.AspNetCore.JsonPatch.JsonPatchDocument jsonDoc)
        {
            return new Azure.JsonPatchDocument(JsonSerializer.SerializeToUtf8Bytes(jsonDoc.Operations));
        }
    }

    public static class CollectionExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
        {
            return source.Select((x, i) => new { Index = i, Value = x })
                         .GroupBy(x => x.Index / size)
                         .Select(x => x.Select(v => v.Value).ToList());
        }

        public async static Task<IEnumerable<T>> FetchAll<T>(this Models.Page<T> page, Func<string, Task<Models.Page<T>>> fetchPage) 
        {
            var content = new List<T>();

            content.AddRange(page.Content);

            while (!string.IsNullOrEmpty(page.ContinuationToken))
            {
                page = await fetchPage(page.ContinuationToken);
                content.AddRange(page.Content);
            }

            return content;
        }

        // Below are copied from the ADT API /tree endpoint and will be removed once we switch to single-tenant.

        public static AsyncPageable<T> ToAsyncPageable<T>(this IEnumerable<T> items)
        {
            var page = Azure.Page<T>.FromValues(items.ToList().AsReadOnly(), null, null);
            return AsyncPageable<T>.FromPages(new List<Azure.Page<T>> { page });
        }

        public static Models.Page<T> ToPageModels<T>(this Page<T> page, string continuationToken = null)
        {
            if (page == null)
            {
                return null;
            }
            return new Models.Page<T> { Content = page.Values, ContinuationToken = continuationToken ?? page.ContinuationToken };
        }

        public static Models.Page<T> ExtractToPageModel<T>(this Page<JsonDocument> page, string selectPropertyName = null, string continuationToken = null)
        {
            if (page == null)
            {
                return null;
            }

            IEnumerable<T> extractedEntities = null;

            // If the select column is * we get the entire response deserialized
            if (selectPropertyName == "*")
            {
                extractedEntities = page.Values.Select(s => s.Deserialize<T>());
            }
            else
            {
                // if the select column is a valid property name we get that property from the response root and deserialize to target type T
                extractedEntities = page.Values.Select(s => s.RootElement.GetProperty(selectPropertyName).Deserialize<T>());
            }

            return new Models.Page<T> { Content = extractedEntities, ContinuationToken = continuationToken ?? page.ContinuationToken };
        }

        public static Models.Page<T> ToPageModel<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);

            return new Models.Page<T> { Content = items, ContinuationToken = pageNumber < totalPages ? (pageNumber + 1).ToString() : null };
        }

        public async static Task<IEnumerable<T>> FetchAll<T>(this Models.Page<T> page, Func<Models.Page<T>, Task<Models.Page<T>>> getPage)
        {
            var content = page.Content.ToList();
            while (!string.IsNullOrEmpty(page.ContinuationToken))
            {
                page = await getPage(page);
                content.AddRange(page.Content);
            }
            return content;
        }

        public static IList<T> ShuffleInPlace<T>(this IList<T> list)
        {
            Random rng = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
