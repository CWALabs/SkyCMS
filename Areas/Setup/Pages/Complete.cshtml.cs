public class CompleteModel : PageModel
{
    private readonly IDatabaseInitializationService dbInitService;
    private readonly IConfiguration configuration;

    public CompleteModel(
        IDatabaseInitializationService dbInitService,
        IConfiguration configuration)
    {
        this.dbInitService = dbInitService;
        this.configuration = configuration;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // ✅ Safe: Called within HTTP request scope
        var connectionString = configuration.GetConnectionString("ApplicationDbContextConnection");
        var result = await dbInitService.InitializeAsync(connectionString);
        
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        return RedirectToPage("/Index");
    }
}