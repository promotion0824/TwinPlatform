using Azure;
using Azure.DigitalTwins.Core;
using System.Globalization;
using System.Text.Json;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Responses;

namespace Willow.AzureDigitalTwins.Services.Extensions;

public static class CollectionExtensions
{
    public static AsyncPageable<T> ToAsyncPageable<T>(this IEnumerable<T> items)
    {
        var page = Page<T>.FromValues(items.ToList().AsReadOnly(), null, null);
        return AsyncPageable<T>.FromPages(new List<Page<T>> { page });
    }

    public static Model.Adt.Page<T> ToPageModels<T>(this Page<T> page, string continuationToken = null)
    {
        if (page == null)
        {
            return null;
        }
        return new Model.Adt.Page<T> { Content = page.Values, ContinuationToken = continuationToken ?? page.ContinuationToken };
    }

    public static Model.Adt.Page<T> ExtractToPageModel<T>(this Page<JsonDocument> page, string selectPropertyName = null, string continuationToken = null)
    {
        if (page == null)
        {
            return null;
        }

        IEnumerable<T> extractedEntities = null;

        // If the select column is * we get the entire response deserialized
        if (selectPropertyName == "*" || selectPropertyName.StartsWith("top", StringComparison.InvariantCultureIgnoreCase))
        {
            extractedEntities = page.Values.Select(s => s.Deserialize<T>());
        }
        else
        {
            // if the select column is a valid property name we get that property from the response root and deserialize to target type T
            extractedEntities = page.Values.Select(s => s.RootElement.GetProperty(selectPropertyName).Deserialize<T>());
        }

        return new Model.Adt.Page<T> { Content = extractedEntities, ContinuationToken = continuationToken ?? page.ContinuationToken };
    }

    public static Model.Adt.Page<T> ExtractToPageModel<T>(this List<JsonDocument> docs, string selectPropertyName = null)
    {
        if (docs == null)
        {
            return null;
        }

        IEnumerable<T> extractedEntities = null;

        // If the select column is * we get the entire response deserialized
        if (selectPropertyName == "*")
        {
            extractedEntities = docs.Select(s => s.Deserialize<T>());
        }
        else
        {
            // if the select column is a valid property name we get that property from the response root and deserialize to target type T
            extractedEntities = docs.Select(s => s.RootElement.GetProperty(selectPropertyName).Deserialize<T>());
        }

        return new Model.Adt.Page<T> { Content = extractedEntities };
    }

    public static Model.Adt.Page<T> ToPageModel<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);

        return new Model.Adt.Page<T> { Content = items, ContinuationToken = pageNumber < totalPages ? (pageNumber + 1).ToString(CultureInfo.InvariantCulture) : null };
    }

    public async static Task<IEnumerable<T>> FetchAll<T>(this Model.Adt.Page<T> page, Func<Model.Adt.Page<T>, Task<Model.Adt.Page<T>>> getPage)
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
        Random rng = new();

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

    public static async Task<List<string>> ValidateAsync(this IEnumerable<IAzureDigitalTwinValidator> validators,
        BasicDigitalTwin twin,
        bool throwIfError
        )
    {
        List<string> errors = [];
        if(validators is null || !validators.Any())
        {
            return errors;
        }


        foreach(var validator in validators)
        {
            await validator.ValidateTwinAsync(twin, out errors);
        }

        if(throwIfError && errors.Count >0)
        {
            throw new TwinValidationException(twin.Id, errors);
        }

        return errors;
    }

}
