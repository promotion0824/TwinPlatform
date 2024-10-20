Willow.Batch
====

The Willow.Batch library implements the common DTOs and methods needed to handle filtred, sorted and paginated responses in APIs. It follows the pattern common for MUI grid but can also apply to server-side API calls.

The key DTOs are `SortSpecificationDTO` and `FilterSpecificationDTO`. These are both closely related to the DTOs used by MUI Grid. Our approach to pagination is page numbers and page counts and not continuation tokens.

Various helper methods are also being developed here to assist with converting Sort and Filter DTOs into .NET Expressions for use with EF Core.

For usage, see InsightCore that uses this library.
