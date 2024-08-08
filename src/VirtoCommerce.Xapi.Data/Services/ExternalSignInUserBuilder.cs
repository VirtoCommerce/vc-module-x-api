using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.ExternalSignIn;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;
using CustomerSettings = VirtoCommerce.CustomerModule.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.Xapi.Data.Services;

public class ExternalSignInUserBuilder(IStoreService storeService, IMemberService memberService) : IExternalSignInUserBuilder
{
    public async Task BuildNewUser(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
    {
        if (user.MemberId is null && user.UserType == UserType.Customer.ToString())
        {
            var contact = AbstractTypeFactory<Contact>.TryCreateInstance();
            contact.Name = externalLoginInfo.Principal.FindFirstValue("name");
            contact.FirstName = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.GivenName);
            contact.LastName = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Surname);
            contact.Emails = [user.Email];

            if (!string.IsNullOrEmpty(user.StoreId))
            {
                var store = await storeService.GetNoCloneAsync(user.StoreId);
                contact.Status = store?.Settings.GetValue<string>(CustomerSettings.ContactDefaultStatus);
            }

            await memberService.SaveChangesAsync([contact]);

            user.MemberId = contact.Id;
        }
    }
}
