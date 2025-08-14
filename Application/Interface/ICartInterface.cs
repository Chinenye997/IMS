

using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interface
{
    public interface ICartInterface
    {
        // Manages cart operations using session

        List<CartItem> GetCart(ISession session);
        void SaveCart(ISession session, List<CartItem> cart);
        Task<List<CartItem>> AddToCartAsync(ISession session, IProductInterface productService,
                                           string productId, int quantity);
        void ClearCart(ISession session);
    }
}
