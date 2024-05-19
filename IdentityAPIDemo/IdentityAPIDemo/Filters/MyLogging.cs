using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace IdentityAPIDemo.Filters
{
    public class MyLogging : Attribute, IActionFilter
    {
        private readonly string _callerName;
        //private readonly ILogger<MyLogging> _logger;

        public MyLogging(string callerName)
        {
            _callerName = callerName;
            

        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //_logger.LogInformation($"Filter executed before");
            //Console.WriteLine($"Filter executed before");
            Log.Information($"Filter executed before");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //_logger.LogInformation($"Filter executed after");
            //Console.WriteLine($"Filter executed after");
            Log.Information($"Filter executed after");

        }
    }
}
