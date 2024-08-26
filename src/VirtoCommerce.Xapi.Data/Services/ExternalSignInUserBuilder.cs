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
    public virtual async Task BuildNewUser(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
    {
        if (user.MemberId is null && user.UserType == UserType.Customer.ToString())
        {
            var contact = AbstractTypeFactory<Contact>.TryCreateInstance();

            contact.Name = ResolveContactName(user, externalLoginInfo);
            contact.FirstName = ResolveContactFirstName(user, externalLoginInfo);
            contact.LastName = ResolveContactLastName(user, externalLoginInfo);
            contact.Emails = ResolveContactEmails(user, externalLoginInfo);

            if (!string.IsNullOrEmpty(user.StoreId))
            {
                var store = await storeService.GetNoCloneAsync(user.StoreId);
                contact.Status = store?.Settings.GetValue<string>(CustomerSettings.ContactDefaultStatus);
            }

            await memberService.SaveChangesAsync([contact]);

            user.MemberId = contact.Id;
        }
    }

    protected virtual string ResolveContactName(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
    {
        return externalLoginInfo.Principal.FindFirstValue("name");
    }

    protected virtual string ResolveContactFirstName(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
    {
        return externalLoginInfo.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
    }

    protected virtual string ResolveContactLastName(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
    {
        return externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
    }

    protected virtual string[] ResolveContactEmails(ApplicationUser user, ExternalLoginInfo externalLoginInfo)
    {
        return [user.Email];
    }
}
