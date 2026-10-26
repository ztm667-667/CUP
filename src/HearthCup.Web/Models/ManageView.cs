using HearthCup.Web.Models;

namespace HearthCup.Web.Models;

/// <summary>Manager dashboard: the product page plus the counts that tell the manager what needs doing.</summary>
public sealed record ManageView(BrowseView Catalogue, int OrderCount, int PendingCount);
