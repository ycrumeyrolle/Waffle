namespace Waffle.Filters
{
    /// <summary>Defines values that specify the order in which filters run within the same filter type and filter order.</summary>
    public enum FilterScope
    {
        /// <summary>Specifies an action before Handler.</summary>
        Global, 

        /// <summary>Specifies an order after Global.</summary>
        Handler = 10
    }
}