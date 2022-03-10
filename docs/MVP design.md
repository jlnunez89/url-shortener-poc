# MVP Requirements
These are the minimum requirements for the minimum-viable-product of this proof-of-concept:

- The PoC must support creating and deleting short URLs with associated long URLs.
- The PoC must support getting the long URL from a short URL.
- The PoC must support getting statistics on the number of times a short URL has been "clicked". This metric tracksthe number of times it's long URL has been retrieved, like in the above requirement.
- The PoC must support entering a custom short URL or letting the app randomly generate one during creation. It must maintain uniqueness of short URLs, in other words, picking an already existing short url identifier must not override the long url if one already exists.

# Proposed Solution:
Define an `IShortUrlManager` interface that will handle the CRUD operations (no update in the PoC MVP).

Implement such interface in `ShortUrlManagerSvc` which would be the underlying service interfacing our web service with the data/storage layer.

Our data/storage layer interface, `IShortUrlRepository`, will handle interfacing the service with the actual storage which stores `short urls` to the `long urls` and `retrieved count metric` mapping.

## Storage & Modeling Layer:
A simple implementation of the `IShortUrlRepository` will contain a `Dictionary<string, UrlInfo>` under the hood. This will suffice for storing our data for the PoC.

The `UrlInfo` type will store:
- The actual, long Url that is referenced by the short url.
- Metrics information for this short url, such as the number of times it has been requested.

The proposed schema for the `UrlInfo` class is:
```
class UrlInfo
{
    string ActualUrl { get; set; }
    MetricsInfo Metrics { get; set; }
}
```

And the proposed schema for the `MetricsInfo` object is:
```
class MetricsInfo
{
    ulong RequestedCount { get; set; }
}
```

> Caveat: `ulong` has a maximum value of `18,446,744,073,709,551,615`, after which it will overflow to `0`. 
>
> Hopefully that's enough to support our most popular short url. If not, then we would have to think about using (and storing) a data type that can handle arbitrarily large numbers, such as `BigInteger`.

--- 

## The IShortUrlManager interface:
The functions that our short url manager will support are:

| Operation | Signature | Remarks |
| -- | -- | -- |
| Create | `public ResultCode CreateShortUrl(string targetUrl, string desiredShortUrl = "")` | Analogous to an Http `POST /v1/short-url/` call, this method will attempt to create a new short url, storing it in our storage layer. If successful, it will return `ResultCode.Success` and, if unsuccessful, a `ResultCode` denoting the reason why. |
| Read | `public (ResultCode, string, ulong) GetMetricsForUrl(string shortUrl)` | Analogous to an Http `GET /v1/short-url/{identifier}` call, this method will attempt to retrieve the short url stored given it's `identifier` and, if successful, will return a tuple of `ResultCode.Success`, long url, and retrievedCount. If unsuccessful, a `ResultCode` is returned denoting the reason why, with the other fields in the tuple containing `default` values. |
| Update | `public ResultCode UpdateShortUrl(string shortUrl, string targetUrl)` | Analogous to an Http `PUT /v1/short-url/{identifier}` call. Our PoC is not expected to support this and thus, our actual implementation will throw a `NotSupportedException`. |
| Delete | `public ResultCode DeleteShortUrl(string shortUrl)` | Analogous to an Http `DELETE /v1/short-url/{identifier}` call, this method will attempt to delete a short url from our storage layer. If successful, it will return `ResultCode.Success`. If unsuccessful, a `ResultCode` denoting the reason why will be returned. |

--- 

## The `ResultCode` enumeration:
We mentioned the use of a `ResultCode` enumeration above. The values for it are defined as:

| ResultCode value | IsError? | Reason |
| -- | -- | -- |
| Success | False | - |
| AlreadyInUse | True | The desired url is already in use. |
| NotFound | True | The short url could not be found in the storage layer, so the operation cannot be performed. |


