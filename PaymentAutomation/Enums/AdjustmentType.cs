namespace PaymentAutomation.Enums;

public class AdjustmentType
{
    public static readonly AdjustmentType ABriggsPassportVisa = new("A Briggs Passport Visa");
    public static readonly AdjustmentType Bonus = new("Bonus");
    public static readonly AdjustmentType CoOp = new("CO-OP");
    public static readonly AdjustmentType CarnivalOverride = new("Carnival Override");
    public static readonly AdjustmentType Garnishment = new("Garnishment");
    public static readonly AdjustmentType GlobalTc = new("Global TC");
    public static readonly AdjustmentType MarketingContribution = new("Marketing Contribution");
    public static readonly AdjustmentType MarketingInvoice = new("Marketing Invoice");
    public static readonly AdjustmentType MerchantFee = new("Merchant Fee");
    public static readonly AdjustmentType Miscellaneous = new("Miscellaneous");
    public static readonly AdjustmentType NclNonCommFare = new("NCL Non Comm Fare");
    public static readonly AdjustmentType PortPromotions = new("Port Promotions");
    public static readonly AdjustmentType PreviousBalance = new("Previous Balance");
    public static readonly AdjustmentType ReferralFee = new("Referral Fee");
    public static readonly AdjustmentType ShipNShore = new("Ship N Shore");
    public static readonly AdjustmentType SpendingReimbursePromo = new("Spending Reimburse Promo");
    public static readonly AdjustmentType SupplierCheck = new("Supplier Check");
    public static readonly AdjustmentType Tg15PercentPromotion = new("TG 15 Percent Promotion");
    public static readonly AdjustmentType TsiBonus = new("TSI Bonus");
    public static readonly AdjustmentType WctCommissions = new("WCT Commissions");
    public static readonly AdjustmentType WthAdvance = new("WTH Advance");
    public static readonly AdjustmentType Unknown = new("Unknown");

    private static readonly AdjustmentType[] allAdjustments = new[]
    {
        ABriggsPassportVisa,
        Bonus,
        CoOp,
        CarnivalOverride,
        Garnishment,
        GlobalTc,
        MarketingContribution,
        MarketingInvoice,
        MerchantFee,
        Miscellaneous,
        NclNonCommFare,
        PortPromotions,
        PreviousBalance,
        ReferralFee,
        ShipNShore,
        SpendingReimbursePromo,
        SupplierCheck,
        Tg15PercentPromotion,
        TsiBonus,
        WctCommissions,
        WthAdvance,
        Unknown,
    };

    public readonly string Value;

    private AdjustmentType(string value) => Value = value;

    public static AdjustmentType FromValue(string value) =>
        allAdjustments.FirstOrDefault(t => t.Value == value)
            ?? throw new ArgumentException("Invalid value", nameof(value));
}
