using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services
{
    public class LoadUserToEvalContextService : ILoadUserToEvalContextService
    {
        private readonly IMemberResolver _memberIdResolver;
        private readonly IMemberService _memberService;

        public LoadUserToEvalContextService(IMemberResolver memberIdResolver, IMemberService memberService)
        {
            _memberIdResolver = memberIdResolver;
            _memberService = memberService;
        }

        public virtual async Task SetShopperDataFromMember(EvaluationContextBase evalContextBase, string customerId)
        {
            if (customerId.IsNullOrEmpty())
            {
                return;
            }

            var member = await _memberIdResolver.ResolveMemberByIdAsync(customerId);
            if (member is Contact contact)
            {
                evalContextBase.ShopperGender = contact.GetDynamicPropertyValue("gender", string.Empty);

                if (contact.BirthDate != null)
                {
                    var zeroTime = new DateTime(1, 1, 1);
                    var span = DateTime.UtcNow - contact.BirthDate.Value;
                    evalContextBase.ShopperAge = (zeroTime + span).Year - 1;
                }

                evalContextBase.GeoTimeZone = contact.TimeZone;

                var userGroups = new List<string>();

                if (!evalContextBase.UserGroups.IsNullOrEmpty())
                {
                    userGroups.AddRange(evalContextBase.UserGroups);
                }

                userGroups.AddRange(contact.Groups?.ToArray());

                evalContextBase.UserGroups = userGroups.Distinct().ToArray();
            }
        }

        public async Task SetShopperDataFromOrganization(EvaluationContextBase evalContextBase, string organizationId)
        {
            if (organizationId.IsNullOrEmpty())
            {
                return;
            }

            var userGroups = new List<string>();

            if (!evalContextBase.UserGroups.IsNullOrEmpty())
            {
                userGroups.AddRange(evalContextBase.UserGroups);
            }

            var organizations = await _memberService.GetByIdsAsync([organizationId], MemberResponseGroup.WithGroups.ToString());
            userGroups.AddRange(organizations.OfType<Organization>().SelectMany(x => x.Groups));

            evalContextBase.UserGroups = userGroups.Distinct().ToArray();
        }
    }
}
