namespace GetProduct;

public record Response( string Name);

public record Request(int Id);

public class GetProductHandler
{
    private readonly AppDbcontext _context;

    public GetProductHandler(AppDbcontext context)
    {
        _context = context;
    }

    public async Task<Response> Handle(
        Request request,
        CancellationToken cancellationToken)
    {
        var product = await _context.products
            .FindAsync([request.Id], cancellationToken);

        if (product is null)
        {
            throw new Exception("Product not found");
        }

        return new Response(
            product.Name);
    }
}
