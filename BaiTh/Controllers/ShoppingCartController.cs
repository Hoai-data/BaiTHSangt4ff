using BaiTh.Data;
using BaiTh.Extensions;
using BaiTh.Models;
using BaiTh.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaiTh.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        //public ShoppingCartController(IProductRepository productRepository)
        //{
        //    _productRepository = productRepository;
        //}
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartController(ApplicationDbContext context,
       UserManager<ApplicationUser> userManager, IProductRepository
       productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Checkout()
        {
            return View(new Order());
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)

        {
            var cart =
           HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index");
            }
            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
            return View("OrderCompleted", order.Id); // Trang xác nhận hoàn 
         
 }
        //public async Task<IActionResult> AddToCart(int productId, int quantity)
        //{
        //    // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
        //    var product = await GetProductFromDatabase(productId);

        //    // Lấy URL hình ảnh đầu tiên hoặc một giá trị mặc định nếu không có hình ảnh
        //    var imageUrl = product.Images?.FirstOrDefault()?.Url ?? "/images/default-product-image.png";

        //    var cartItem = new CartItem
        //    {
        //        ProductId = productId,
        //        Name = product.Name,
        //        Price = product.Price,
        //        Quantity = quantity,
        //        ImageUrl = imageUrl // Đã thêm ImageUrl vào CartItem
        //    };

        //    // Lấy giỏ hàng hiện tại từ session hoặc tạo mới nếu chưa có
        //    var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

        //    // Thêm sản phẩm vào giỏ hàng
        //    cart.AddItem(cartItem);

        //    // Lưu giỏ hàng cập nhật vào session
        //    HttpContext.Session.SetObjectAsJson("Cart", cart);

        //    // Quay trở lại trang danh sách giỏ hàng
        //    return RedirectToAction("Index");
        //    // Note: Bạn có thể thay đổi "Index" thành bất kỳ hành động/controller nào bạn muốn người dùng quay lại sau khi thêm sản phẩm.
        //}

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }
        // Các actions khác...
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);

                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        //
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            var product = await GetProductFromDatabase(productId);

            // Lấy URL hình ảnh đầu tiên hoặc một giá trị mặc định nếu không có hình ảnh
            var imageUrl = product.ImageUrl ?? "/images/default-product-image.png";

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = imageUrl // Đã thêm ImageUrl vào CartItem
            };

            // Lấy giỏ hàng hiện tại từ session hoặc tạo mới nếu chưa có
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            // Thêm sản phẩm vào giỏ hàng
            cart.AddItem(cartItem);

            // Lưu giỏ hàng cập nhật vào session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Quay trở lại trang danh sách giỏ hàng
            return RedirectToAction("Index");
            // Note: Bạn có thể thay đổi "Index" thành bất kỳ hành động/controller nào bạn muốn người dùng quay lại sau khi thêm sản phẩm.
        }
        //
        public async Task<IActionResult> UpdateQuantityAsync(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.UpdateQuantity(productId, quantity);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
    }
}
