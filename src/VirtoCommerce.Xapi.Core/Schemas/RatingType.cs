
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class RatingType: ExtendableGraphType<ExpRating>
{
    public RatingType()
    {
        Name = "Rating";

        Field(x => x.Value, nullable: false).Description("Average rating");
        Field(x => x.ReviewCount, nullable: false).Description("Total count of customer reviews");
    }
}
