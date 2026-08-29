using GetProduct;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class ProductEndpoint
{
    public static void MapApplicationEndpoin(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/Product").WithTags("Product");
        group.MapGet("/", async (
            GetProductHandler handler,
            CancellationToken cancellationToken,
            int id) =>
        {
            var request = new Request(id);
            return await handler.Handle(request, cancellationToken);
        });
    }
}