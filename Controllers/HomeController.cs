using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Data;
using ToDoListApp.Models;
using System.Threading.Tasks;

namespace ToDoListApp.Controllers
{
public class HomeController : Controller
{
    private readonly ToDoContext _context;

    public HomeController(ToDoContext context)
    {
        _context = context;
    }

            // Action to display the authentication page
        public IActionResult Auth()
        {
            return View(); // This view will show the login/register options
        }


    public async Task<IActionResult> Index()
    {
        var tasks = await _context.TaskItems.ToListAsync();
        return View(tasks); // Pass the tasks to the view
    }
}
}



