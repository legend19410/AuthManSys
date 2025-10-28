using Microsoft.AspNetCore.Identity;

// Test if SignInManager is available
public class TestSignInManager
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public TestSignInManager(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<bool> TestSignIn(string username, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
        return result.Succeeded;
    }
}