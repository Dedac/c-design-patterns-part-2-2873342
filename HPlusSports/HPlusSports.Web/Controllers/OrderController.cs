using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HPlusSports.Core;
using HPlusSports.Web.ViewModels;
using HPlusSports.Models;
using HPlusSports.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HPlusSports.Web.Controllers
{
    public class OrderController : Controller
    {
        ISimpleCachingOrderService _orderService;
        IRepository<Customer> _customerRepo;
        IRepository<Product> _productRepo;
        ISalesPersonRepository _salesRepo;
        public OrderController(ISimpleCachingOrderService orderService, IRepository<Customer> customerRepo, IRepository<Product> productRepo, ISalesPersonRepository salesRepo)
        {
            _orderService = orderService;
            _customerRepo = customerRepo;
            _productRepo = productRepo;
            _salesRepo = salesRepo;
        }

        public async Task<ActionResult> Index()
        {
            var vm = new OrderListViewModel();
            var orders = await _orderService.GetOrdersWithCustomers();
            
            vm.Orders = orders.Where( o => o.Status != "cancelled")
                .Select(o => new OrderItemViewModel(orders, o));
            return View(vm);
        }

        public async Task<ActionResult> Customer(int id)
        {
            var vm = new OrderListViewModel();
            var orders = await _orderService.GetCustomerOrders(id);
            vm.Orders = orders.Select(o => new OrderItemViewModel(orders, o));
            return View("Index", vm);
        }

        public async Task<ActionResult> Create()
        {
            var vm = new CreateOrderViewModel()
            {
                Products = (await _productRepo.GetAll())
                    .Select(p => new SelectListItem() { Text = $"{p.ProductName} - {p.Variety}", Value = p.ProductCode }).ToList(),
                Customers = (await _customerRepo.GetAll())
                    .Select(c => new SelectListItem() { Text = $"{c.FirstName} {c.LastName}", Value = c.Id.ToString() }).ToList(),
                SalesPeople = (await _salesRepo.GetAll())
                    .Select(s => new SelectListItem() { Text = $"{s.FirstName} {s.LastName}", Value = s.Id.ToString() }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder(CreateOrderViewModel order)
        {
            await _orderService.CreateOrder(order.CustomerId, order.SalesPersonId, order.SelectedProductCodes);
            return Redirect("Index");
        }

    }
}