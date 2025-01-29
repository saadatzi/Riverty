using Riverty.Web.Models;

namespace Riverty.Web.Models;

public class RateViewModel
{
    public RateRequestModel Request { get; set; } = new RateRequestModel();
    public RateResponseModel? Response { get; set; } // Nullable since it may not always have a value
}