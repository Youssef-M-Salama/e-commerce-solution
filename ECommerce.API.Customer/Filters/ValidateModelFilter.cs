using ECommerce.API.Admin.Application.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(
                AppResponse<object>.ErrorResult(errors, StatusCodes.Status400BadRequest)
            );
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {

    }
}
