
using Microsoft.AspNetCore.Mvc;

namespace ItemTTT.Views
{
	public abstract class BaseController : Controller
	{
		internal protected System.Exception NewUnexpectedException(string message)
		{
			return new Utils.TTTException( message );
		}

		internal protected IActionResult ObjectNotFound()
		{
			return NotFound();
		}

		internal protected IActionResult NotAuthenticated()
		{
			return NotFound();
		}

		internal protected IActionResult InvalidParameters()
		{
			return NotFound();
		}
	}
}
