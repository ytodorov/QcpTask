using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QcpTask.Web.Pages
{
    public class IndexModel : PageModel
    {
        public string TestKey { get; set; }
        private readonly ILogger<IndexModel> _logger;

        private readonly IConfiguration _config;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public void OnGet()
        {
            TestKey = _config.GetValue<string>("testkey");
        }
    }
}