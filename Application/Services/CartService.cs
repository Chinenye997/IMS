using Application.DTOs;
using Application.Extensions;
using Application.Interface;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class CartService : ICartInterface
    {
        public List<CartItem> GetCart(ISession session)
             => session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        public void SaveCart(ISession session, List<CartItem> cart)
            => session.SetObjectAsJson("Cart", cart);

        public async Task<List<CartItem>> AddToCartAsync(
            ISession session,
            IProductInterface productService,
            string productId,
            int quantity)
        {
            var cart = GetCart(session);
            var prod = await productService.GetProductById(productId);
            if (prod == null) throw new KeyNotFoundException("Product not found");

            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = prod.Id,
                    ProductName = prod.Name,
                    PhotoUrl = prod.PhotoUrls.FirstOrDefault(),
                    UnitPrice = prod.Price,
                    Quantity = System.Math.Min(quantity, prod.Quantity)
                });
            }
            else
            {
                item.Quantity = System.Math.Min(item.Quantity + quantity, prod.Quantity);
            }

            SaveCart(session, cart);
            return cart;
        }

        public void ClearCart(ISession session)
            => session.Remove("Cart");
    }
}